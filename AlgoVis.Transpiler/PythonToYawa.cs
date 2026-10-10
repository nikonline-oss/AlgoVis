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
    /// <summary>Стек префиксов вложенных функций. Верхний элемент — текущий.</summary>
    private readonly Stack<string> _prefixStack = new();

    /// <summary>Карта переименования: имя внутри функции → полное имя top-level.</summary>
    private readonly Stack<Dictionary<string, string>> _renameStack = new();

    private int _tmpCounter;
    private readonly HashSet<string> _classNames = new(StringComparer.Ordinal);
    // Для каждого класса — методы по короткому имени (без ClassName.).
    private readonly Dictionary<string, Dictionary<string, YawaFunction>> _classMethodMap
        = new(StringComparer.Ordinal);


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

        // Первый проход: собираем имена классов, чтобы знать,
        // где call это instantiate, а где — обычная функция.
        foreach (var child in root.NamedChildren())
        {
            if (child.Type == "class_definition")
            {
                var nameNode = child.NamedChildren().FirstOrDefault(c => c.Type == "identifier");
                if (nameNode.IsValid)
                    _classNames.Add(Text(nameNode));
            }
        }

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

                    // Добавляем все nested функции и создаём алиасы-обёртки.
                    foreach (var nested in _pendingNestedFunctions)
                    {
                        if (_functionNames.Add(nested.Name))
                            program.Functions.Add(nested);

                        // Извлекаем короткое имя (после последнего "__") и создаём алиас,
                        // если такого top-level имени ещё нет.
                        var shortName = nested.Name.Contains("__")
                            ? nested.Name[(nested.Name.LastIndexOf("__", StringComparison.Ordinal) + 2)..]
                            : nested.Name;

                        if (shortName != nested.Name && !_functionNames.Contains(shortName))
                        {
                            var wrapper = new YawaFunction
                            {
                                Name = shortName,
                                Params = nested.Params.Select(p => new YawaParam { Name = p.Name }).ToList(),
                                Returns = nested.Returns
                            };

                            // return outer__inner(p1, p2, ...)
                            wrapper.Body.Add(new ReturnStatement
                            {
                                Value = new CallExpr
                                {
                                    Name = nested.Name,
                                    Args = nested.Params
                                        .Select(p => (YawaExpression)new RefExpr { Name = p.Name })
                                        .ToList()
                                }
                            });

                            program.Functions.Add(wrapper);
                            _functionNames.Add(shortName);
                        }
                    }
                    _pendingNestedFunctions.Clear();
                    break;

                case "class_definition":
                    {
                        var methods = ConvertClass(child);
                        foreach (var m in methods)
                        {
                            if (_functionNames.Add(m.Name))
                                program.Functions.Add(m);
                        }
                        break;
                    }

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
        var newPrefix = string.IsNullOrEmpty(savedPrefix)
            ? fn.Name
            : savedPrefix + "__" + fn.Name;
        _currentFunctionPrefix = newPrefix;

        var renameMap = new Dictionary<string, string>(StringComparer.Ordinal);
        if (block.IsValid)
        {
            foreach (var child in block.NamedChildren())
            {
                if (child.Type == "function_definition")
                {
                    var childName = child.NamedChildren()
                        .FirstOrDefault(c => c.Type == "identifier");
                    if (childName.IsValid)
                    {
                        var n = Text(childName);
                        renameMap[n] = newPrefix + "__" + n;
                    }
                }
            }
        }

        _renameStack.Push(renameMap);
        try
        {

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
                                if (p.Type == "typed_parameter")
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
                                // *args
                                var inner = p.NamedChildren().FirstOrDefault();
                                if (inner.IsValid && inner.Type == "identifier")
                                {
                                    fn.Params.Add(new YawaParam
                                    {
                                        Name = Text(inner),
                                        IsVariadic = true
                                    });
                                }
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

                        case "list_splat_pattern":

                        case "dictionary_splat_pattern":
                            {
                                var inner = p.NamedChildren().FirstOrDefault();
                                if (inner.IsValid && inner.Type == "identifier")
                                {
                                    fn.Params.Add(new YawaParam
                                    {
                                        Name = Text(inner),
                                        IsKeywordVariadic = true
                                    });
                                }
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
        }
        finally
        {
            _renameStack.Pop();
            _currentFunctionPrefix = savedPrefix;
        }

        return fn;
    }

    /// <summary>
    /// Конвертирует класс в набор top-level функций:
    ///   ClassName__init, ClassName__method1, ...
    /// Сами классы в YAWA не существуют — есть только объекты с полем __type__.
    /// </summary>
    private List<YawaFunction> ConvertClass(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        var nameNode = children.FirstOrDefault(c => c.Type == "identifier");
        var block = children.FirstOrDefault(c => c.Type == "block");

        if (!nameNode.IsValid)
            throw Err("Класс без имени", node);

        var className = Text(nameNode);
        var result = new List<YawaFunction>();

        // Собираем базовые классы (аргументы в скобках после имени)
        var bases = new List<string>();
        var argList = children.FirstOrDefault(c => c.Type == "argument_list");
        if (argList.IsValid)
        {
            foreach (var b in argList.NamedChildren())
                if (b.Type == "identifier") bases.Add(Text(b));
        }

        if (!block.IsValid) return result;

        // Собственные методы класса
        foreach (var stmt in block.NamedChildren())
        {
            if (stmt.Type == "function_definition")
            {
                _pendingNestedFunctions.Clear();
                var fn = ConvertFunction(stmt);
                fn.Name = $"{className}.{fn.Name}";
                result.Add(fn);
                foreach (var nested in _pendingNestedFunctions) result.Add(nested);
                _pendingNestedFunctions.Clear();
            }
            else if (stmt.Type == "decorated_definition")
            {
                var inner = stmt.NamedChildren()
                    .FirstOrDefault(n => n.Type == "function_definition");
                if (!inner.IsValid) continue;

                _pendingNestedFunctions.Clear();
                var fn = ConvertFunction(inner);
                fn.Name = $"{className}.{fn.Name}";
                result.Add(fn);
                _pendingNestedFunctions.Clear();
            }
        }

        // Собираем короткие имена собственных методов
        var ownNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var f in result)
        {
            if (f.Name.StartsWith(className + ".", StringComparison.Ordinal))
                ownNames.Add(f.Name.Substring(className.Length + 1));
        }

        // Копируем унаследованные методы из баз
        foreach (var baseName in bases)
        {
            if (!_classMethodMap.TryGetValue(baseName, out var baseMethods)) continue;

            foreach (var (shortName, baseFn) in baseMethods)
            {
                if (ownNames.Contains(shortName)) continue;

                var clone = new YawaFunction
                {
                    Name = $"{className}.{shortName}",
                    Params = baseFn.Params.Select(p => new YawaParam
                    {
                        Name = p.Name,
                        Type = p.Type,
                        DefaultValue = p.DefaultValue,
                        IsVariadic = p.IsVariadic,
                        IsKeywordVariadic = p.IsKeywordVariadic
                    }).ToList(),
                    Returns = baseFn.Returns,
                    Body = baseFn.Body
                };
                result.Add(clone);
            }
        }

        // Сохраняем в карту для будущих наследников
        var methodDict = new Dictionary<string, YawaFunction>(StringComparer.Ordinal);
        foreach (var f in result)
        {
            if (f.Name.StartsWith(className + ".", StringComparison.Ordinal))
                methodDict[f.Name.Substring(className.Length + 1)] = f;
        }
        _classMethodMap[className] = methodDict;

        return result;
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
                    if (!val.IsValid)
                        return new ReturnStatement { Value = null };

                    // return a, b  →  return tuple(a, b)
                    if (val.Type == "expression_list")
                    {
                        var items = val.NamedChildren().Select(ConvertExpr).ToList();
                        return new ReturnStatement
                        {
                            Value = new TupleExpr { Items = items }
                        };
                    }

                    return new ReturnStatement { Value = ConvertExpr(val) };
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

            case "class_definition":
                throw Err(
                    "Классы не поддерживаются транспайлером. " +
                    "Используйте словари или именованные структуры (dict/tuple).",
                    node);
            case "assert_statement":
                return ConvertAssert(node);

            case "global_statement":
            case "nonlocal_statement":
                return null;

            case "try_statement":
                return ConvertTry(node);

            case "delete_statement":
                return ConvertDelete(node);

            default:
                throw Err($"Неподдерживаемый statement: {node.Type}", node);
        }
    }
    private YawaStatement ConvertAssert(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count == 0)
            throw Err("assert без условия", node);

        return new AssertStatement
        {
            Cond = ConvertExpr(children[0]),
            Message = children.Count > 1 ? ConvertExpr(children[1]) : null
        };
    }

    private YawaStatement ConvertDelete(TsNode node)
    {
        var targets = node.NamedChildren()
            .Where(c => c.Type != "comment")
            .Select(ConvertExpr)
            .ToList();

        return new DeleteStatement { Targets = targets };
    }

    private YawaStatement ConvertAssignment(TsNode assign)
    {
        var children = assign.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректное присваивание", assign);

        var target = children[0];
        var value = children[children.Count - 1];

        // Цепочка: a = b = c = value
        // value при этом — assignment
        if (value.Type == "assignment")
        {
            var targets = new List<TsNode> { target };
            var cur = value;
            while (cur.Type == "assignment")
            {
                var c = cur.NamedChildren().ToList();
                targets.Add(c[0]);
                cur = c[c.Count - 1];
            }
            var last = ConvertExpr(cur);
            return new TupleAssignStatement
            {
                Targets = targets.Select(ConvertLValue).ToList(),
                Values = targets.Select(_ => last).ToList()
            };
        }

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
            // Есть ли *rest среди target-ов?
            int splatIdx = -1;
            for (int i = 0; i < targetItems.Count; i++)
            {
                if (targetItems[i].Type == "list_splat_pattern")
                {
                    if (splatIdx >= 0)
                        throw Err("Множественный * в распаковке не поддерживается",
                                  targetItems[i]);
                    splatIdx = i;
                }
            }

            // Значения формируем как обычно
            List<YawaExpression> values;
            if (targetItems.Count == valueItems.Count)
            {
                values = valueItems.Select(ConvertExpr).ToList();
            }
            else if (valueItems.Count == 1)
            {
                values = new List<YawaExpression> { ConvertExpr(valueItems[0]) };
            }
            else
            {
                throw Err(
                    $"Несовпадение количества при распаковке: {targetItems.Count} слева, " +
                    $"{valueItems.Count} справа", assign);
            }

            // Цели: splat превращаем в обычный идентификатор (имя) — значение подставим как массив
            var targets = new List<YawaExpression>();
            foreach (var ti in targetItems)
            {
                if (ti.Type == "list_splat_pattern")
                {
                    var inner = ti.NamedChildren().FirstOrDefault();
                    if (!inner.IsValid || inner.Type != "identifier")
                        throw Err("Ожидалось имя после * в распаковке", ti);
                    targets.Add(new RefExpr { Name = Text(inner) });
                }
                else
                {
                    targets.Add(ConvertLValue(ti));
                }
            }

            return new TupleAssignStatement
            {
                Targets = targets,
                Values = values,
                SplatIndex = splatIdx
            };
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

    private YawaStatement ConvertTry(TsNode node)
    {
        var children = node.NamedChildren().ToList();

        var bodyBlock = children.FirstOrDefault(c => c.Type == "block");
        var finallyClause = children.FirstOrDefault(c => c.Type == "finally_clause");

        var stmt = new TryStatement
        {
            Body = bodyBlock.IsValid ? ConvertBlock(bodyBlock) : new List<YawaStatement>()
        };

        // except_clause — может быть несколько
        foreach (var c in children)
        {
            if (c.Type != "except_clause") continue;

            var handler = new ExceptHandler();
            var cc = c.NamedChildren().ToList();

            // Опциональная переменная: `except ValueError as e:` — структура
            // except_clause → identifier (ValueError) → identifier (e)
            // Простая форма: `except:` — ничего лишнего
            // Мы тип не различаем, но имя переменной вытащим, если есть.
            var varName = ExtractExceptVar(c);
            handler.VarName = varName;

            var hblock = cc.FirstOrDefault(x => x.Type == "block");
            if (hblock.IsValid)
                handler.Body = ConvertBlock(hblock);

            stmt.Handlers.Add(handler);
        }

        if (finallyClause.IsValid)
        {
            var fblock = finallyClause.NamedChildren().FirstOrDefault(c => c.Type == "block");
            if (fblock.IsValid)
                stmt.Finally = ConvertBlock(fblock);
        }

        return stmt;
    }

    private string? ExtractExceptVar(TsNode exceptClause)
    {
        // `except ValueError as e:` → tree-sitter: except_clause → [identifier, identifier, block]
        // Не всегда удаётся отличить тип от имени. Упрощённо:
        //   если есть 2 identifier до block, второй — имя переменной
        //   если 1 identifier до block, это либо тип, либо имя (берём как имя только если есть 'as' в тексте)
        var text = Text(exceptClause);
        if (!text.Contains(" as ")) return null;

        var ids = exceptClause.NamedChildren()
            .Where(c => c.Type == "identifier")
            .ToList();
        // Последний identifier перед block — имя переменной
        return ids.Count >= 1 ? Text(ids[^1]) : null;
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
        var children = node.NamedChildren().ToList();

        // Левая часть for может быть:
        //   identifier  — for i in ...
        //   pattern_list / tuple_pattern — for k, v in ...
        int varIdx = -1;
        string varKind = "";
        for (int k = 0; k < children.Count; k++)
        {
            var t = children[k].Type;
            if (t == "identifier") { varIdx = k; varKind = "identifier"; break; }
            if (t == "pattern_list" || t == "tuple_pattern") { varIdx = k; varKind = "pattern"; break; }
        }

        if (varIdx < 0)
            throw Err("Некорректный for-loop (нет переменной цикла)", node);

        TsNode iterable = default;
        for (int k = varIdx + 1; k < children.Count; k++)
        {
            if (children[k].Type != "block") { iterable = children[k]; break; }
        }

        var block = children.FirstOrDefault(c => c.Type == "block");
        var varNode = children[varIdx];

        if (!iterable.IsValid)
            throw Err("Некорректный for-loop (нет iterable)", node);

        // Распаковка: for k, v in pairs → for __tmp in pairs: k = __tmp[0]; v = __tmp[1]
        if (varKind == "pattern")
        {
            var names = varNode.NamedChildren()
                .Where(c => c.Type == "identifier")
                .Select(Text)
                .ToList();

            if (names.Count == 0)
                throw Err("Не найдены переменные в распаковке for", varNode);

            var tmpVar = $"__tup_{Interlocked.Increment(ref _tmpCounter)}";
            var realBody = block.IsValid ? ConvertBlock(block) : new List<YawaStatement>();

            var newBody = new List<YawaStatement>();
            for (int i = 0; i < names.Count; i++)
            {
                newBody.Add(new AssignStatement
                {
                    Target = new RefExpr { Name = names[i] },
                    Value = new IndexExpr
                    {
                        Target = new RefExpr { Name = tmpVar },
                        Index = new LiteralExpr { Value = ToJsonElement(i) }
                    }
                });
            }
            newBody.AddRange(realBody);

            return new ForeachStatement
            {
                Var = tmpVar,
                In = ConvertExpr(iterable),
                Body = newBody
            };
        }

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
                return ConvertIdentifier(node);

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

            case "set":
                return ConvertSet(node);

            case "tuple":
                return ConvertTuple(node);

            case "list_comprehension":
                return ConvertListComp(node);

            case "dictionary_comprehension":
                return ConvertDictComp(node);

            case "set_comprehension":
                return ConvertSetComp(node);

            case "generator_expression":
                // sum(x*x for x in A) → sum([x*x for x in A])
                return ConvertListComp(node);

            case "lambda":
                return ConvertLambda(node);

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

    private YawaExpression ConvertIdentifier(TsNode node)
    {
        var name = Text(node);

        // Локальная функция (замыкание) — переименовываем через стек.
        var renamed = LookupRename(name);
        if (renamed != name)
            return new FunctionRefExpr { Name = renamed };

        // Top-level функция как значение.
        if (_functionNames.Contains(name) && !_classNames.Contains(name))
            return new FunctionRefExpr { Name = name };

        return new RefExpr { Name = name };
    }

    private YawaExpression ConvertLambda(TsNode node)
    {
        // lambda x: expr  |  lambda: expr  |  lambda x, y: expr
        var children = node.NamedChildren().ToList();

        TsNode paramsNode = default;
        TsNode body = default;

        foreach (var c in children)
        {
            if (c.Type == "lambda_parameters") paramsNode = c;
            else if (c.Type == "identifier" && !paramsNode.IsValid && children.Count == 2)
                paramsNode = c; // lambda x: ... — единственный параметр как identifier
            else if (body.Type == "") body = c;
        }

        // Иногда body — последний named child
        if (!body.IsValid)
        {
            var last = children.LastOrDefault();
            if (last.IsValid && last.Type != "lambda_parameters") body = last;
        }

        var name = $"__lambda_{Interlocked.Increment(ref _tmpCounter)}";

        var fn = new YawaFunction { Name = name };

        if (paramsNode.IsValid)
        {
            if (paramsNode.Type == "lambda_parameters")
            {
                foreach (var p in paramsNode.NamedChildren())
                {
                    if (p.Type == "identifier")
                        fn.Params.Add(new YawaParam { Name = Text(p) });
                }
            }
            else if (paramsNode.Type == "identifier")
            {
                fn.Params.Add(new YawaParam { Name = Text(paramsNode) });
            }
        }

        if (!body.IsValid)
            throw Err("Некорректная lambda — нет тела", node);

        fn.Body.Add(new ReturnStatement { Value = ConvertExpr(body) });
        _pendingNestedFunctions.Add(fn);

        return new FunctionRefExpr { Name = name };
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

    private YawaExpression ConvertTuple(TsNode node)
    {
        var items = node.NamedChildren()
            .Where(c => c.Type != "comment")
            .Select(ConvertExpr)
            .ToList();
        return new TupleExpr { Items = items };
    }

    private YawaExpression ConvertListComp(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректный list comprehension", node);

        var body = children[0];
        var clauses = ParseCompClauses(children.Skip(1).ToList(), node);
        if (clauses.Count == 0)
            throw Err("В comprehension нет 'for'", node);

        return new ListCompExpr
        {
            Body = ConvertExpr(body),
            Clauses = clauses
        };
    }

    private YawaExpression ConvertSetComp(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректный set comprehension", node);

        var body = children[0];
        var clauses = ParseCompClauses(children.Skip(1).ToList(), node);
        if (clauses.Count == 0)
            throw Err("В set comprehension нет 'for'", node);

        return new SetCompExpr
        {
            Body = ConvertExpr(body),
            Clauses = clauses
        };
    }

    private YawaExpression ConvertDictComp(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректный dict comprehension", node);

        var pair = children[0];
        if (pair.Type != "pair")
            throw Err("Ожидалась пара ключ:значение", pair);

        var pairChildren = pair.NamedChildren().ToList();
        if (pairChildren.Count < 2)
            throw Err("Некорректная пара в dict comprehension", pair);

        var clauses = ParseCompClauses(children.Skip(1).ToList(), node);
        if (clauses.Count == 0)
            throw Err("В dict comprehension нет 'for'", node);

        return new DictCompExpr
        {
            BodyKey = ConvertExpr(pairChildren[0]),
            BodyValue = ConvertExpr(pairChildren[1]),
            Clauses = clauses
        };
    }

    private List<CompClause> ParseCompClauses(List<TsNode> children, TsNode context)
    {
        var clauses = new List<CompClause>();

        foreach (var c in children)
        {
            if (c.Type == "for_in_clause")
            {
                var fc = c.NamedChildren().ToList();
                if (fc.Count < 2)
                    throw Err("Некорректный 'for' в comprehension", c);

                var varNode = fc[0];
                var iterable = fc[1];

                if (varNode.Type != "identifier")
                    throw Err("Переменная цикла должна быть identifier " +
                              "(распаковка tuple в comprehension пока не поддерживается)", varNode);

                var clause = new CompClause
                {
                    Var = Text(varNode),
                    Source = ConvertExpr(iterable)
                };

                // if внутри for_in_clause
                var innerIf = fc.FirstOrDefault(x => x.Type == "if_clause");
                if (innerIf.IsValid)
                {
                    var cond = innerIf.NamedChildren().FirstOrDefault();
                    if (cond.IsValid) clause.Filter = ConvertExpr(cond);
                }

                clauses.Add(clause);
            }
            else if (c.Type == "if_clause")
            {
                // filter привязан к последнему for
                if (clauses.Count == 0)
                    throw Err("if без предшествующего for", c);

                var cond = c.NamedChildren().FirstOrDefault();
                if (cond.IsValid && clauses[^1].Filter is null)
                    clauses[^1].Filter = ConvertExpr(cond);
            }
        }

        return clauses;
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

        var fieldName = Text(nameNode);

        // math.pi / math.e / math.tau — константы
        if (target.Type == "identifier")
        {
            var tname = Text(target);
            if (tname == "math")
            {
                switch (fieldName)
                {
                    case "pi":
                        return new LiteralExpr { Value = ToJsonElement(Math.PI) };
                    case "e":
                        return new LiteralExpr { Value = ToJsonElement(Math.E) };
                    case "tau":
                        return new LiteralExpr { Value = ToJsonElement(Math.Tau) };
                }
            }
        }

        return new FieldExpr
        {
            Target = ConvertExpr(target),
            FieldName = fieldName
        };
    }
    private YawaExpression ConvertBinary(TsNode node)
    {
        var children = node.NamedChildren().ToList();
        if (children.Count < 2)
            throw Err("Некорректная бинарная операция", node);

        var a = children[0];
        var b = children[children.Count - 1];

        // Оператор — текст МЕЖДУ первым и последним named-child.
        // Это надёжнее, чем IndexOf по всему выражению (который может
        // поймать унарный минус или минус во вложенном выражении).
        var opStart = (int)a.EndByte;
        var opEnd = (int)b.StartByte;
        var op = System.Text.Encoding.UTF8.GetString(_bytes, opStart, opEnd - opStart).Trim();

        if (string.IsNullOrEmpty(op))
            throw Err($"Не удалось определить оператор в '{Text(node)}'", node);

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

        // Одиночное сравнение — как раньше.
        if (children.Count == 2)
        {
            var a = children[0];
            var b = children[1];

            var opStart = (int)a.EndByte;
            var opEnd = (int)b.StartByte;
            var rawOp = System.Text.Encoding.UTF8.GetString(_bytes, opStart, opEnd - opStart).Trim();

            return new BinaryExpr
            {
                Op = MapComparisonOp(rawOp, node),
                A = ConvertExpr(a),
                B = ConvertExpr(b)
            };
        }

        // Chained: a < b < c → (a < b) and (b < c).
        // Tree-sitter даёт [a, b, c, ...] и операторы между ними.
        var result = new List<YawaExpression>();
        for (int i = 0; i < children.Count - 1; i++)
        {
            var a = children[i];
            var b = children[i + 1];

            var opStart = (int)a.EndByte;
            var opEnd = (int)b.StartByte;
            var rawOp = System.Text.Encoding.UTF8.GetString(_bytes, opStart, opEnd - opStart).Trim();

            result.Add(new BinaryExpr
            {
                Op = MapComparisonOp(rawOp, node),
                A = ConvertExpr(a),
                B = ConvertExpr(b)
            });
        }

        // Складываем все пары через AND.
        var combined = result[0];
        for (int i = 1; i < result.Count; i++)
            combined = new BinaryExpr { Op = "and", A = combined, B = result[i] };
        return combined;
    }

    private string MapComparisonOp(string rawOp, TsNode node)
    {
        return rawOp switch
        {
            "==" => "==",
            "!=" => "!=",
            "<" => "<",
            "<=" => "<=",
            ">" => ">",
            ">=" => ">=",
            "in" => "in",
            "not in" => "not_in",
            "is" => "==",
            "is not" => "!=",
            _ => throw Err($"Неизвестный оператор сравнения: '{rawOp}'", node)
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

        // Проверяем, есть ли keyword-аргументы
        if (argsList.IsValid &&
            argsList.NamedChildren().Any(a => a.Type == "keyword_argument"))
        {
            return ConvertCallWithKeywords(node, callee, argsList);
        }

        // В tree-sitter-python call может иметь:
        //   - argument_list (обычный случай)
        //   - generator_expression напрямую (sum(x*x for x in A) — без скобок)
        List<TsNode> argNodes;
        if (argsList.IsValid)
            argNodes = argsList.NamedChildren()
                .Where(a => a.Type != "comment")
                .ToList();
        else
            argNodes = children.Skip(1)
                .Where(a => a.Type != "comment")
                .ToList();

        var args = argNodes.Select(ConvertExpr).ToList();

        // Встроенные
        if (callee.Type == "identifier")
        {
            var name = Text(callee);

            name = LookupRename(name);

            // Вызов класса → instantiate
            if (_classNames.Contains(name))
            {
                return new InstantiateExpr { ClassName = name, Args = args };
            }

            var mapped = name switch
            {
                "len" => "length",
                "range" => "range",
                "min" => "min",
                "max" => "max",
                "abs" => "abs",
                "print" => "print",
                "set" => "set",
                "list" => "list",
                "tuple" => "tuple",
                "sum" => "sum",
                "any" => "any",
                "all" => "all",
                "chr" => "chr",
                "ord" => "ord",
                "int" => "int",
                "str" => "str",
                "float" => "float",
                "bool" => "bool",
                "enumerate" => "enumerate",
                "zip" => "zip",
                "sorted" => "sorted",
                "map" => "map",
                "filter" => "filter",
                _ => name
            };
            return new CallExpr { Name = mapped, Args = args };
        }

        // Метод
        if (callee.Type == "attribute")
        {
            var attrChildren = callee.NamedChildren().ToList();
            if (attrChildren.Count < 2)
                throw Err("Некорректный доступ к методу", callee);

            var receiver = attrChildren[0];
            var methodName = Text(attrChildren[1]);

            // MathHelper.double(5) — вызов статического метода через имя класса.
            if (receiver.Type == "identifier" && _classNames.Contains(Text(receiver)))
            {
                return new CallExpr
                {
                    Name = $"{Text(receiver)}.{methodName}",
                    Args = args
                };
            }

            return new CallMethodExpr
            {
                Receiver = ConvertExpr(receiver),
                Name = methodName,
                Args = args
            };
        }

        throw Err("Неподдерживаемый вызываемый объект", callee);
    }

    private YawaExpression ConvertCallWithKeywords(TsNode node, TsNode callee, TsNode argsList)
    {
        if (callee.Type != "identifier")
            throw Err("keyword-аргументы поддерживаются только для простых вызовов", node);

        var funcName = Text(callee);
        var args = new List<YawaExpression>();
        var keywords = new Dictionary<string, TsNode>();

        foreach (var a in argsList.NamedChildren())
        {
            if (a.Type == "comment") continue;
            if (a.Type == "keyword_argument")
            {
                var kw = a.NamedChildren().ToList();
                if (kw.Count < 2)
                    throw Err("Некорректный keyword_argument", a);
                keywords[Text(kw[0])] = kw[1];
            }
            else
            {
                args.Add(ConvertExpr(a));
            }
        }

        // sorted(A, key=abs) / sorted(A, key=len)
        if (funcName == "sorted")
        {
            if (args.Count != 1)
                throw Err("sorted() должен иметь один позиционный аргумент", node);

            if (!keywords.TryGetValue("key", out var keyNode))
                return new CallExpr { Name = "sorted", Args = args };

            if (keyNode.Type != "identifier")
                throw Err("sorted(key=...) принимает только имя функции", keyNode);

            var keyName = Text(keyNode);

            // Специальные встроенные — быстрые пути
            if (keyName == "abs")
                return new CallExpr { Name = "sorted_by_abs", Args = args };
            if (keyName == "len")
                return new CallExpr { Name = "sorted_by_len", Args = args };

            // Произвольная функция — sorted_with(array, funcref)
            var funcRef = new FunctionRefExpr { Name = keyName };
            args.Add(funcRef);
            return new CallExpr { Name = "sorted_with", Args = args };
        }

        // Общий случай: упаковываем keyword-аргументы в объект
        // и добавляем как последний позиционный аргумент.
        // Функция с **kwargs получит его; без **kwargs — ошибка в runtime.
        var kwargsObj = new DictExpr();
        foreach (var (k, v) in keywords)
        {
            kwargsObj.Items.Add(new DictEntry
            {
                Key = new LiteralExpr { Value = ToJsonElement(k) },
                Value = ConvertExpr(v)
            });
        }
        args.Add(kwargsObj);

        var finalName = funcName;
        finalName = LookupRename(finalName);

        if (_classNames.Contains(finalName))
            return new InstantiateExpr { ClassName = finalName, Args = args };

        return new CallExpr { Name = finalName, Args = args };
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
        // В позиции target-а identifier — это всегда переменная, не функция.
        if (node.Type == "identifier")
            return new RefExpr { Name = Text(node) };

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
    private YawaExpression ConvertSet(TsNode node)
    {
        var result = new SetLiteralExpr();
        foreach (var child in node.NamedChildren())
        {
            if (child.Type == "comment") continue;
            result.Items.Add(ConvertExpr(child));
        }
        return result;
    }

    // ─────────── Helpers ───────────

    private string LookupRename(string name)
    {
        // Stack перечисляет сверху вниз — сначала проверяем самые вложенные.
        foreach (var dict in _renameStack)
            if (dict.TryGetValue(name, out var renamed))
                return renamed;
        return name;
    }

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