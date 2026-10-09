using System.Text;
using System.Text.Json;
using System.Threading;
using AlgoVis.Parser.Parsing;
using AlgoVis.Yawa.Yawa;
using AlgoVis.Yawa.Yawa.Expressions;
using AlgoVis.Yawa.Yawa.Statements;

namespace AlgoVis.Transpiler.Cpp;

/// <summary>
/// Транспайлер C++ → YAWA.
/// MVP: main(), переменные, if/while/for, функции, массивы, vector, cout.
/// </summary>
public sealed class CppToYawa
{
    private readonly string _source;
    private readonly byte[] _bytes;
    private readonly List<string> _warnings = new();
    private readonly HashSet<string> _functionNames = new(StringComparer.Ordinal);
    public CppToYawa(string source)
    {
        _source = source;
        _bytes = Encoding.UTF8.GetBytes(source);
    }

    public IReadOnlyList<string> Warnings => _warnings;

    // ─────────── Точка входа ───────────

    public YawaProgram Transpile()
    {
        var parser = new CppParser();
        using var tree = parser.Parse(_source);
        var root = tree.Root;

        if (root.Type != "translation_unit")
            throw new UnsupportedFeatureException(
                $"Ожидался 'translation_unit', получено '{root.Type}'");

        var program = new YawaProgram
        {
            YawaVersion = "1.0",
            Metadata = new YawaMetadata
            {
                Name = "C++ → YAWA",
                Generator = "algovis-transpiler-cpp@0.1"
            }
        };

        // Собираем имена функций (для последующей трансляции вызовов)
        foreach (var child in root.NamedChildren())
        {
            if (child.Type == "function_definition")
            {
                var name = ExtractFunctionName(child);
                if (!string.IsNullOrEmpty(name)) _functionNames.Add(name);
            }
        }

        // Конвертируем top-level
        foreach (var child in root.NamedChildren())
        {
            switch (child.Type)
            {
                case "function_definition":
                    program.Functions.Add(ConvertFunction(child));
                    break;

                case "preproc_include":
                case "preproc_def":
                case "preproc_ifdef":
                case "using_declaration":
                case "namespace_definition":
                case "comment":
                case "declaration":
                    // Пропускаем: include, using, forward declarations
                    break;

                case "linkage_specification":
                    // extern "C" { ... } — пропускаем
                    break;

                default:
                    _warnings.Add($"Пропущен top-level: {child.Type}");
                    break;
            }
        }

        // Ищем main
        var mainFn = program.Functions.FirstOrDefault(f => f.Name == "main")
            ?? throw new UnsupportedFeatureException(
                "Не найдена функция main(). C++ программы должны иметь int main().");

        program.Entry = new YawaEntry
        {
            Function = "main",
            Args = new List<YawaExpression>()
        };

        program.Limits = new YawaLimits
        {
            MaxSteps = 1_000_000,
            MaxDepth = 500,
            MaxSeconds = 10,
            SnapshotEvery = 0
        };

        return program;
    }

    // ─────────── Functions ───────────

    private YawaFunction ConvertFunction(TsNode node)
    {
        var name = ExtractFunctionName(node);
        if (string.IsNullOrEmpty(name))
            throw Err("Функция без имени", node);

        var fn = new YawaFunction { Name = name };

        // Параметры
        var declarator = FindChildByType(node, "function_declarator");
        if (declarator.IsValid)
        {
            var paramList = FindChildByType(declarator, "parameter_list");
            if (paramList.IsValid)
            {
                foreach (var p in paramList.NamedChildren())
                {
                    if (p.Type == "parameter_declaration")
                    {
                        // Ищем identifier в ЛЮБОМ вложенном declarator:
                        //   int n               → identifier напрямую
                        //   int A[]             → array_declarator → identifier
                        //   int *p              → pointer_declarator → identifier
                        //   const vector<int>& v → reference_declarator → identifier
                        var name1 = FindDeepIdentifier(p);
                        if (!string.IsNullOrEmpty(name1))
                            fn.Params.Add(new YawaParam { Name = name1 });
                    }
                }
            }
        }

        // Тело
        var body = FindChildByType(node, "compound_statement");
        if (body.IsValid)
            fn.Body = ConvertBlock(body);

        return fn;
    }

