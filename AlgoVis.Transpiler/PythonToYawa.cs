using System.Text;
using AlgoVis.Parser.Parsing;
using AlgoVis.Yawa.Yawa;
using AlgoVis.Yawa.Yawa.Expressions;
using AlgoVis.Yawa.Yawa.Statements;

namespace AlgoVis.Transpiler;

/// <summary>
/// Конвертер Python-кода в YAWA-JSON.
/// Поддерживает подмножество Python, достаточное для алгоритмов.
/// </summary>
public sealed class PythonToYawa
{
    private readonly string _source;
    private readonly byte[] _bytes;
    private readonly List<string> _warnings = new();
    private readonly HashSet<string> _functionNames = new();
    private readonly List<YawaFunction> _pendingNestedFunctions = new();
    private string _currentFunctionPrefix = "";

    public PythonToYawa(string source)
    {
        _source = source;
        _bytes = Encoding.UTF8.GetBytes(source);
    }

    public IReadOnlyList<string> Warnings => _warnings;

    // ─────────── Точка входа ───────────

    public YawaProgram Transpile(string? entryFunction = null)
    {
        var parser = new PythonParser();
        using var tree = parser.Parse(_source);
        var root = tree.Root;

        if (root.Type != "module")
            throw new UnsupportedFeatureException($"Ожидался 'module', получено '{root.Type}'");

        var program = new YawaProgram
        {
            YawaVersion = "1.0",
            Metadata = new YawaMetadata
            {
                Name = "Python → YAWA",
                Generator = "algovis-transpiler@0.1"
            }
        };

        // Top-level: функции, глобальные присваивания, импорты
        foreach (var child in root.NamedChildren())
        {
            switch (child.Type)
            {
                case "function_definition":
                case "decorated_definition":
                    TsNode fnode = child;
                    if (child.Type == "decorated_definition")
                    {
                        fnode = child.NamedChildren().FirstOrDefault(n => n.Type == "function_definition");
                        if (!fnode.IsValid)
                            throw Err("Поддерживаются только декораторы над функциями", child);
                    }

                    _pendingNestedFunctions.Clear();
                    var fn = ConvertFunction(fnode);
                    if (!_functionNames.Add(fn.Name))
                        throw Err($"Дублирующаяся функция: {fn.Name}", fnode);
                    program.Functions.Add(fn);

                    foreach (var nested in _pendingNestedFunctions)
                    {
                        if (_functionNames.Add(nested.Name))
                            program.Functions.Add(nested);
                    }
                    _pendingNestedFunctions.Clear();
                    break;

                case "expression_statement":
                    {
                        var assign = child.NamedChildren().FirstOrDefault(c => c.Type == "assignment");
                        if (assign.IsValid)
                        {
                            var g = ConvertGlobalAssignment(assign);
                            if (g is not null)
                                program.Globals[g.Value.Key] = g.Value.Value;
                        }
                        break;
                    }

                case "import_statement":
                case "import_from_statement":
                case "comment":
                    // молча игнорируем
                    break;

                default:
                    _warnings.Add($"Пропущен top-level узел: {child.Type}");
                    break;
            }
        }

        if (program.Functions.Count == 0)
            throw new UnsupportedFeatureException("В файле нет ни одной функции");

        // Определяем entry
        var entryName = entryFunction;
        if (string.IsNullOrEmpty(entryName))
        {
            if (_functionNames.Contains("main"))
                entryName = "main";
            else if (program.Functions.Count == 1)
                entryName = program.Functions[0].Name;
            else
                throw new UnsupportedFeatureException(
                    "Не могу выбрать entry-функцию: либо добавьте 'def main():', " +
                    "либо оставьте одну функцию в файле");
        }

        var entryFn = program.Functions.FirstOrDefault(f => f.Name == entryName)
            ?? throw new UnsupportedFeatureException($"Entry-функция '{entryName}' не найдена");

        var entryArgs = new List<YawaExpression>();
        foreach (var p in entryFn.Params)
        {
            if (p.DefaultValue is not null)
                entryArgs.Add(p.DefaultValue);
            else
                throw new UnsupportedFeatureException(
                    $"Entry '{entryName}': параметр '{p.Name}' не имеет значения по умолчанию. " +
                    "Оформите main() без параметров или задайте default value.");
        }

        program.Entry = new YawaEntry
        {
            Function = entryName,
            Args = entryArgs
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
        var children = node.NamedChildren().ToList();
        var nameNode = children.FirstOrDefault(c => c.Type == "identifier");
        var paramsNode = children.FirstOrDefault(c => c.Type == "parameters");
        var returnType = children.FirstOrDefault(c => c.Type == "type");
        var block = children.FirstOrDefault(c => c.Type == "block");

        if (!nameNode.IsValid)
            throw Err("Функция без имени", node);

        var fn = new YawaFunction
        {
            Name = Text(nameNode),
            Returns = returnType.IsValid ? Text(returnType) : null
        };

        var savedPrefix = _currentFunctionPrefix;
        _currentFunctionPrefix = string.IsNullOrEmpty(savedPrefix)
            ? fn.Name
            : savedPrefix + "__" + fn.Name;

        if (paramsNode.IsValid)
        {
            foreach (var p in paramsNode.NamedChildren())
            {
                switch (p.Type)
                {
                    case "identifier":
                        fn.Params.Add(new YawaParam { Name = Text(p) });
                        break;

                    case "typed_parameter":
                        {
                            var pname = p.NamedChildren().FirstOrDefault(c => c.Type == "identifier");
                            var ptype = p.NamedChildren().FirstOrDefault(c => c.Type == "type");
                            fn.Params.Add(new YawaParam
                            {
                                Name = pname.IsValid ? Text(pname) : "",
                                Type = ptype.IsValid ? Text(ptype) : null
                            });
                            break;
                        }

                    case "default_parameter":
                        {
                            var pname = p.NamedChildren().FirstOrDefault(c => c.Type == "identifier");
                            var pval = p.NamedChildren().LastOrDefault();
                            fn.Params.Add(new YawaParam
                            {
                                Name = pname.IsValid ? Text(pname) : "",
                                DefaultValue = pval.IsValid ? ConvertExpr(pval) : null
                            });
                            break;
                        }

                    case "typed_default_parameter":
                        {
                            var pname = p.NamedChildren().FirstOrDefault(c => c.Type == "identifier");
                            var ptype = p.NamedChildren().FirstOrDefault(c => c.Type == "type");
                            var pval = p.NamedChildren().LastOrDefault();
                            fn.Params.Add(new YawaParam
                            {
                                Name = pname.IsValid ? Text(pname) : "",
                                Type = ptype.IsValid ? Text(ptype) : null,
                                DefaultValue = pval.IsValid ? ConvertExpr(pval) : null
                            });
                            break;
                        }

                    default:
                        _warnings.Add($"Пропущен параметр типа '{p.Type}'");
                        break;
                }
            }
        }

        if (block.IsValid)
            fn.Body = ConvertBlock(block);


        _currentFunctionPrefix = savedPrefix;

        return fn;
    }

    private (string Key, YawaGlobal Value)? ConvertGlobalAssignment(TsNode assign)
    {
        var children = assign.NamedChildren().ToList();
        if (children.Count < 2) return null;

        var target = children[0];
        var value = children[children.Count - 1];

        if (target.Type != "identifier") return null;

        return (Text(target), new YawaGlobal { Value = ConvertExpr(value) });
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
            case "expression_statement":
                {
                    var assign = node.NamedChildren().FirstOrDefault(c => c.Type == "assignment");
                    if (assign.IsValid) return ConvertAssignment(assign);

                    var aug = node.NamedChildren().FirstOrDefault(c => c.Type == "augmented_assignment");
                    if (aug.IsValid) return ConvertAugmentedAssignment(aug);

                    // просто выражение: print(x), some_func(), etc.
                    var inner = node.NamedChildren().FirstOrDefault();
                    if (inner.IsValid)
                    {
                        // проверяем спец-функции: swap, compare, mark, annotate
                        var special = TryConvertSpecialCall(inner);
                        if (special is not null) return special;

                        return new ExprStatement { Value = ConvertExpr(inner) };
                    }
                    return null;
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

            case "pass_statement":
                return null; // пустой statement не нужен

            case "break_statement":
                return new BreakStatement();

            case "continue_statement":
                return new ContinueStatement();

            case "function_definition":
                // Вложенная функция — вытащим её на top-level с переименованием.
                // Возвращаем null, потому что регистрация функции идёт отдельно через
                // _pendingNestedFunctions.
                var nested = ConvertFunction(node);
                var newName = _currentFunctionPrefix + "__" + nested.Name;
                nested.Name = newName;
                _pendingNestedFunctions.Add(nested);
                return null; // в текущий body ничего не добавляем

            case "comment":
                return null;

            default:
                throw Err($"Неподдерживаемый statement: {node.Type}", node);
        }
    }

    private YawaStatement ConvertAssignment(TsNode assign)
    {
        var children = assign.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректное присваивание", assign);

        var target = children[0];
        var value = children[children.Count - 1];

        // swap: A[i], A[j] = A[j], A[i]
        if (children.Count == 2 && TryConvertSwap(target, value, assign) is { } sw)
            return sw;

        // Кортежное присваивание: a, b = expr1, expr2  или  a, b = expr_pair
        // Признак: target это pattern_list (a, b) или expression_list (a, b)
        // либо value это expression_list (несколько значений).
        var targetItems = FlattenTuple(target);
        var valueItems = FlattenTuple(value);

        bool isTupleAssign = targetItems.Count > 1
                          || (valueItems.Count > 1 && targetItems.Count == 1);

        if (isTupleAssign)
        {
            // Случай 1: a, b = 1, 2 — просто два списка одинаковой длины
            if (targetItems.Count == valueItems.Count)
            {
                return new TupleAssignStatement
                {
                    Targets = targetItems.Select(ConvertLValue).ToList(),
                    Values = valueItems.Select(ConvertExpr).ToList()
                };
            }

            // Случай 2: a, b = функция(), возвращающая tuple — интерпретатор не умеет.
            // Это редко, кидаем ошибку с подсказкой.
            throw Err(
                $"Кортежное присваивание: {targetItems.Count} слева, {valueItems.Count} справа. " +
                $"Распаковка tuple-результата функции пока не поддерживается — " +
                $"присвойте результат переменной, затем разберите её вручную.",
                assign);
        }

        // Обычное присваивание
        return new AssignStatement
        {
            Target = ConvertLValue(target),
            Value = ConvertExpr(value)
        };
    }

    private YawaStatement? TryConvertSwap(TsNode target, TsNode value, TsNode node)
    {
        // A[i], A[j] = A[j], A[i] — pattern_list = expression_list
        // Проверяем структуру
        TsNode left = target;
        TsNode right = value;

        // Разворачиваем pattern_list / expression_list / tuple
        var leftItems = FlattenTuple(target);
        var rightItems = FlattenTuple(value);

        if (leftItems.Count != 2 || rightItems.Count != 2) return null;

        var l0 = leftItems[0];
        var l1 = leftItems[1];
        var r0 = rightItems[0];
        var r1 = rightItems[1];

        // A[i] и A[j] — subscript
        if (l0.Type != "subscript" || l1.Type != "subscript") return null;
        if (r0.Type != "subscript" || r1.Type != "subscript") return null;

        var l0Text = Text(l0);
        var l1Text = Text(l1);
        var r0Text = Text(r0);
        var r1Text = Text(r1);

        // Ожидаем: A[i], A[j] = A[j], A[i]
        if (l0Text == r1Text && l1Text == r0Text)
        {
            return new SwapStatement
            {
                A = ConvertExpr(l0),
                B = ConvertExpr(l1)
            };
        }

        return null;
    }

    private List<TsNode> FlattenTuple(TsNode node)
    {
        // Разворачиваем ТОЛЬКО настоящие tuple-паттерны:
        //   a, b = ...       → target: pattern_list
        //   x = 1, 2         → value:  expression_list
        // НЕ разворачиваем list [1,2,3] и (1,2) — это обычные значения.
        if (node.Type is "pattern_list" or "expression_list")
        {
            var result = new List<TsNode>();
            foreach (var c in node.NamedChildren())
                result.AddRange(FlattenTuple(c));
            return result;
        }
        return new List<TsNode> { node };
    }

    private YawaStatement ConvertAugmentedAssignment(TsNode aug)
    {
        // x += 1 → x = x + 1
        var children = aug.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректный augmented assignment", aug);

        var target = children[0];
        var value = children[children.Count - 1];

        // оператор извлекаем из исходника: например "+=" → "+"
        var op = ExtractAugmentedOperator(aug);
        if (op is null)
            throw Err("Не удалось разобрать оператор augmented assignment", aug);

        return new AssignStatement
        {
            Target = ConvertLValue(target),
            Value = new BinaryExpr
            {
                Op = op,
                A = ConvertExpr(target),
                B = ConvertExpr(value)
            }
        };
    }

    private string? ExtractAugmentedOperator(TsNode aug)
    {
        // Между target и value лежит оператор вида "+=", "-=", и т.д.
        // Проще всего взять текст и найти оператор
        var text = Text(aug);
        foreach (var op in new[] { "+=", "-=", "*=", "/=", "%=", "**=" })
        {
            if (text.Contains(op))
                return op[..^1]; // "+=" → "+"
        }
        return null;
    }

    private YawaStatement ConvertIf(TsNode node)
    {
        var children = node.NamedChildren().ToList();

        var condNode = children.FirstOrDefault(c => c.Type != "block"
                                                 && c.Type != "elif_clause"
                                                 && c.Type != "else_clause");
        var thenBlock = children.FirstOrDefault(c => c.Type == "block");
        var elifs = children.Where(c => c.Type == "elif_clause").ToList();
        var elseClause = children.FirstOrDefault(c => c.Type == "else_clause");

        var stmt = new IfStatement
        {
            Cond = ConvertExpr(condNode),
            Then = thenBlock.IsValid ? ConvertBlock(thenBlock) : new List<YawaStatement>()
        };

        // Строим цепочку else от последнего elif к первому (снизу вверх).
        List<YawaStatement>? elseBranch = null;

        if (elseClause.IsValid)
        {
            var elseBlock = elseClause.NamedChildren().FirstOrDefault(c => c.Type == "block");
            if (elseBlock.IsValid) elseBranch = ConvertBlock(elseBlock);
        }

        for (int i = elifs.Count - 1; i >= 0; i--)
        {
            var e = elifs[i];
            var ec = e.NamedChildren().ToList();
            var econd = ec.FirstOrDefault(c => c.Type != "block");
            var eblock = ec.FirstOrDefault(c => c.Type == "block");

            var nested = new IfStatement
            {
                Cond = ConvertExpr(econd),
                Then = eblock.IsValid ? ConvertBlock(eblock) : new List<YawaStatement>()
            };

            if (elseBranch is not null) nested.Else = elseBranch;
            elseBranch = new List<YawaStatement> { nested };
        }

        if (elseBranch is not null) stmt.Else = elseBranch;
        return stmt;
    }

    private YawaStatement ConvertElif(TsNode elif)
    {
        var children = elif.NamedChildren().ToList();
        var condNode = children.FirstOrDefault(c => c.Type != "block"
                                                 && c.Type != "else_clause");
        var thenBlock = children.FirstOrDefault(c => c.Type == "block");
        var elseClause = children.FirstOrDefault(c => c.Type == "else_clause");

        var stmt = new IfStatement
        {
            Cond = ConvertExpr(condNode),
            Then = thenBlock.IsValid ? ConvertBlock(thenBlock) : new List<YawaStatement>()
        };

        if (elseClause.IsValid)
        {
            var elseBlock = elseClause.NamedChildren().FirstOrDefault(c => c.Type == "block");
            if (elseBlock.IsValid)
                stmt.Else = ConvertBlock(elseBlock);
        }

        return stmt;
    }

    private YawaStatement ConvertWhile(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        var condNode = children.FirstOrDefault(c => c.Type != "block");
        var block = children.FirstOrDefault(c => c.Type == "block");

        return new WhileStatement
        {
            Cond = ConvertExpr(condNode),
            Body = block.IsValid ? ConvertBlock(block) : new List<YawaStatement>()
        };
    }

    private YawaStatement ConvertFor(TsNode node)
    {
        // for i in range(...):   → ForStatement
        // for x in seq:          → ForeachStatement

        // В tree-sitter-python for_statement имеет дочерние узлы: identifier, "in", iterable, block
        // Мы получаем через NamedChildren только identifier и iterable и block
        var children = node.NamedChildren().ToList();

        // Первый identifier — переменная цикла.
        // Второй named child, не являющийся block и не identifier цикла — iterable.
        // Внимание: iterable тоже может быть identifier (`for ch in s`).
        int varIdx = -1;
        for (int k = 0; k < children.Count; k++)
        {
            if (children[k].Type == "identifier") { varIdx = k; break; }
        }

        if (varIdx < 0)
            throw Err("Некорректный for-loop (нет переменной цикла)", node);

        TsNode iterable = default;
        for (int k = varIdx + 1; k < children.Count; k++)
        {
            if (children[k].Type != "block") { iterable = children[k]; break; }
        }

        var block = children.FirstOrDefault(c => c.Type == "block");

        if (!iterable.IsValid)
            throw Err("Некорректный for-loop (нет iterable)", node);

        var varNode = children[varIdx];

        var varName = Text(varNode);
        var body = block.IsValid ? ConvertBlock(block) : new List<YawaStatement>();

        // range(...) → ForStatement
        if (iterable.Type == "call")
        {
            var callee = iterable.NamedChildren().FirstOrDefault();
            if (callee.IsValid && callee.Type == "identifier" && Text(callee) == "range")
            {
                var argsList = iterable.NamedChildren().FirstOrDefault(c => c.Type == "argument_list");
                var args = argsList.IsValid
                    ? argsList.NamedChildren().Select(ConvertExpr).ToList()
                    : new List<YawaExpression>();

                switch (args.Count)
                {
                    case 1:
                        return new ForStatement
                        {
                            Var = varName,
                            From = new LiteralExpr { Value = ToJsonElement(0) },
                            To = args[0],
                            Body = body
                        };
                    case 2:
                        return new ForStatement
                        {
                            Var = varName,
                            From = args[0],
                            To = args[1],
                            Body = body
                        };
                    case 3:
                        return new ForStatement
                        {
                            Var = varName,
                            From = args[0],
                            To = args[1],
                            Step = args[2],
                            Body = body
                        };
                    default:
                        throw Err($"range() принимает 1-3 аргумента, получено {args.Count}", iterable);
                }
            }
        }

        // foreach по коллекции
        return new ForeachStatement
        {
            Var = varName,
            In = ConvertExpr(iterable),
            Body = body
        };
    }

    // ─────────── Expressions ───────────

    private YawaExpression ConvertExpr(TsNode node)
    {
        switch (node.Type)
        {
            case "integer":
                return new LiteralExpr { Value = ToJsonElement(long.Parse(Text(node))) };

            case "float":
                return new LiteralExpr
                {
                    Value = ToJsonElement(double.Parse(Text(node),
                    System.Globalization.CultureInfo.InvariantCulture))
                };

            case "string":
                return new LiteralExpr { Value = ToJsonElement(ParseStringLiteral(Text(node))) };

            case "true":
                return new LiteralExpr { Value = ToJsonElement(true) };

            case "false":
                return new LiteralExpr { Value = ToJsonElement(false) };

            case "none":
                return new LiteralExpr { Value = ToJsonElement<object?>(null) };

            case "identifier":
                return new RefExpr { Name = Text(node) };

            case "slice":
                return ConvertSlice(node);

            case "subscript":
                return ConvertSubscript(node);

            case "attribute":
                return ConvertAttribute(node);

            case "binary_operator":
                return ConvertBinary(node);

            case "unary_operator":
                return ConvertUnary(node);

            case "comparison_operator":
                return ConvertComparison(node);

            case "boolean_operator":
                return ConvertBoolean(node);

            case "not_operator":
                {
                    var arg = node.NamedChildren().FirstOrDefault();
                    return new UnaryExpr
                    {
                        Op = "not",
                        A = arg.IsValid ? ConvertExpr(arg) : new LiteralExpr { Value = ToJsonElement(false) }
                    };
                }

            case "call":
                return ConvertCall(node);

            case "dictionary":
                return ConvertDict(node);

            case "list":
                return ConvertListLiteral(node);

            case "parenthesized_expression":
                {
                    var inner = node.NamedChildren().FirstOrDefault();
                    if (!inner.IsValid)
                        throw Err("Пустое выражение в скобках", node);
                    return ConvertExpr(inner);
                }

            case "conditional_expression":
                return ConvertTernary(node);

            default:
                throw Err($"Неподдерживаемое выражение: {node.Type}", node);
        }
    }

    private YawaExpression ConvertSlice(TsNode node)
    {
        // В tree-sitter slice может иметь разный состав детей:
        // [start:stop]   → [start, stop]
        // [:stop]        → [stop]     (нет start)
        // [start:]       → [start]    (нет stop)
        // [:]            → []         (оба отсутствуют)
        // [start:stop:step] → [start, stop, step]
        // Эвристика: используем текстовое представление
        var text = Text(node);
        var parts = text.Split(':');

        // Части — как текст, но нужны TsNode. Идём через named children + text.
        // Проще: разбираем по named children и сопоставляем с текстом.
        // tree-sitter-python использует анонимные ":" как разделители,
        // поэтому NamedChildren даёт только реально присутствующие выражения.
        var children = node.NamedChildren().ToList();

        // Определяем, какие компоненты пропущены — по позициям ":" в тексте.
        // Более надёжно: смотрим позицию каждого named-child относительно узла.

        // Находим позиции `:` в исходнике узла (в байтах)
        var colons = new List<int>();
        for (int i = 0; i < text.Length; i++)
            if (text[i] == ':') colons.Add(i);

        YawaExpression? start = null, stop = null, step = null;

        // Сопоставляем детей с позициями в тексте.
        // Каждый child знает свою StartByte/EndByte — приводим к относительному offset.
        var nodeStart = (int)node.StartByte;
        var childCols = children.Select(c => new
        {
            Node = c,
            Offset = (int)c.StartByte - nodeStart
        }).ToList();

        // Определяем, в какую «щель» попал каждый ребёнок.
        // Между colons[i-1] и colons[i] — i-й сегмент (0 = start, 1 = stop, 2 = step).
        foreach (var cc in childCols)
        {
            int seg = 0;
            if (colons.Count > 0 && cc.Offset > colons[0]) seg = 1;
            if (colons.Count > 1 && cc.Offset > colons[1]) seg = 2;

            var expr = ConvertExpr(cc.Node);
            switch (seg)
            {
                case 0: start = expr; break;
                case 1: stop = expr; break;
                case 2: step = expr; break;
            }
        }

        return new SliceExpr
        {
            Target = new RefExpr { Name = "<slice-root>" },
            Start = start,
            Stop = stop,
            Step = step
        };
    }

    private YawaExpression ConvertSubscript(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректный subscript", node);

        var target = children[0];
        var index = children[1];

        // A[start:stop] — tree-sitter кладёт slice как второй ребёнок
        if (index.Type == "slice")
        {
            var slice = (SliceExpr)ConvertSlice(index);
            slice.Target = ConvertExpr(target);
            return slice;
        }

        return new IndexExpr
        {
            Target = ConvertExpr(target),
            Index = ConvertExpr(index)
        };
    }

    private YawaExpression ConvertAttribute(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректный attribute access", node);

        var target = children[0];
        var nameNode = children[1];

        if (nameNode.Type != "identifier")
            throw Err("Ожидалось имя поля после точки", nameNode);

        return new FieldExpr
        {
            Target = ConvertExpr(target),
            FieldName = Text(nameNode)
        };
    }

    private YawaExpression ConvertBinary(TsNode node)
    {
        var text = Text(node);
        string? op = null;
        int opIdx = -1;

        // Определяем оператор по тексту
        foreach (var candidate in new[] { "**", "//", "+", "-", "*", "/", "%" })
        {
            opIdx = text.IndexOf(candidate, StringComparison.Ordinal);
            if (opIdx >= 0) { op = candidate; break; }
        }

        if (op is null)
            throw Err($"Не удалось определить бинарный оператор в '{text}'", node);

        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректная бинарная операция", node);

        var a = children[0];
        var b = children[children.Count - 1];

        // // (целочисленное деление) в YAWA нет — эмулируем через floor
        // Пока просто делаем вид что это "/"
        if (op == "//") op = "/";

        return new BinaryExpr
        {
            Op = op,
            A = ConvertExpr(a),
            B = ConvertExpr(b)
        };
    }

    private YawaExpression ConvertUnary(TsNode node)
    {
        var text = Text(node);
        var op = text.StartsWith("-") ? "-" : "+";
        var child = node.NamedChildren().FirstOrDefault();

        return new UnaryExpr
        {
            Op = op,
            A = child.IsValid ? ConvertExpr(child) : new LiteralExpr { Value = ToJsonElement(0) }
        };
    }

    private YawaExpression ConvertComparison(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректное сравнение", node);

        var text = Text(node);

        string? op = null;
        foreach (var candidate in new[] { "==", "!=", "<=", ">=", "<", ">", "not in", "in" })
        {
            if (text.Contains(candidate))
            {
                op = candidate;
                break;
            }
        }

        // Python: `x is None` / `x is not None` / `x is y`
        // tree-sitter даёт это как comparison_operator, но оператор "is" / "is not"
        if (text.Contains(" is not "))
        {
            var a1 = children[0];
            var b1 = children[^1];
            return new BinaryExpr
            {
                Op = "!=",
                A = ConvertExpr(a1),
                B = ConvertExpr(b1)
            };
        }
        if (text.Contains(" is "))
        {
            var a1 = children[0];
            var b1 = children[^1];
            return new BinaryExpr
            {
                Op = "==",
                A = ConvertExpr(a1),
                B = ConvertExpr(b1)
            };
        }

        if (op is null)
            throw Err($"Неизвестный оператор сравнения: {text}", node);

        // Приведение Python-стиля к YAWA
        var yawaOp = op switch
        {
            "==" => "==",
            "!=" => "!=",
            "<" => "<",
            "<=" => "<=",
            ">" => ">",
            ">=" => ">=",
            "in" => "in",
            "not in" => "not_in",
            _ => op
        };

        // Цепочки сравнений типа a < b < c пока поддерживаем только попарно —
        // берём первые два и последний
        var a = children[0];
        var b = children.Count == 2 ? children[1] : children[1];

        return new BinaryExpr
        {
            Op = yawaOp,
            A = ConvertExpr(a),
            B = ConvertExpr(b)
        };
    }

    private YawaExpression ConvertBoolean(TsNode node)
    {
        var text = Text(node);
        var op = text.Contains(" and ") ? "and" : "or";

        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректное логическое выражение", node);

        return new BinaryExpr
        {
            Op = op,
            A = ConvertExpr(children[0]),
            B = ConvertExpr(children[^1])
        };
    }

    private YawaExpression ConvertCall(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        var callee = children.FirstOrDefault();
        var argsList = children.FirstOrDefault(c => c.Type == "argument_list");

        if (!callee.IsValid)
            throw Err("Некорректный вызов", node);

        var args = argsList.IsValid
            ? argsList.NamedChildren()
                .Where(a => a.Type != "comment")
                .Select(ConvertExpr).ToList()
            : new List<YawaExpression>();

        // Встроенные: len → length, append → push, pop → pop, и т.д.
        if (callee.Type == "identifier")
        {
            var name = Text(callee);
            var mapped = name switch
            {
                "len" => "length",
                "range" => "range",
                "min" => "min",
                "max" => "max",
                "abs" => "abs",
                "print" => "print",
                _ => name
            };
            return new CallExpr { Name = mapped, Args = args };
        }

        // Методы: A.append(x) → push(A, x); A.pop() → pop(A)
        if (callee.Type == "attribute")
        {
            var attrChildren = callee.NamedChildren().ToList();
            if (attrChildren.Count < 2)
                throw Err("Некорректный доступ к методу", callee);

            var receiver = attrChildren[0];
            var methodName = Text(attrChildren[1]);

            var mapped = methodName switch
            {
                "append" => "push",
                "pop" => "pop",
                "insert" => "insert",
                "remove" => "remove",
                _ => methodName
            };

            // receiver передаём первым аргументом
            var allArgs = new List<YawaExpression> { ConvertExpr(receiver) };
            allArgs.AddRange(args);

            return new CallExpr { Name = mapped, Args = allArgs };
        }

        throw Err("Неподдерживаемый вызываемый объект", callee);
    }

    private YawaExpression ConvertDict(TsNode node)
    {
        var result = new DictExpr();
        foreach (var child in node.NamedChildren())
        {
            if (child.Type != "pair")
            {
                // пропускаем комментарии
                continue;
            }
            var pairChildren = child.NamedChildren().ToList();
            if (pairChildren.Count < 2)
                throw Err("Некорректная пара ключ:значение в словаре", child);

            result.Items.Add(new DictEntry
            {
                Key = ConvertExpr(pairChildren[0]),
                Value = ConvertExpr(pairChildren[1])
            });
        }
        return result;
    }

    private YawaExpression ConvertListLiteral(TsNode node)
    {
        var items = node.NamedChildren()
            .Where(c => c.Type != "comment")
            .ToList();

        // Пустой список или только литералы → LiteralExpr с массивом
        if (items.All(x => x.Type is "integer" or "float" or "string" or "true" or "false" or "none"))
            return new LiteralExpr { Value = ToJsonElement(BuildArray(items)) };

        // Иначе — ArrayExpr с подвыражениями
        return new ArrayExpr
        {
            Items = items.Select(ConvertExpr).ToList()
        };
    }

    private object? BuildArray(List<TsNode> items)
    {
        var list = new List<object?>();
        foreach (var it in items)
        {
            list.Add(it.Type switch
            {
                "integer" => long.Parse(Text(it)),
                "float" => double.Parse(Text(it), System.Globalization.CultureInfo.InvariantCulture),
                "string" => ParseStringLiteral(Text(it)),
                "true" => true,
                "false" => false,
                "none" => null,
                _ => throw Err($"Неизвестный литерал: {it.Type}", it)
            });
        }
        return list;
    }

    private YawaExpression ConvertTernary(TsNode node)
    {
        // then if cond else otherwise
        var children = node.NamedChildren().ToList();
        if (children.Count < 3)
            throw Err("Некорректный тернарный оператор", node);

        // Порядок: then, cond, else
        return new TernaryExpr
        {
            Parts = new TernaryParts
            {
                Then = ConvertExpr(children[0]),
                Cond = ConvertExpr(children[1]),
                Else = ConvertExpr(children[2])
            }
        };
    }

    private YawaExpression ConvertLValue(TsNode node)
    {
        // LValue — та же структура, что и обычное выражение, но интерпретатор
        // поймёт его как target присваивания.
        return ConvertExpr(node);
    }

    // ─────────── Специальные визуальные функции ───────────

    private YawaStatement? TryConvertSpecialCall(TsNode node)
    {
        if (node.Type != "call") return null;

        var callee = node.NamedChildren().FirstOrDefault();
        if (!callee.IsValid || callee.Type != "identifier") return null;

        var name = Text(callee);
        var argsList = node.NamedChildren().FirstOrDefault(c => c.Type == "argument_list");
        var args = argsList.IsValid
            ? argsList.NamedChildren().Select(ConvertExpr).ToList()
            : new List<YawaExpression>();

        switch (name)
        {
            case "swap":
                if (args.Count != 3)
                    throw Err("swap(A, i, j) требует 3 аргумента", node);
                return new SwapStatement
                {
                    A = new IndexExpr { Target = args[0], Index = args[1] },
                    B = new IndexExpr { Target = args[0], Index = args[2] }
                };

            case "compare":
                if (args.Count != 2)
                    throw Err("compare(a, b) требует 2 аргумента", node);
                return new CompareStatement
                {
                    A = args[0],
                    B = args[1],
                    Result = "?",
                    Label = null
                };

            case "mark":
                if (args.Count < 3)
                    throw Err("mark(A, i, \"color\") требует 3 аргумента", node);
                return new MarkStatement
                {
                    Target = new IndexExpr { Target = args[0], Index = args[1] },
                    Color = (args[2] as LiteralExpr)?.Value.ToString() ?? "yellow"
                };

            case "annotate":
                if (args.Count != 1)
                    throw Err("annotate(\"текст\") требует 1 аргумент", node);
                var text = (args[0] as LiteralExpr)?.Value.ToString() ?? "";
                return new AnnotateStatement { Text = text };

            default:
                return null;
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

    private string ParseStringLiteral(string raw)
    {
        // Простая обработка: убираем обрамляющие кавычки, разэкранируем
        if (raw.Length < 2) return raw;
        var quote = raw[0];
        var inner = raw.Substring(1, raw.Length - 2);

        var sb = new StringBuilder();
        for (int i = 0; i < inner.Length; i++)
        {
            if (inner[i] == '\\' && i + 1 < inner.Length)
            {
                i++;
                sb.Append(inner[i] switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\\' => '\\',
                    '"' => '"',
                    '\'' => '\'',
                    _ => inner[i]
                });
            }
            else sb.Append(inner[i]);
        }
        return sb.ToString();
    }

    private UnsupportedFeatureException Err(string msg, TsNode node)
    {
        var p = node.StartPoint;
        return new UnsupportedFeatureException(msg, (int)p.Row, (int)p.Col);
    }

    private static System.Text.Json.JsonElement ToJsonElement<T>(T value)
    {
        var json = System.Text.Json.JsonSerializer.SerializeToElement(value);
        return json;
    }
}