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
    private readonly List<YawaStatement?> _topLevelDeclarations = new();
    private readonly HashSet<string> _classNamesCpp = new(StringComparer.Ordinal);
    private HashSet<string>? _currentClassFields;
    private string? _currentClassName;
    private Dictionary<string, (string key, string value)>? _iterPairReplacement;
    private HashSet<string>? _iterSimpleReplacement;

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
                    // Пропускаем: include, using, namespace, forward declarations
                    break;
                case "declaration":
                    {
                        // top-level const/static — глобальные переменные
                        var decl = ConvertDeclaration(child);
                        // ConvertDeclaration возвращает ExprStatement/If/DeclareStatement.
                        // DeclareStatement — то, что нам нужно, но он умеет работать и в top-level.
                        // Однако наш интерпретатор ожидает global в program.Globals (YawaGlobal с value).
                        // Простое решение: обернуть как отдельную функцию "__globals__" — нет.
                        // Другой вариант: конвертируем в declaration внутри main. Но main уже есть.
                        //
                        // Самое простое: пропустить, но запомнить и вставить в начало main.
                        // Сделаем это через очередь "_topDeclarations" и вставим в начало main.
                        _topLevelDeclarations.Add(decl);
                        break;
                    }

                case "linkage_specification":
                    // extern "C" { ... } — пропускаем
                    break;

                case "class_specifier":
                case "struct_specifier":
                    {
                        var nameNode = child.NamedChildren().FirstOrDefault(c => c.Type == "type_identifier");
                        if (nameNode.IsValid)
                            _classNamesCpp.Add(Text(nameNode));

                        var methods = ExtractClassMethods(child);
                        foreach (var m in methods)
                        {
                            if (!string.IsNullOrEmpty(m.Name) && _functionNames.Add(m.Name))
                                program.Functions.Add(m);
                        }
                        break;
                    }

                default:
                    _warnings.Add($"Пропущен top-level: {child.Type}");
                    break;
            }
        }

        // Вставляем top-level declarations в начало main
        if (_topLevelDeclarations.Count > 0)
        {
            var mainCandidate = program.Functions.FirstOrDefault(f => f.Name == "main");
            if (mainCandidate is not null)
            {
                var prefix = _topLevelDeclarations
                    .Where(d => d is not null)
                    .Cast<YawaStatement>()
                    .ToList();
                mainCandidate.Body.InsertRange(0, prefix);
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
    /// <summary>
    /// Извлекает методы класса/структуры как top-level функции ClassName.method
    /// с неявным self как первым параметром.
    /// </summary>
    private List<YawaFunction> ExtractClassMethods(TsNode classNode)
    {
        var result = new List<YawaFunction>();

        var children = classNode.NamedChildren().ToList();
        var nameNode = children.FirstOrDefault(c => c.Type == "type_identifier");
        if (!nameNode.IsValid) return result;
        var className = Text(nameNode);

        var bodyNode = children.FirstOrDefault(c => c.Type == "field_declaration_list");
        if (!bodyNode.IsValid) return result;

        // Собираем имена полей: `int value;` → "value"
        var fields = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in bodyNode.NamedChildren())
        {
            if (member.Type == "field_declaration")
            {
                var id = FindChildByType(member, "field_identifier");
                if (!id.IsValid) id = FindChildByType(member, "identifier");
                if (id.IsValid) fields.Add(Text(id));
            }
        }

        var savedFields = _currentClassFields;
        var savedClass = _currentClassName;
        _currentClassFields = fields;
        _currentClassName = className;

        try
        {
            foreach (var member in bodyNode.NamedChildren())
            {
                if (member.Type != "function_definition") continue;

                var funcDecl = FindChildByType(member, "function_declarator");

                // Методы класса имеют field_identifier, конструктор — identifier
                var nameId = default(TsNode);
                if (funcDecl.IsValid)
                {
                    nameId = FindChildByType(funcDecl, "field_identifier");
                    if (!nameId.IsValid)
                        nameId = FindChildByType(funcDecl, "identifier");
                }

                if (nameId.IsValid)
                {
                    var methodName = Text(nameId);

                    // Конструктор — имя совпадает с классом
                    if (methodName == className)
                    {
                        var fn = ConvertConstructor(member, funcDecl, className);
                        if (fn is not null) result.Add(fn);
                    }
                    else
                    {
                        var fn = ConvertMethod(member, funcDecl, methodName, className);
                        if (fn is not null) result.Add(fn);
                    }
                }
                else
                {
                    // Безымянный function_definition внутри класса — очень странно
                    // Пробуем как конструктор
                    var fn = ConvertConstructor(member, funcDecl, className);
                    if (fn is not null) result.Add(fn);
                }
            }
        }
        finally
        {
            _currentClassFields = savedFields;
            _currentClassName = savedClass;
        }

        return result;
    }

    /// <summary>
    /// Метод класса C++ → Counter.inc с self первым параметром.
    /// </summary>
    private YawaFunction? ConvertMethod(TsNode funcDef, TsNode funcDecl, string methodName, string className)
    {
        var fn = new YawaFunction
        {
            Name = $"{className}.{methodName}"
        };

        // Параметры (кроме self)
        if (funcDecl.IsValid)
        {
            var paramList = FindChildByType(funcDecl, "parameter_list");
            if (paramList.IsValid)
            {
                foreach (var p in paramList.NamedChildren())
                {
                    if (p.Type != "parameter_declaration") continue;
                    var pName = FindDeepIdentifier(p);
                    if (!string.IsNullOrEmpty(pName))
                        fn.Params.Add(new YawaParam { Name = pName });
                }
            }
        }

        // self — первый
        fn.Params.Insert(0, new YawaParam { Name = "self" });

        // Тело
        var body = FindChildByType(funcDef, "compound_statement");
        if (body.IsValid)
            fn.Body = ConvertBlock(body);

        return fn;
    }

    /// <summary>
    /// Конструктор C++ → метод ClassName.ClassName с self первым параметром.
    /// </summary>
    private YawaFunction? ConvertConstructor(TsNode funcDef, TsNode funcDecl, string className)
    {
        var fn = new YawaFunction
        {
            Name = $"{className}.{className}"
        };

        // Параметры (без self — его добавим)
        if (funcDecl.IsValid)
        {
            var paramList = FindChildByType(funcDecl, "parameter_list");
            if (paramList.IsValid)
            {
                foreach (var p in paramList.NamedChildren())
                {
                    if (p.Type != "parameter_declaration") continue;
                    var pName = FindDeepIdentifier(p);
                    if (!string.IsNullOrEmpty(pName))
                        fn.Params.Add(new YawaParam { Name = pName });
                }
            }
        }

        // self — первый
        fn.Params.Insert(0, new YawaParam { Name = "self" });

        // Тело
        var body = FindChildByType(funcDef, "compound_statement");
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
            case "init_declarator":
                return ConvertDeclaration(node);

            case "expression_statement":
                {
                    var inner = node.NamedChildren().FirstOrDefault();
                    if (!inner.IsValid) return null;

                    // swap(a, b) → SwapRefStatement
                    if (inner.Type == "call_expression")
                    {
                        var callKids = inner.NamedChildren().ToList();
                        if (callKids.Count >= 1 && callKids[0].Type == "identifier" &&
                            Text(callKids[0]) == "swap")
                        {
                            var argList = callKids.FirstOrDefault(c => c.Type == "argument_list");
                            if (argList.IsValid)
                            {
                                var args = argList.NamedChildren()
                                    .Where(a => a.Type != "comment")
                                    .Select(ConvertExpr).ToList();
                                if (args.Count == 2)
                                {
                                    return new SwapRefStatement { A = args[0], B = args[1] };
                                }
                            }
                        }
                    }

                    // cout << ... — специальный случай
                    if (inner.Type == "binary_expression" &&
                        IsCoutChain(inner))
                    {
                        return ConvertCout(inner);
                    }

                    // cin >> ... — специальный случай
                    if (inner.Type == "binary_expression" && IsCinChain(inner))
                    {
                        // cin >> x — игнорируем (нет stdin в визуализаторе).
                        return new AnnotateStatement { Text = "cin (ввод не поддерживается в демо)" };
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

            case "for_range_loop":
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
        var statements = new List<YawaStatement>();

        // Определяем "тип" — vector / array / обычный
        var typeNode = node.NamedChildren().FirstOrDefault(c =>
            c.Type is "primitive_type" or "type_identifier" or "template_type");
        var typeName = typeNode.IsValid ? Text(typeNode) : "";
        var isContainer = IsVectorType(typeName) || typeName.Contains("set") || typeName.Contains("map");
        var isSet = typeName.Contains("set") && !typeName.Contains("bitset");
        var isMap = typeName.Contains("map") && !typeName.Contains("unordered_map")
                 || typeName.Contains("unordered_map");

        foreach (var child in node.NamedChildren())
        {
            if (child.Type == "init_declarator")
            {
                var parts = child.NamedChildren().ToList();
                if (parts.Count < 2) continue;
                var target = parts[0];
                var value = parts[^1];

                var declName = ExtractDeclaratorName(target);
                if (string.IsNullOrEmpty(declName)) continue;

                YawaExpression initValue;
                if (value.Type == "initializer_list")
                {
                    if (isSet)
                    {
                        initValue = new SetLiteralExpr
                        {
                            Items = value.NamedChildren().Select(ConvertExpr).ToList()
                        };
                    }
                    else
                    {
                        initValue = ConvertInitializerList(value);
                    }
                }
                else if (value.Type == "call_expression")
                {
                    initValue = ConvertExpr(value);
                }
                else
                {
                    initValue = ConvertExpr(value);
                }

                statements.Add(new DeclareStatement
                {
                    Name = declName,
                    Value = initValue
                });
            }
            else if (child.Type == "identifier")
            {
                YawaExpression initValue;
                if (isSet)
                    initValue = new SetLiteralExpr();
                else if (isMap)
                    initValue = new DictExpr();
                else if (isContainer)
                    initValue = new ArrayExpr();
                else if (_classNamesCpp.Contains(typeName))
                    // Counter c; → Instantiate("Counter")
                    initValue = new InstantiateExpr { ClassName = typeName, Args = new() };
                else if (IsUserType(typeName))
                    // Неизвестный тип → пустой объект с __type__
                    initValue = new NewObjectExpr
                    {
                        Fields = { ["__type__"] = Lit(typeName) }
                    };
                else
                    initValue = Lit(null);

                statements.Add(new DeclareStatement
                {
                    Name = Text(child),
                    Value = initValue
                });
            }
            else if (child.Type == "array_declarator")
            {
                var declName = ExtractDeclaratorName(child);
                if (string.IsNullOrEmpty(declName)) continue;

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

    /// <summary>
    /// Проверяет, является ли тип "vector&lt;...&gt;" или "array&lt;...&gt;".
    /// </summary>
    private bool IsVectorType(string typeName) =>
        typeName.StartsWith("vector") || typeName.StartsWith("std::vector")
        || typeName.StartsWith("array") || typeName.StartsWith("std::array")
        || typeName.Contains("vector<") || typeName.Contains("array<");

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
                return FindDeepIdentifier(declarator);

            default:
                return FindDeepIdentifier(declarator);
        }
    }
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

        TsNode condNode = default;
        TsNode thenNode = default;
        TsNode elseClause = default;

        bool condSeen = false;
        foreach (var c in children)
        {
            if (c.Type == "condition_clause" && !condSeen)
            {
                condNode = c;
                condSeen = true;
                continue;
            }
            if (c.Type == "else_clause")
            {
                elseClause = c;
                continue;
            }
            if (condSeen && !thenNode.IsValid)
                thenNode = c;
        }

        var stmt = new IfStatement
        {
            Cond = ConvertExpr(UnwrapConditionClause(condNode)),
            Then = ConvertBody(thenNode)
        };

        if (elseClause.IsValid)
        {
            var ec = elseClause.NamedChildren().ToList();
            TsNode elseBody = default;
            foreach (var c in ec)
            {
                if (c.Type == "if_statement")
                {
                    stmt.Else = new List<YawaStatement> { ConvertIf(c) };
                    return stmt;
                }
                if (!elseBody.IsValid) elseBody = c;
            }
            stmt.Else = ConvertBody(elseBody);
        }

        return stmt;
    }

    /// <summary>
    /// Тело if/while/for: либо { statements }, либо один statement.
    /// </summary>
    private List<YawaStatement> ConvertBody(TsNode node)
    {
        if (!node.IsValid) return new List<YawaStatement>();

        if (node.Type == "compound_statement")
            return ConvertBlock(node);

        var s = ConvertStatement(node);
        return s is null ? new List<YawaStatement>() : new List<YawaStatement> { s };
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

        TsNode condNode = default;
        TsNode bodyNode = default;

        bool condSeen = false;
        foreach (var c in children)
        {
            if (c.Type == "condition_clause" && !condSeen)
            {
                condNode = c;
                condSeen = true;
                continue;
            }
            if (condSeen && !bodyNode.IsValid)
                bodyNode = c;
        }

        return new WhileStatement
        {
            Cond = ConvertExpr(UnwrapConditionClause(condNode)),
            Body = ConvertBody(bodyNode)
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
        if (TryConvertIteratorFor(node) is { } r) return r;

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
            Body = ConvertBody(block)
        };
    }
    private YawaStatement? TryConvertIteratorFor(TsNode node)
    {
        // Быстрая проверка по тексту: должен быть .begin() и .end()
        var fullText = Text(node);
        if (!fullText.Contains(".begin()") || !fullText.Contains(".end()"))
            return null;

        // Найдём body
        TsNode body = default;
        foreach (var c in node.NamedChildren())
            if (c.Type == "compound_statement") { body = c; break; }
        if (!body.IsValid) return null;

        // Найдём init: первый named child, НЕ condition_clause, НЕ compound_statement, НЕ update_expression
        TsNode init = default;
        foreach (var c in node.NamedChildren())
        {
            if (c.Type == "compound_statement") continue;
            if (c.Type == "condition_clause") continue;
            if (c.Type == "update_expression") continue;
            if (c.Type == "expression_statement" && Text(c).Contains("++")) continue;
            init = c;
            break;
        }
        if (!init.IsValid) return null;

        // Теперь рекурсивно по всему init ищем:
        //   - iterName — первый identifier, стоящий до ".begin()"
        //   - containerNode — receiver перед ".begin"
        var initText = Text(init);
        var beginIdx = initText.IndexOf(".begin()", StringComparison.Ordinal);
        if (beginIdx < 0) return null;

        string iterName = "";
        TsNode containerNode = default;

        ScanForIter(init, beginIdx, ref iterName, ref containerNode);

        if (string.IsNullOrEmpty(iterName) || !containerNode.IsValid)
            return null;

        bool hasPairAccess = BodyHasFieldAccess(body, iterName, "first")
                          || BodyHasFieldAccess(body, iterName, "second");

        if (hasPairAccess)
        {
            var keyVar = iterName + "_k";
            var valueVar = iterName + "_v";
            _iterPairReplacement = new Dictionary<string, (string, string)>
            {
                [iterName] = (keyVar, valueVar)
            };
            var convertedBody = ConvertBlock(body);
            _iterPairReplacement = null;

            return new ForeachPairStatement
            {
                KeyVar = keyVar,
                ValueVar = valueVar,
                In = ConvertExpr(containerNode),
                Body = convertedBody
            };
        }

        _iterSimpleReplacement = new HashSet<string> { iterName };
        var convertedBody2 = ConvertBlock(body);
        _iterSimpleReplacement = null;

        return new ForeachStatement
        {
            Var = iterName,
            In = ConvertExpr(containerNode),
            Body = convertedBody2
        };
    }

    /// <summary>
    /// Рекурсивный обход init: ищет identifier до позиции beginIdx (iterName),
    /// и field_expression с "begin" (containerNode = receiver).
    /// </summary>
    private void ScanForIter(TsNode n, int beginIdx, ref string iterName, ref TsNode containerNode)
    {
        if (!string.IsNullOrEmpty(iterName) && containerNode.IsValid) return;

        if (n.Type == "identifier" && string.IsNullOrEmpty(iterName))
        {
            iterName = Text(n);
        }

        if (n.Type == "field_expression" && !containerNode.IsValid)
        {
            var fc = n.NamedChildren().ToList();
            if (fc.Count >= 2 &&
                fc[^1].Type == "field_identifier" &&
                Text(fc[^1]) == "begin")
            {
                containerNode = fc[0];
            }
        }

        foreach (var c in n.NamedChildren())
            ScanForIter(c, beginIdx, ref iterName, ref containerNode);
    }

    private bool BodyHasFieldAccess(TsNode node, string iterName, string fieldName)
    {
        if (node.Type == "field_expression")
        {
            var children = node.NamedChildren().ToList();
            if (children.Count >= 2 &&
                children[0].Type == "identifier" && Text(children[0]) == iterName &&
                children[^1].Type == "field_identifier" && Text(children[^1]) == fieldName)
                return true;
        }
        foreach (var c in node.NamedChildren())
            if (BodyHasFieldAccess(c, iterName, fieldName)) return true;
        return false;
    }

    private YawaStatement ConvertRangeFor(TsNode node)
    {
        // for (int x : v) { ... }
        // for (auto &x : v) { ... }
        // for (char c : s) { ... }
        // for (Point p : points) { ... }
        //
        // Возможные структуры в tree-sitter-cpp:
        //   v0.23.x: [primitive_type, identifier(x), identifier(v), compound_statement]
        //   другие:  [declaration(int x), identifier(v), compound_statement]
        //   auto:    [identifier(x), identifier(v), compound_statement]
        //   auto&:   [reference_declarator(x), identifier(v), compound_statement]

        var children = node.NamedChildren().ToList();

        var blockNode = children.FirstOrDefault(c => c.Type == "compound_statement");
        var others = children.Where(c => c.Type != "compound_statement").ToList();

        string varName = "";
        TsNode iterableNode = default;

        // ─── Подход 1: declaration + identifier ───
        var decl = others.FirstOrDefault(c => c.Type == "declaration");
        if (decl.IsValid)
        {
            varName = FindDeepIdentifier(decl);
            var idx = others.IndexOf(decl);
            if (idx + 1 < others.Count)
                iterableNode = others[idx + 1];
        }

        // ─── Подход 2: primitive_type + identifier + identifier (v0.23.x) ───
        if (string.IsNullOrEmpty(varName))
        {
            var ids = others.Where(c => c.Type == "identifier").ToList();
            if (ids.Count >= 2)
            {
                varName = Text(ids[^2]);       // предпоследний identifier
                iterableNode = ids[^1];        // последний identifier
            }
        }

        // ─── Подход 3: reference_declarator / pointer_declarator + identifier ───
        if (string.IsNullOrEmpty(varName))
        {
            var refDecl = others.FirstOrDefault(c =>
                c.Type == "reference_declarator" || c.Type == "pointer_declarator");
            if (refDecl.IsValid)
            {
                varName = FindDeepIdentifier(refDecl);
                var idx = others.IndexOf(refDecl);
                if (idx + 1 < others.Count)
                    iterableNode = others[idx + 1];
            }
        }

        if (string.IsNullOrEmpty(varName) || !iterableNode.IsValid)
        {
            var shape = string.Join(", ", children.Select(c => c.Type));
            throw Err($"Не удалось разобрать for-range (узлы: {shape})", node);
        }

        return new ForeachStatement
        {
            Var = varName,
            In = ConvertExpr(iterableNode),
            Body = blockNode.IsValid ? ConvertBlock(blockNode) : new List<YawaStatement>()
        };
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

            case "pointer_expression":
                {
                    var inner = node.NamedChildren().FirstOrDefault();
                    if (!inner.IsValid) return Lit(null);

                    // *it внутри for-iterator по set → сам it
                    if (inner.Type == "identifier" &&
                        _iterSimpleReplacement?.Contains(Text(inner)) == true)
                    {
                        return new RefExpr { Name = Text(inner) };
                    }

                    return ConvertExpr(inner);
                }

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

        // Замена it->first / it->second внутри for-iterator по map
        if (_iterPairReplacement is not null && obj.Type == "identifier")
        {
            var iterName = Text(obj);
            var fieldName = Text(fieldNameNode);
            if (_iterPairReplacement.TryGetValue(iterName, out var repl))
            {
                if (fieldName == "first") return new RefExpr { Name = repl.key };
                if (fieldName == "second") return new RefExpr { Name = repl.value };
            }
        }

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

        // X.erase(X.begin()) → erase_first(X)
        if (callee.Type == "field_expression")
        {
            var fc = callee.NamedChildren().ToList();
            if (fc.Count >= 2 && fc[^1].Type == "field_identifier"
                             && Text(fc[^1]) == "erase")
            {
                var receiverText = Text(fc[0]);
                if (argsList.IsValid)
                {
                    var argsRaw = argsList.NamedChildren().ToList();
                    if (argsRaw.Count == 1 && argsRaw[0].Type == "call_expression")
                    {
                        var innerCall = argsRaw[0].NamedChildren().ToList();
                        if (innerCall.Count > 0 && innerCall[0].Type == "field_expression")
                        {
                            var innerFc = innerCall[0].NamedChildren().ToList();
                            if (innerFc.Count >= 2 && Text(innerFc[^1]) == "begin"
                                                  && Text(innerFc[0]) == receiverText)
                            {
                                return new CallExpr
                                {
                                    Name = "erase_first",
                                    Args = new List<YawaExpression> { ConvertExpr(fc[0]) }
                                };
                            }
                        }
                    }
                }
            }
        }
        var args = argsList.IsValid
            ? argsList.NamedChildren()
                .Where(a => a.Type != "comment")
                .Select(ConvertExpr).ToList()
            : new List<YawaExpression>();

        // STL-итераторы: sort(v.begin(), v.end()) → sort(v)
        // Ловим паттерн: args[0] = v.begin(), args[1] = v.end() с одинаковым v
        if (args.Count >= 2 && IsBeginEndPair(argsList, out var container))
        {
            var cleanArgs = new List<YawaExpression> { container! };
            // Для accumulate третьим аргументом идёт начальное значение — оставляем его
            for (int i = 2; i < args.Count; i++)
                cleanArgs.Add(args[i]);

            // min_element/max_element возвращают указатель — обычно используется как *min_element
            // а мы уже развернули pointer_expression в значении — значит просто отдаём container.

            var simpleName = Text(callee);
            return new CallExpr { Name = simpleName, Args = cleanArgs };
        }

        // Прямой вызов функции
        if (callee.Type == "identifier")
        {
            var name = Text(callee);

            // C++: Counter() → Instantiate
            if (_classNamesCpp.Contains(name))
                return new InstantiateExpr { ClassName = name, Args = args };

            var mapped = name switch
            {
                "min" => "min",
                "max" => "max",
                "abs" => "abs",
                "fabs" => "abs",
                "swap" => "__cpp_swap",
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

    /// <summary>
    /// Проверяет, что argsList содержит паттерн v.begin(), v.end() (с одинаковым v).
    /// Возвращает v как YawaExpression.
    /// </summary>
    private bool IsBeginEndPair(TsNode argsList, out YawaExpression? container)
    {
        container = null;
        if (!argsList.IsValid) return false;

        var args = argsList.NamedChildren()
            .Where(a => a.Type != "comment")
            .ToList();

        if (args.Count < 2) return false;

        var first = args[0];
        var second = args[1];

        var firstRecv = ExtractMethodReceiver(first, "begin");
        var secondRecv = ExtractMethodReceiver(second, "end");

        if (firstRecv is null || secondRecv is null) return false;

        // Оба должны быть одинаковыми (v.begin() и v.end())
        if (Text(firstRecv.Value) != Text(secondRecv.Value)) return false;

        container = ConvertExpr(firstRecv.Value);
        return true;
    }

    /// <summary>
    /// Извлекает receiver из вызова v.method() или v.method(…).
    /// Возвращает null, если это не вызов метода с указанным именем.
    /// </summary>
    private TsNode? ExtractMethodReceiver(TsNode call, string methodName)
    {
        if (call.Type != "call_expression") return null;

        var children = call.NamedChildren().ToList();
        if (children.Count == 0) return null;

        var callee = children[0];
        if (callee.Type != "field_expression") return null;

        var fc = callee.NamedChildren().ToList();
        if (fc.Count < 2) return null;

        var methodId = fc[^1];
        if (methodId.Type != "field_identifier") return null;
        if (Text(methodId) != methodName) return null;

        return fc[0];
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
        if (node.Type == "identifier")
        {
            var name = Text(node);

            // Поле класса
            if (_currentClassName is not null &&
                _currentClassFields is not null &&
                _currentClassFields.Contains(name))
            {
                return new FieldExpr
                {
                    Target = new RefExpr { Name = "self" },
                    FieldName = name
                };
            }

            return new RefExpr { Name = name };
        }

        return ConvertExpr(node);
    }

    // ─────────── Helpers ───────────

    private string Text(TsNode node)
    {
        var s = (int)node.StartByte;
        var e = (int)node.EndByte;
        if (s < 0 || e <= s || e > _bytes.Length) return "";
        return Encoding.UTF8.GetString(_bytes, s, e - s);
    }

    /// <summary>
    /// Пользовательский тип — struct/class (не primitive, не STL-контейнер).
    /// </summary>
    private bool IsUserType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return false;
        if (IsVectorType(typeName)) return false;
        if (typeName.Contains("set") || typeName.Contains("map")) return false;
        if (typeName.Contains("stack") || typeName.Contains("queue")) return false;
        if (typeName.Contains("pair")) return false;
        if (typeName == "int" || typeName == "long" || typeName == "short"
            || typeName == "char" || typeName == "bool" || typeName == "float"
            || typeName == "double" || typeName == "void" || typeName == "string"
            || typeName == "auto") return false;
        return true;
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

    private YawaExpression ConvertIdentifier(TsNode node)
    {
        var name = Text(node);

        // Внутри метода класса — если name это поле класса, значит self.name
        if (_currentClassName is not null &&
            _currentClassFields is not null &&
            _currentClassFields.Contains(name))
        {
            return new FieldExpr
            {
                Target = new RefExpr { Name = "self" },
                FieldName = name
            };
        }

        // Глобальная функция как значение
        if (_functionNames.Contains(name) && !_classNamesCpp.Contains(name))
            return new FunctionRefExpr { Name = name };

        return new RefExpr { Name = name };
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