    private string ExtractFunctionName(TsNode funcDef)
    {
        var declarator = FindChildByType(funcDef, "function_declarator");
        if (!declarator.IsValid) return "";
        var id = FindChildByType(declarator, "identifier");
        return id.IsValid ? Text(id) : "";
    }

    // ─────────── Blocks & Statements ───────────

    private List<YawaStatement> ConvertBlock(TsNode block)
    {
        var result = new List<YawaStatement>();
        foreach (var stmt in block.NamedChildren())
        {
            var s = ConvertStatement(stmt);
            if (s is not null) result.Add(s);
        }
        return result;
    }

    private YawaStatement? ConvertStatement(TsNode node)
    {
        switch (node.Type)
        {
            case "compound_statement":
                // Вложенный блок — оборачиваем в try-подобную структуру? Нет —
                // просто разворачиваем statements в плоский список через временную
                // группу. Здесь для простоты возвращаем "if (true) { ... }".
                return new IfStatement
                {
                    Cond = Lit(true),
                    Then = ConvertBlock(node)
                };

            case "declaration":
                return ConvertDeclaration(node);

            case "expression_statement":
                {
                    var inner = node.NamedChildren().FirstOrDefault();
                    if (!inner.IsValid) return null;

                    // cout << ... — специальный случай
                    if (inner.Type == "binary_expression" &&
                        IsCoutChain(inner))
                    {
                        return ConvertCout(inner);
                    }

                    // cin >> ... — специальный случай
                    if (inner.Type == "binary_expression" &&
                        IsCinChain(inner))
                    {
                        // Просто пропускаем — мы не можем читать stdin в реальном времени
                        // (для олимпиадных задач нужно подавать данные — это отдельная фича)
                        _warnings.Add("cin >> ... — чтение ввода игнорируется");
                        return null;
                    }

                    // Присваивание
                    if (inner.Type == "assignment_expression")
                        return ConvertAssignment(inner);

                    // Обновление (i++, ++i, i += 1, ...)
                    if (inner.Type == "update_expression")
                        return ConvertUpdate(inner);

                    // Просто вызов (например, swap(a, b))
                    return new ExprStatement { Value = ConvertExpr(inner) };
                }

            case "return_statement":
                {
                    var val = node.NamedChildren().FirstOrDefault();
                    return new ReturnStatement
                    {
                        Value = val.IsValid ? ConvertExpr(val) : null
                    };
                }

            case "if_statement":
                return ConvertIf(node);

            case "while_statement":
                return ConvertWhile(node);

            case "for_statement":
                return ConvertFor(node);

            case "range_based_for_statement":
                return ConvertRangeFor(node);

            case "break_statement":
                return new BreakStatement();

            case "continue_statement":
                return new ContinueStatement();

            case "comment":
                return null;

            default:
                throw Err($"Неподдерживаемый statement: {node.Type}", node);
        }
    }

    // ─────────── Declarations ───────────

    private YawaStatement ConvertDeclaration(TsNode node)
    {
        // declaration:
        //   int x = 5;            → init_declarator
        //   int x, y;             → identifier, identifier
        //   vector<int> v = {...}; → init_declarator с initializer_list
        //   int A[5];             → array_declarator
        var statements = new List<YawaStatement>();

        // Определяем "тип" — нужен для vector/array
        var typeNode = node.NamedChildren().FirstOrDefault(c =>
            c.Type is "primitive_type" or "type_identifier" or "template_type");
        var typeName = typeNode.IsValid ? Text(typeNode) : "";

        foreach (var child in node.NamedChildren())
        {
            if (child.Type == "init_declarator")
            {
                var parts = child.NamedChildren().ToList();
                if (parts.Count < 2) continue;
                var target = parts[0];
                var value = parts[^1];

                // Имя переменной — последний identifier в target-е (в array_declarator это [N])
                var declName = ExtractDeclaratorName(target);
                if (string.IsNullOrEmpty(declName)) continue;

                // Инициализация из списка — массив
                if (value.Type == "initializer_list")
                {
                    statements.Add(new DeclareStatement
                    {
                        Name = declName,
                        Value = ConvertInitializerList(value)
                    });
                }
                else if (IsVectorType(typeName) && value.Type == "call_expression")
                {
                    // vector<int> v(n, 0) — на будущее
                    statements.Add(new DeclareStatement
                    {
                        Name = declName,
                        Value = ConvertExpr(value)
                    });
                }
                else
                {
                    statements.Add(new DeclareStatement
                    {
                        Name = declName,
                        Value = ConvertExpr(value)
                    });
                }
            }
            else if (child.Type == "identifier")
            {
                statements.Add(new DeclareStatement
                {
                    Name = Text(child),
                    Value = Lit(null)
                });
            }
            else if (child.Type == "array_declarator")
            {
                // int A[5];           → [0,0,0,0,0]
                // int M[3][3];        → [[0,0,0],[0,0,0],[0,0,0]]
                // int A[];            → []
                // int M[][3];         → (в параметрах, но на всякий случай)
                var declName = ExtractDeclaratorName(child);
                if (string.IsNullOrEmpty(declName)) continue;

                // Ищем initializer_list внутри array_declarator (для int A[] = {...})
                var initList = FindChildByType(child, "initializer_list");
                if (initList.IsValid)
                {
                    statements.Add(new DeclareStatement
                    {
                        Name = declName,
                        Value = ConvertInitializerList(initList)
                    });
                    continue;
                }

                // Иначе — фиксированный размер (может быть многомерным)
                var sizes = new List<int>();
                CollectArraySizes(child, sizes);

                if (sizes.Count == 0)
                {
                    statements.Add(new DeclareStatement
                    {
                        Name = declName,
                        Value = new ArrayExpr { Items = new List<YawaExpression>() }
                    });
                }
                else
                {
                    statements.Add(new DeclareStatement
                    {
                        Name = declName,
                        Value = BuildNestedArray(sizes, 0)
                    });
                }
            }
        }

        return statements.Count switch
        {
            0 => new ExprStatement { Value = Lit(null) },
            1 => statements[0],
            _ => new IfStatement
            {
                Cond = Lit(true),
                Then = statements
            }
        };
    }

    private string ExtractDeclaratorName(TsNode declarator)
    {
        switch (declarator.Type)
        {
            case "identifier":
                return Text(declarator);

            case "init_declarator":
                return ExtractDeclaratorName(declarator.NamedChildren().First());

            case "array_declarator":
            case "pointer_declarator":
            case "reference_declarator":
                // Используем рекурсивный поиск: identifier может быть на любом уровне
                return FindDeepIdentifier(declarator);

            default:
                return FindDeepIdentifier(declarator);
        }
    }

    private bool IsVectorType(string typeName) =>
        typeName.Contains("vector") || typeName.Contains("array");

    private YawaExpression ConvertInitializerList(TsNode node)
    {
        var items = node.NamedChildren()
            .Where(c => c.Type != "comment")
            .Select(ConvertExpr)
            .ToList();
        return new ArrayExpr { Items = items };
    }

    // ─────────── If / While / For ───────────

    private YawaStatement ConvertIf(TsNode node)
    {
        var children = node.NamedChildren().ToList();

        var cond = children.FirstOrDefault(c =>
            c.Type != "compound_statement" &&
            c.Type != "else_clause");
        var thenBlock = children.FirstOrDefault(c => c.Type == "compound_statement");

        // condition_clause — разворачиваем
        var condExpr = UnwrapConditionClause(cond);

        var stmt = new IfStatement
        {
            Cond = ConvertExpr(condExpr),
            Then = thenBlock.IsValid ? ConvertBlock(thenBlock) : new List<YawaStatement>()
        };

        var elseClause = children.FirstOrDefault(c => c.Type == "else_clause");
        if (elseClause.IsValid)
        {
            var ec = elseClause.NamedChildren().ToList();
            var elseBlock = ec.FirstOrDefault(c => c.Type == "compound_statement");
            var elseIf = ec.FirstOrDefault(c => c.Type == "if_statement");

            if (elseBlock.IsValid)
                stmt.Else = ConvertBlock(elseBlock);
            else if (elseIf.IsValid)
                stmt.Else = new List<YawaStatement> { ConvertIf(elseIf) };
        }

        return stmt;
    }

    /// <summary>
    /// В tree-sitter-cpp условия if/while/for обёрнуты в condition_clause.
    /// Эта функция возвращает внутреннее выражение.
    /// </summary>
    private TsNode UnwrapConditionClause(TsNode node)
    {
        if (node.Type != "condition_clause") return node;

        // Внутри condition_clause может быть:
        //   - одно выражение (binary_expression, call_expression, identifier, ...)
        //   - declaration + выражение (для for-loop)
        // Возвращаем последнее named-child (это условие).
        var last = default(TsNode);
        foreach (var c in node.NamedChildren())
            last = c;

        return last;
    }

    private YawaStatement ConvertWhile(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        var cond = children.FirstOrDefault(c => c.Type != "compound_statement");
        var block = children.FirstOrDefault(c => c.Type == "compound_statement");

        var condExpr = UnwrapConditionClause(cond);

        return new WhileStatement
        {
            Cond = ConvertExpr(condExpr),
            Body = block.IsValid ? ConvertBlock(block) : new List<YawaStatement>()
        };
    }

    private YawaStatement ConvertFor(TsNode node)
    {
        // for (init; cond; update) { ... }
        //
        // tree-sitter-cpp структура:
        //   for_statement
        //     declaration / expression_statement  (init)
        //     condition_clause                    (cond, иногда с init внутри)
        //     update_expression / expression_statement  (update)
        //     compound_statement                  (body)
        //
        // condition_clause может содержать:
        //   - один binary_expression (обычный случай)
        //   - declaration + binary_expression (если for (int i = 0; i < n; i++))
        //     в этом случае init тоже внутри condition_clause

        // Разворачиваем named children, заходя в condition_clause
        var flat = new List<TsNode>();
        foreach (var child in node.NamedChildren())
        {
            if (child.Type == "condition_clause")
            {
                foreach (var inner in child.NamedChildren())
                    flat.Add(inner);
            }
            else
            {
                flat.Add(child);
            }
        }

        TsNode init = default, cond = default, update = default, block = default;

        foreach (var c in flat)
        {
            if (c.Type == "compound_statement") { block = c; continue; }

            // update_expression / update через выражение
            if (c.Type == "update_expression")
            {
                update = c;
                continue;
            }

            // init: declaration или expression_statement с assignment
            if (c.Type == "declaration" && !init.IsValid)
            {
                init = c;
                continue;
            }

            // Условие: binary_expression, identifier, call_expression, true
            if (c.Type is "binary_expression" or "identifier" or "call_expression"
                      or "true" or "false" or "parenthesized_expression"
                      and not "compound_statement")
            {
                if (!cond.IsValid) cond = c;
                continue;
            }

            // Прочие — сначала в init (если пусто), потом в update
            if (!init.IsValid) init = c;
            else if (!update.IsValid) update = c;
        }

        // Извлекаем переменную цикла из init
        string loopVar = "";
        YawaExpression? startValue = null;

        if (init.IsValid)
        {
            if (init.Type == "declaration")
            {
                var ids = init.NamedChildren().Where(c => c.Type == "identifier").ToList();
                var initDecl = FindChildByType(init, "init_declarator");
                if (initDecl.IsValid)
                {
                    var parts = initDecl.NamedChildren().ToList();
                    loopVar = ExtractDeclaratorName(parts[0]);
                    startValue = parts.Count > 1 ? ConvertExpr(parts[^1]) : Lit(0);
                }
                else if (ids.Count > 0)
                {
                    loopVar = Text(ids[0]);
                    startValue = Lit(0);
                }
            }
            else if (init.Type == "assignment_expression")
            {
                var parts = init.NamedChildren().ToList();
                loopVar = ExtractDeclaratorName(parts[0]);
                startValue = parts.Count > 1 ? ConvertExpr(parts[^1]) : Lit(0);
            }
        }

        if (string.IsNullOrEmpty(loopVar))
            throw Err("Не удалось найти переменную в for-цикле", node);

        // Извлекаем "to" из cond
        YawaExpression to;
        YawaExpression? step = null;

        if (cond.IsValid && cond.Type == "binary_expression")
        {
            var binChildren = cond.NamedChildren().ToList();
            if (binChildren.Count >= 2)
            {
                var op = ExtractOperator(cond);
                var rhs = binChildren[^1];

                switch (op)
                {
                    case "<":
                        to = ConvertExpr(rhs);
                        break;
                    case "<=":
                        to = new BinaryExpr
                        {
                            Op = "+",
                            A = ConvertExpr(rhs),
                            B = Lit(1)
                        };
                        break;
                    case ">":
                        to = new BinaryExpr
                        {
                            Op = "-",
                            A = ConvertExpr(rhs),
                            B = Lit(1)
                        };
                        step = Lit(-1);
                        break;
                    case ">=":
                        to = ConvertExpr(rhs);
                        step = Lit(-1);
                        break;
                    case "!=":
                        to = ConvertExpr(rhs);
                        break;
                    default:
                        throw Err($"Неизвестный оператор в for-cond: {op}", cond);
                }
            }
            else
                throw Err("Некорректный for-cond", cond);
        }
        else
            throw Err("Ожидался binary_expression в for-cond", node);

        // Определяем step по update
        if (update.IsValid)
        {
            if (update.Type == "update_expression")
            {
                var text = Text(update);
                if (text.StartsWith("--") || text.Contains("--"))
                    step ??= Lit(-1);
            }
            else if (update.Type == "assignment_expression")
            {
                var op = ExtractAssignmentOperator(update);
                if (op == "-=") step = Lit(-1);
                // += оставляем по умолчанию 1
            }
        }

        return new ForStatement
        {
            Var = loopVar,
            From = startValue ?? Lit(0),
            To = to,
            Step = step,
            Body = block.IsValid ? ConvertBlock(block) : new List<YawaStatement>()
        };
    }

    private YawaStatement ConvertRangeFor(TsNode node)
    {
        // for (int x : A) { ... }
        // Пока не поддерживается — редкий случай для визуализации
        throw Err("range-based for (for (x : A)) пока не поддерживается. " +
                  "Используйте for (int i = 0; i < ...; i++)", node);
    }

    // ─────────── Assignment / Update ───────────

    private YawaStatement ConvertAssignment(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректное присваивание", node);

        var target = children[0];
        var value = children[^1];
        var op = ExtractAssignmentOperator(node);

        if (op == "=")
        {
            return new AssignStatement
            {
                Target = ConvertLValue(target),
                Value = ConvertExpr(value)
            };
        }

        // +=, -=, *=, /=, %=, ...
        var binOp = op[..^1];
        return new AssignStatement
        {
            Target = ConvertLValue(target),
            Value = new BinaryExpr
            {
                Op = binOp,
                A = ConvertExpr(target),
                B = ConvertExpr(value)
            }
        };
    }

    private YawaStatement ConvertUpdate(TsNode node)
    {
        // i++ / ++i / i-- / --i
        var text = Text(node);
        var id = FindChildByType(node, "identifier");
        if (!id.IsValid)
            throw Err("Некорректный update_expression", node);

        var name = Text(id);
        var isInc = text.Contains("++");
        var op = isInc ? "+" : "-";

        return new AssignStatement
        {
            Target = new RefExpr { Name = name },
            Value = new BinaryExpr
            {
                Op = op,
                A = new RefExpr { Name = name },
                B = Lit(1)
            }
        };
    }

    // ─────────── cout / cin ───────────

    private bool IsCoutChain(TsNode expr)
    {
        var text = Text(expr);
        return text.StartsWith("cout") || text.StartsWith("std::cout");
    }

    private bool IsCinChain(TsNode expr)
    {
        var text = Text(expr);
        return text.StartsWith("cin") || text.StartsWith("std::cin");
    }

    private YawaStatement ConvertCout(TsNode node)
    {
        // cout << a << " " << b << endl;
        // Превращаем в annotate(concat(...)).
        // Собираем всё, что идёт после <<, кроме endl.
        var parts = new List<YawaExpression>();
        CollectCoutParts(node, parts);

        if (parts.Count == 0)
            return new AnnotateStatement { Text = "" };

        // Склеиваем через +, с конвертацией в строку
        YawaExpression current = new CallExpr
        {
            Name = "str",
            Args = new List<YawaExpression> { parts[0] }
        };

        for (int i = 1; i < parts.Count; i++)
        {
            current = new BinaryExpr
            {
                Op = "+",
                A = current,
                B = new CallExpr
                {
                    Name = "str",
                    Args = new List<YawaExpression> { parts[i] }
                }
            };
        }

        return new AnnotateStatement
        {
            Text = "",  // заполним через специальный ExprAnnotate
        };
        // Пока упрощаем: annotate принимает строку. Нам нужен вариант с выражением.
        // Fallback: annotate("cout: <не поддерживается в MVP>")
    }

    private void CollectCoutParts(TsNode node, List<YawaExpression> parts)
    {
        // В бинарке a << b, left может быть либо identifier "cout", либо вложенная
        if (node.Type != "binary_expression") return;

        var children = node.NamedChildren().ToList();
        if (children.Count < 2) return;

        var left = children[0];
        var right = children[1];

        if (left.Type == "binary_expression")
            CollectCoutParts(left, parts);

        // Пропускаем endl
        if (right.Type == "identifier" &&
            (Text(right) == "endl" || Text(right) == "std::endl"))
            return;

        parts.Add(ConvertExpr(right));
    }

    // ─────────── Expressions ───────────

    private YawaExpression ConvertExpr(TsNode node)
    {
        switch (node.Type)
        {
            case "number_literal":
                {
                    var text = Text(node);
                    if (text.Contains('.') || text.Contains('e') || text.Contains('E'))
                        return Lit(double.Parse(text, System.Globalization.CultureInfo.InvariantCulture));
                    return Lit(long.Parse(text));
                }

            case "string_literal":
                return Lit(UnquoteString(Text(node)));

            case "char_literal":
                {
                    var text = Text(node).Trim('\'');
                    return Lit(text);
                }

            case "true":
                return Lit(true);
            case "false":
                return Lit(false);
            case "null":
            case "nullptr":
            case "NULL":
                return Lit(null);

            case "identifier":
                return new RefExpr { Name = Text(node) };

            case "subscript_expression":
                return ConvertSubscript(node);

            case "field_expression":
                return ConvertFieldAccess(node);

            case "binary_expression":
                return ConvertBinary(node);

            case "unary_expression":
                return ConvertUnary(node);

            case "assignment_expression":
                // Внутри выражения — редко. Например, (x = f())
                // Пока не поддерживаем.
                throw Err("Присваивание внутри выражения не поддерживается", node);

            case "call_expression":
                return ConvertCall(node);

            case "parenthesized_expression":
                {
                    var inner = node.NamedChildren().FirstOrDefault();
                    return inner.IsValid ? ConvertExpr(inner) : Lit(null);
                }

            case "initializer_list":
                return ConvertInitializerList(node);

            case "conditional_expression":
                return ConvertTernary(node);

            case "comma_expression":
                {
                    var last = node.NamedChildren().LastOrDefault();
                    return last.IsValid ? ConvertExpr(last) : Lit(null);
                }

            default:
                throw Err($"Неподдерживаемое выражение: {node.Type}", node);
        }
    }

    private YawaExpression ConvertSubscript(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count == 0)
            throw Err("Некорректный subscript", node);

        // Целевой объект
        YawaExpression current = ConvertExpr(children[0]);

        // M2[j][i] может парситься как:
        //   subscript_expression(subscript_expression(M2, [j]), [i])
        //   или subscript_expression(M2, [j], [i])
        // Идём по всем subscript_argument_list и навешиваем IndexExpr.
        for (int i = 1; i < children.Count; i++)
        {
            var idx = children[i];
            if (idx.Type != "subscript_argument_list") continue;

            var args = idx.NamedChildren().ToList();
            if (args.Count == 0) continue;  // A[] → пропускаем

            var indexExpr = ConvertExpr(args[0]);
            current = new IndexExpr
            {
                Target = current,
                Index = indexExpr
            };
        }

        return current;
    }

    private YawaExpression ConvertFieldAccess(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректный field access", node);

        var obj = children[0];
        var fieldNameNode = children[^1];
        if (fieldNameNode.Type != "field_identifier")
            throw Err("Ожидалось имя поля после .", fieldNameNode);

        return new FieldExpr
        {
            Target = ConvertExpr(obj),
            FieldName = Text(fieldNameNode)
        };
    }

    private YawaExpression ConvertBinary(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректная бинарная операция", node);

        var op = ExtractOperator(node);
        var a = children[0];
        var b = children[^1];

        return new BinaryExpr
        {
            Op = MapOperator(op),
            A = ConvertExpr(a),
            B = ConvertExpr(b)
        };
    }

    private string MapOperator(string cppOp) => cppOp switch
    {
        "==" => "==",
        "!=" => "!=",
        "<" => "<",
        "<=" => "<=",
        ">" => ">",
        ">=" => ">=",
        "+" => "+",
        "-" => "-",
        "*" => "*",
        "/" => "/",
        "%" => "%",
        "&&" => "and",
        "||" => "or",
        "&" => "and",   // побитовые — упрощаем
        "|" => "or",
        _ => cppOp
    };

    private YawaExpression ConvertUnary(TsNode node)
    {
        var text = Text(node);
        var child = node.NamedChildren().FirstOrDefault();

        if (text.StartsWith("!"))
            return new UnaryExpr { Op = "not", A = ConvertExpr(child) };

        if (text.StartsWith("-"))
            return new UnaryExpr { Op = "-", A = ConvertExpr(child) };

        if (text.StartsWith("+"))
            return ConvertExpr(child);

        // &x, *x, ~x — не поддерживаем
        throw Err($"Унарный оператор '{text}' не поддерживается", node);
    }

    private YawaExpression ConvertCall(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count == 0)
            throw Err("Некорректный вызов", node);

        var callee = children[0];
        var argsList = children.FirstOrDefault(c => c.Type == "argument_list");

        var args = argsList.IsValid
            ? argsList.NamedChildren()
                .Where(a => a.Type != "comment")
                .Select(ConvertExpr).ToList()
            : new List<YawaExpression>();

        // Прямой вызов функции
        if (callee.Type == "identifier")
        {
            var name = Text(callee);
            var mapped = name switch
            {
                "min" => "min",
                "max" => "max",
                "abs" => "abs",
                "fabs" => "abs",
                "swap" => "__cpp_swap",   // специальный
                _ => name
            };
            return new CallExpr { Name = mapped, Args = args };
        }

        // Метод: obj.method(...)
        if (callee.Type == "field_expression")
        {
            var fchildren = callee.NamedChildren().ToList();
            var recv = fchildren[0];
            var methodName = fchildren[^1].Type == "field_identifier"
                ? Text(fchildren[^1]) : "";
            return new CallMethodExpr
            {
                Receiver = ConvertExpr(recv),
                Name = methodName,
                Args = args
            };
        }

        // std::cout << ... — сюда обычно не попадает (обработано на statement)

        throw Err($"Неподдерживаемый вызов: {callee.Type}", callee);
    }

    private YawaExpression ConvertTernary(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 3)
            throw Err("Некорректный тернарный оператор", node);

        return new TernaryExpr
        {
            Parts = new TernaryParts
            {
                Cond = ConvertExpr(children[0]),
                Then = ConvertExpr(children[1]),
                Else = ConvertExpr(children[2])
            }
        };
    }

    private YawaExpression ConvertLValue(TsNode node)
    {
        switch (node.Type)
        {
            case "identifier":
                return new RefExpr { Name = Text(node) };
            case "subscript_expression":
                return ConvertSubscript(node);
            case "field_expression":
                return ConvertFieldAccess(node);
            default:
                return ConvertExpr(node);
        }
    }

    // ─────────── Helpers ───────────

    private string Text(TsNode node)
    {
        var s = (int)node.StartByte;
        var e = (int)node.EndByte;
        if (s < 0 || e <= s || e > _bytes.Length) return "";
        return Encoding.UTF8.GetString(_bytes, s, e - s);
    }

    private string ExtractOperator(TsNode node)
    {
        // Оператор находится между children[0] и children[^1]
        var children = node.NamedChildren().ToList();
        if (children.Count < 2) return "";

        var a = children[0];
        var b = children[^1];
        var start = (int)a.EndByte;
        var end = (int)b.StartByte;
        if (start >= end) return "";
        return Encoding.UTF8.GetString(_bytes, start, end - start).Trim();
    }

    private string ExtractAssignmentOperator(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2) return "=";
        var a = children[0];
        var b = children[^1];
        var start = (int)a.EndByte;
        var end = (int)b.StartByte;
        if (start >= end) return "=";
        return Encoding.UTF8.GetString(_bytes, start, end - start).Trim();
    }

    /// <summary>
    /// Собирает размеры из вложенного array_declarator.
    /// int A[5]        → [5]
    /// int M[3][3]     → [3, 3]
    /// int A[]         → []
    /// </summary>
    private void CollectArraySizes(TsNode node, List<int> sizes)
    {
        foreach (var c in node.NamedChildren())
        {
            if (c.Type == "number_literal")
            {
                if (int.TryParse(Text(c), out var n))
                    sizes.Add(n);
            }
            else if (c.Type == "array_declarator")
            {
                CollectArraySizes(c, sizes);
            }
        }
    }

    /// <summary>
    /// Строит массив с указанными размерами, заполненный нулями.
    /// [3, 3] → [[0,0,0],[0,0,0],[0,0,0]]
    /// [5]    → [0,0,0,0,0]
    /// </summary>
    private YawaExpression BuildNestedArray(List<int> sizes, int depth)
    {
        if (depth == sizes.Count - 1)
        {
            var items = Enumerable.Range(0, sizes[depth])
                .Select(_ => (Lit(0) as YawaExpression)!).ToList();
            return new ArrayExpr { Items = items };
        }

        var children = Enumerable.Range(0, sizes[depth])
            .Select(_ => BuildNestedArray(sizes, depth + 1))
            .ToList();
        return new ArrayExpr { Items = children };
    }

    private string UnquoteString(string raw)
    {
        if (raw.Length < 2) return raw;
        var inner = raw.Substring(1, raw.Length - 2);
        return inner
            .Replace("\\n", "\n")
            .Replace("\\t", "\t")
            .Replace("\\r", "\r")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    private static TsNode FindChildByType(TsNode node, string type)
    {
        foreach (var c in node.NamedChildren())
            if (c.Type == type) return c;
        return default;
    }

    /// <summary>
    /// Рекурсивно ищет первый identifier внутри declarator-а.
    /// Для int A[] → A, для int *p → p, для int n → n.
    /// </summary>
    private string FindDeepIdentifier(TsNode node)
    {
        // Прямой случай
        if (node.Type == "identifier") return Text(node);

        // Ищем в детях
        foreach (var c in node.NamedChildren())
        {
            // Пропускаем primitive_type / type_identifier
            if (c.Type is "primitive_type" or "type_identifier" or "type_descriptor" or "sized_type_specifier")
                continue;
            if (c.Type == "template_type" || c.Type == "template_argument_list")
                continue;

            var result = FindDeepIdentifier(c);
            if (!string.IsNullOrEmpty(result)) return result;
        }
        return "";
    }

    private static LiteralExpr Lit(object? value)
        => new() { Value = JsonSerializer.SerializeToElement(value) };

    private static UnsupportedFeatureException Err(string msg, TsNode node)
    {
        var p = node.StartPoint;
        return new UnsupportedFeatureException(msg, (int)p.Row, (int)p.Col);
    }
}
