using System.Diagnostics;
using AlgoVis.Yawa.Trace;
using AlgoVis.Yawa.Yawa;
using AlgoVis.Yawa.Yawa.Expressions;
using AlgoVis.Yawa.Yawa.Statements;

namespace AlgoVis.Yawa.Runtime;

public sealed class Interpreter
{
    private readonly YawaProgram _program;
    private readonly Dictionary<string, YawaFunction> _functions;
    private readonly InterpreterOptions _options;
    private readonly Stopwatch _clock = new();
    private int _stepBudget;

    public TraceRecorder Recorder { get; }
    public Evaluator Eval { get; }

    public Interpreter(YawaProgram program, InterpreterOptions? options = null)
    {
        _program = program;
        _options = options ?? new InterpreterOptions
        {
            MaxSteps = program.Limits.MaxSteps,
            MaxDepth = program.Limits.MaxDepth,
            MaxSeconds = program.Limits.MaxSeconds,
            SnapshotEvery = program.Limits.SnapshotEvery
        };
        _stepBudget = _options.MaxSteps;
        _functions = program.Functions.ToDictionary(f => f.Name);
        Recorder = new TraceRecorder(
            program.YawaVersion,
            new Dictionary<string, object?> { ["name"] = program.Metadata.Name },
            _options);
        Eval = new Evaluator(this);
    }

    public TraceSession Run()
    {
        _clock.Start();

        var globalFrame = new Frame("<global>");

        // 1. Глобальные переменные (если объявлены в YAWA)
        foreach (var (name, g) in _program.Globals)
        {
            var val = g.Value is null ? NullValue.Instance : Eval.Eval(g.Value, globalFrame);
            globalFrame.Declare(name, val);
        }

        // 2. Входные аргументы — часть НАЧАЛЬНОГО состояния.
        // Вычисляем их сразу и объявляем в globalFrame, чтобы они попали
        // в init-шаг и первый snapshot.
        var entryFn = _functions[_program.Entry.Function];
        var entryArgs = _program.Entry.Args
            .Select(e => Eval.Eval(e, globalFrame))
            .ToList();

        if (entryArgs.Count != entryFn.Params.Count)
            throw new YawaRuntimeException(
                $"Entry '{entryFn.Name}': expected {entryFn.Params.Count} args, got {entryArgs.Count}");

        for (int i = 0; i < entryArgs.Count; i++)
        {
            var argName = entryFn.Params[i].Name;
            globalFrame.Declare(argName, entryArgs[i]);
        }

        // 3. Пишем init и стартовый snapshot — теперь со всеми входными данными
        Recorder.SetFrame(globalFrame);
        Recorder.Record("init", null);
        Recorder.RecordSnapshot(globalFrame, "start");

        try
        {
            var result = CallFunction(_program.Entry.Function, entryArgs, globalFrame, null,
                                      syncLocalsBackToCaller: true);

            if (result is not NullValue)
                globalFrame.Declare("__return__", result);

            Recorder.Record("end", null);
            Recorder.Finalize(globalFrame, result);
        }
        catch (ReturnException rex)
        {
            if (rex.Value is not NullValue)
                globalFrame.Declare("__return__", rex.Value);
            Recorder.Record("end", null);
            Recorder.Finalize(globalFrame, rex.Value);
        }
        catch (YawaRuntimeException ex)
        {
            Recorder.SetAnnotation($"ERROR: {ex.Message}");
            Recorder.Record("error", null);
            Recorder.Finalize(globalFrame, null);
        }

        TraceRecorder.SetStructureDescriptor(Recorder.Session, globalFrame);
        return Recorder.Session;
    }

    public RuntimeValue CallFunction(string name, List<YawaExpression> argExprs, Frame callerFrame, string? nodeId)
    {
        var args = argExprs.Select(e => Eval.Eval(e, callerFrame)).ToList();
        return CallFunction(name, args, callerFrame, nodeId);
    }

    public RuntimeValue CallFunction(string name, List<RuntimeValue> args, Frame callerFrame, string? nodeId, bool syncLocalsBackToCaller = false)
    {
        CheckLimits();

        // 1. Локальная функция-значение (в т.ч. с замыканием)
        if (callerFrame.Locals.TryGetValue(name, out var localVal) &&
            localVal is FunctionRefValue localFn)
        {
            return CallUserFunction(localFn.Name, args, callerFrame, localFn.CapturedFrame,
                                    nodeId, syncLocalsBackToCaller);
        }

        // 1.5. Специальные builtin-ы, требующие доступа к CallFunction/CallFunctionValue.
        if (name is "filter" or "map")
            return CallHigherOrderBuiltin(name, args, callerFrame);

        if (name == "sorted_with")
            return CallSortedWith(args, callerFrame);

        // 2. Builtin (только если нет пользовательской)
        if (!_functions.ContainsKey(name) && Builtins.Names.Contains(name))
        {
            var result = Builtins.Call(name, args, Recorder);
            if (name == "print")
                Recorder.Record("call", nodeId);
            return result;
        }

        // 3. Пользовательская функция
        if (!_functions.ContainsKey(name))
            throw new YawaRuntimeException($"Unknown function: {name}");

        return CallUserFunction(name, args, callerFrame, null, nodeId, syncLocalsBackToCaller);
    }

    /// <summary>
    /// Вызов FunctionRefValue с сохранением capturedFrame.
    /// Используется в higher-order builtins (map/filter/sorted).
    /// </summary>
    public RuntimeValue CallFunctionValue(FunctionRefValue fn, List<RuntimeValue> args, Frame callerFrame, string? nodeId)
    {
        CheckLimits();
        return CallUserFunction(fn.Name, args, callerFrame, fn.CapturedFrame, nodeId, false);
    }

    private RuntimeValue CallUserFunction(string name, List<RuntimeValue> args, Frame callerFrame, Frame? capturedFrame, string? nodeId, bool syncLocalsBackToCaller)
    {
        if (!_functions.TryGetValue(name, out var fn))
            throw new YawaRuntimeException($"Unknown function: {name}");

        // Родитель нового frame-а: захваченный (замыкание) или вызывающий.
        var parent = capturedFrame ?? callerFrame;

        var frame = new Frame(name, parent);
        BindArguments(fn, args, frame);

        Recorder.SetFrame(frame);
        var argRepr = args.Count == 0
            ? ""
            : "(" + string.Join(", ", args.Select(ShortRepr)) + ")";
        Recorder.SetAnnotation($"→ {name}{argRepr}");
        Recorder.Record("call", nodeId, consumeHighlights: true);

        if (_options.SnapshotEvery > 0 && Recorder.Session.Steps.Count % _options.SnapshotEvery == 0)
            Recorder.RecordSnapshot(frame, $"auto snapshot every {_options.SnapshotEvery}");

        try
        {
            ExecuteBlock(fn.Body, frame);
            if (syncLocalsBackToCaller) SyncLocals(fn, frame, callerFrame);
            return NullValue.Instance;
        }
        catch (ReturnException rex)
        {
            if (syncLocalsBackToCaller) SyncLocals(fn, frame, callerFrame);
            return rex.Value;
        }
        finally
        {
            Recorder.SetFrame(callerFrame);
            Recorder.RecordSnapshot(frame, $"exit {name}");
        }
    }
    private RuntimeValue CallHigherOrderBuiltin(string name, List<RuntimeValue> args, Frame callerFrame)
    {
        if (args.Count != 2)
            throw new YawaRuntimeException($"{name}() needs 2 args");
        if (args[0] is not FunctionRefValue fn)
            throw new YawaRuntimeException($"{name}() needs function as first arg");

        var items = args[1] switch
        {
            ArrayValue a => a.Items,
            SetValue s => s.Items,
            TupleValue t => t.Items,
            _ => throw new YawaRuntimeException($"{name}() needs iterable, got {args[1].TypeName}")
        };

        var result = new List<RuntimeValue>();
        foreach (var item in items)
        {
            var r = CallFunctionValue(fn, new List<RuntimeValue> { item }, callerFrame, null);
            if (name == "filter")
            {
                if (Evaluator.AsBool(r)) result.Add(item);
            }
            else
            {
                result.Add(r);
            }
        }
        return new ArrayValue(result);
    }

    private RuntimeValue CallSortedWith(List<RuntimeValue> args, Frame callerFrame)
    {
        if (args.Count != 2)
            throw new YawaRuntimeException("sorted_with() needs 2 args");
        if (args[0] is not ArrayValue arr)
            throw new YawaRuntimeException("sorted_with() needs array");
        if (args[1] is not FunctionRefValue fn)
            throw new YawaRuntimeException("sorted_with() needs function as second arg");

        var items = arr.Items.ToList();

        // Insertion sort: стабильная, вызывает key-функцию для каждого элемента.
        for (int i = 1; i < items.Count; i++)
        {
            var current = items[i];
            var currentKey = CallFunctionValue(fn, new List<RuntimeValue> { current }, callerFrame, null);

            int j = i - 1;
            while (j >= 0)
            {
                var otherKey = CallFunctionValue(fn, new List<RuntimeValue> { items[j] }, callerFrame, null);
                if (CompareRuntime(otherKey, currentKey) <= 0) break;
                items[j + 1] = items[j];
                j--;
            }
            items[j + 1] = current;
        }

        return new ArrayValue(items, arr.ElementType);
    }

    private static int CompareRuntime(RuntimeValue a, RuntimeValue b)
    {
        if (a is IntValue ai && b is IntValue bi) return ai.Value.CompareTo(bi.Value);
        if (a is FloatValue af && b is FloatValue bf) return af.Value.CompareTo(bf.Value);
        if (a is IntValue ai2 && b is FloatValue bf2) return ((double)ai2.Value).CompareTo(bf2.Value);
        if (a is FloatValue af2 && b is IntValue bi2) return af2.Value.CompareTo((double)bi2.Value);
        if (a is StringValue asv && b is StringValue bsv) return string.CompareOrdinal(asv.Value, bsv.Value);
        throw new YawaRuntimeException($"Cannot compare {a.TypeName} and {b.TypeName}");
    }

    /// <summary>
    /// Привязывает аргументы к параметрам функции с учётом *args / **kwargs.
    /// </summary>
    private static void BindArguments(YawaFunction fn, List<RuntimeValue> args, Frame frame)
    {
        var hasVariadic = fn.Params.Any(p => p.IsVariadic || p.IsKeywordVariadic);
        var hasKwVariadic = fn.Params.Any(p => p.IsKeywordVariadic);

        if (!hasVariadic)
        {
            if (args.Count != fn.Params.Count)
                throw new YawaRuntimeException(
                    $"{fn.Name}: expected {fn.Params.Count} args, got {args.Count}");
            for (int i = 0; i < fn.Params.Count; i++)
                frame.Declare(fn.Params[i].Name, args[i]);
            return;
        }

        // С variadic: последние параметры — variadic
        var normalCount = fn.Params.Count(p => !p.IsVariadic && !p.IsKeywordVariadic);

        // Отделяем kwargs от позиционных (kwargs — единственный объект в конце)
        RuntimeValue? kwargsObj = null;
        var positional = args;
        if (hasKwVariadic && args.Count > 0 && args[^1] is ObjectValue obj)
        {
            // Не можем однозначно отличить обычный объект от kwargs.
            // Считаем: если есть **kwargs-параметр и последний аргумент — ObjectValue,
            // и до этого не хватает аргументов, то это kwargs.
            var expectAtLeast = fn.Params.Count(p => !p.IsKeywordVariadic);
            if (args.Count >= expectAtLeast)
            {
                kwargsObj = obj;
                positional = args.GetRange(0, args.Count - 1);
            }
        }

        if (positional.Count < normalCount)
            throw new YawaRuntimeException(
                $"{fn.Name}: expected at least {normalCount} args, got {positional.Count}");

        // Привязываем обычные
        int pi = 0;
        for (; pi < fn.Params.Count; pi++)
        {
            var p = fn.Params[pi];
            if (p.IsVariadic || p.IsKeywordVariadic) break;
            frame.Declare(p.Name, positional[pi]);
        }

        var consumed = pi; // сколько позиционных ушло на обычные

        // Если следующий — *args
        if (pi < fn.Params.Count && fn.Params[pi].IsVariadic)
        {
            var rest = positional.Skip(consumed).ToList();
            frame.Declare(fn.Params[pi].Name, new ArrayValue(rest));
            pi++;
        }

        // Если следующий — **kwargs
        if (pi < fn.Params.Count && fn.Params[pi].IsKeywordVariadic)
        {
            frame.Declare(fn.Params[pi].Name,
                kwargsObj ?? new ObjectValue());
            pi++;
        }
    }

    /// <summary>
    /// Создаёт объект указанного класса. Если есть ClassName__init — вызывает.
    /// Возвращаемое значение __init игнорируется (как в Python).
    /// </summary>
    public RuntimeValue Instantiate(string className, List<YawaExpression> argExprs, Frame frame, string? nodeId)
    {
        var obj = new ObjectValue();
        obj.SetField("__type__", new StringValue(className));

        var initName = $"{className}.__init__";
        if (!_functions.ContainsKey(initName))
        {
            // C++: конструктор называется по имени класса
            initName = $"{className}.{className}";
        }
        if (_functions.ContainsKey(initName))
        {
            var args = new List<RuntimeValue> { obj };
            foreach (var expr in argExprs)
                args.Add(Eval.Eval(expr, frame));

            CallFunction(initName, args, frame, nodeId);   // возврат игнорируем
        }
        else if (argExprs.Count > 0)
        {
            throw new YawaRuntimeException(
                $"Class '{className}' has no __init__, but {argExprs.Count} args provided");
        }

        return obj;
    }
    /// <summary>
    /// Вызов метода obj.name(args).
    /// Приоритет:
    ///   1. Если obj имеет __type__ = ClassName и есть функция ClassName__name — вызываем её.
    ///   2. Иначе, если name — builtin-метод (append/pop/insert/remove), мапим и вызываем builtin.
    ///   3. Иначе, вызываем глобальную функцию name с receiver первым аргументом.
    /// </summary>
    public RuntimeValue CallMethod(RuntimeValue receiver, string name, List<YawaExpression> argExprs, Frame frame, string? nodeId)
    {
        // 1. Метод класса
        if (receiver is ObjectValue obj && obj.ClassName is { } cls)
        {
            var qualified = $"{cls}.{name}";
            if (_functions.ContainsKey(qualified))
            {
                var args = new List<RuntimeValue> { receiver };
                foreach (var expr in argExprs)
                    args.Add(Eval.Eval(expr, frame));
                return CallFunction(qualified, args, frame, nodeId);
            }
        }

        // C++-специфичные методы
        if (receiver is SetValue)
        {
            switch (name)
            {
                case "insert":
                case "add":
                    {
                        if (argExprs.Count != 1)
                            throw new YawaRuntimeException($"{name}() needs 1 arg");
                        var val = Eval.Eval(argExprs[0], frame);
                        ((SetValue)receiver).Add(val);
                        return NullValue.Instance;
                    }
                case "count":
                case "contains":
                    {
                        if (argExprs.Count != 1)
                            throw new YawaRuntimeException($"{name}() needs 1 arg");
                        var val = Eval.Eval(argExprs[0], frame);
                        var has = ((SetValue)receiver).Contains(val);
                        // C++ ожидает число, Python — bool. Возвращаем IntValue (0/1),
                        // для if это тоже сработает через AsBool.
                        return new IntValue(has ? 1 : 0);
                    }
                case "erase":
                case "remove":
                case "discard":
                    {
                        if (argExprs.Count != 1)
                            throw new YawaRuntimeException($"{name}() needs 1 arg");
                        var val = Eval.Eval(argExprs[0], frame);
                        ((SetValue)receiver).Remove(val);
                        return NullValue.Instance;
                    }
                case "size":
                case "length":
                    return new IntValue(((SetValue)receiver).Count);
            }
        }

        if (receiver is ObjectValue obj2)
        {
            // m.count(key) → 1/0
            if (name == "count" && argExprs.Count == 1)
            {
                var key = Eval.Eval(argExprs[0], frame);
                var keyStr = key is StringValue sv1 ? sv1.Value : key.ToString() ?? "";
                return obj2.HasField(keyStr) ? new IntValue(1) : new IntValue(0);
            }
            // m.erase(key)
            if ((name == "erase" || name == "remove") && argExprs.Count == 1)
            {
                var key = Eval.Eval(argExprs[0], frame);
                var keyStr = key is StringValue sv1 ? sv1.Value : key.ToString() ?? "";
                obj2.Fields.Remove(keyStr);
                return new IntValue(obj2.HasField(keyStr) ? 1 : 0);
            }
            if (name == "find" && argExprs.Count == 1)
            {
                var key = Eval.Eval(argExprs[0], frame);
                var keyStr = key is StringValue sv1 ? sv1.Value : key.ToString() ?? "";
                return obj2.HasField(keyStr) ? new IntValue(1) : new IntValue(0);
            }
            if (name == "find" && argExprs.Count == 1)
            {
                var key = Eval.Eval(argExprs[0], frame);
                var keyStr = key is StringValue sv1 ? sv1.Value : key.ToString() ?? "";
                return obj2.HasField(keyStr) ? new IntValue(1) : new IntValue(0);
            }

            if (name == "end")
            {
                // C++ итератор "end" — у нас маркер "ничего не найдено" = 0
                return new IntValue(0);
            }
        }

        if (receiver is StringValue sv)
        {
            return CallStringMethod(sv, name, argExprs, frame);
        }

        // Специальные методы коллекций — мапим в builtin.
        var builtinName = name switch
        {
            "append" => "push",
            "push_back" => "push",     // C++ vector
            "pop" => "pop",
            "pop_back" => "pop",      // C++ vector
            "insert" => "insert",
            "remove" => "remove",
            "add" => "add",
            "discard" => "discard",
            "size" => "length",   // C++ string, vector
            "length" => "length",
            _ => null
        };

        // Метод remove у set тоже есть — но у нас remove только для array.
        // Если receiver — set, а метод remove — вызываем discard.
        if (receiver is SetValue && name == "remove")
            builtinName = "discard";

        if (builtinName is not null && Builtins.Names.Contains(builtinName))
        {
            var args2 = new List<RuntimeValue> { receiver };
            foreach (var expr in argExprs)
                args2.Add(Eval.Eval(expr, frame));
            return Builtins.Call(builtinName, args2, Recorder);
        }

        // 3. Fallback — глобальная функция с receiver первым аргументом.
        var defaultArgs = new List<RuntimeValue> { receiver };
        foreach (var expr in argExprs)
            defaultArgs.Add(Eval.Eval(expr, frame));
        return CallFunction(name, defaultArgs, frame, nodeId);
    }

    private RuntimeValue CallStringMethod(
    StringValue s, string name, List<YawaExpression> argExprs, Frame frame)
    {
        var args = argExprs.Select(e => Eval.Eval(e, frame)).ToList();
        string str = s.Value;

        switch (name)
        {
            case "strip": return new StringValue(str.Trim());
            case "lstrip": return new StringValue(str.TrimStart());
            case "rstrip": return new StringValue(str.TrimEnd());
            case "lower": return new StringValue(str.ToLowerInvariant());
            case "upper": return new StringValue(str.ToUpperInvariant());
            case "capitalize":
                return new StringValue(str.Length == 0
                    ? str
                    : char.ToUpperInvariant(str[0]) + str[1..].ToLowerInvariant());
            case "title":
                return new StringValue(System.Globalization.CultureInfo.InvariantCulture
                    .TextInfo.ToTitleCase(str.ToLowerInvariant()));

            case "split":
                {
                    var sep = args.Count > 0 && args[0] is StringValue sepS
                        ? sepS.Value
                        : null;
                    var parts = sep is null
                        ? str.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                        : str.Split(sep);
                    return new ArrayValue(
                        parts.Select(p => (RuntimeValue)new StringValue(p)));
                }

            case "join":
                {
                    if (args.Count == 0 || args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("join() needs array");
                    return new StringValue(string.Join(str,
                        arr.Items.Select(v => v is StringValue s2 ? s2.Value : v.ToString())));
                }

            case "replace":
                {
                    if (args.Count < 2)
                        throw new YawaRuntimeException("replace() needs 2 args");
                    var oldV = args[0] is StringValue o ? o.Value : args[0].ToString() ?? "";
                    var newV = args[1] is StringValue n ? n.Value : args[1].ToString() ?? "";
                    return new StringValue(str.Replace(oldV, newV));
                }

            case "startswith":
                {
                    var prefix = args[0] is StringValue p ? p.Value : args[0].ToString() ?? "";
                    return str.StartsWith(prefix) ? BoolValue.True : BoolValue.False;
                }

            case "endswith":
                {
                    var suffix = args[0] is StringValue p ? p.Value : args[0].ToString() ?? "";
                    return str.EndsWith(suffix) ? BoolValue.True : BoolValue.False;
                }

            case "find":
                {
                    var sub = args[0] is StringValue p ? p.Value : args[0].ToString() ?? "";
                    return new IntValue(str.IndexOf(sub, StringComparison.Ordinal));
                }

            case "count":
                {
                    var sub = args[0] is StringValue p ? p.Value : args[0].ToString() ?? "";
                    if (sub.Length == 0) return new IntValue(0);
                    int cnt = 0, idx = 0;
                    while ((idx = str.IndexOf(sub, idx, StringComparison.Ordinal)) >= 0)
                    { cnt++; idx += sub.Length; }
                    return new IntValue(cnt);
                }

            case "isdigit":
                return str.Length > 0 && str.All(char.IsDigit)
                    ? BoolValue.True : BoolValue.False;
            case "isalpha":
                return str.Length > 0 && str.All(char.IsLetter)
                    ? BoolValue.True : BoolValue.False;
            case "isspace":
                return str.Length > 0 && str.All(char.IsWhiteSpace)
                    ? BoolValue.True : BoolValue.False;
            case "isupper":
                return str.Length > 0 && str.Any(char.IsLetter) && str.All(c => !char.IsLetter(c) || char.IsUpper(c))
                    ? BoolValue.True : BoolValue.False;
            case "islower":
                return str.Length > 0 && str.Any(char.IsLetter) && str.All(c => !char.IsLetter(c) || char.IsLower(c))
                    ? BoolValue.True : BoolValue.False;
            case "size":
                return new IntValue(str.Length);
            case "length":
                return new IntValue(str.Length);

            default:
                throw new YawaRuntimeException($"Unknown string method: {name}");
        }
    }

    private static void SyncLocals(YawaFunction fn, Frame funcFrame, Frame callerFrame)
    {
        foreach (var (name, value) in funcFrame.Locals)
        {
            if (name.StartsWith("__")) continue;
            callerFrame.Set(name, value);
        }
    }

    private void ExecuteBlock(List<YawaStatement> body, Frame frame)
    {
        foreach (var stmt in body)
            ExecuteStatement(stmt, frame);
    }

    private void ExecuteStatement(YawaStatement stmt, Frame frame)
    {
        CheckLimits();
        Recorder.SetFrame(frame);

        switch (stmt)
        {
            case DeclareStatement d: ExecuteDeclare(d, frame); break;
            case AssignStatement a: ExecuteAssign(a, frame); break;
            case AssertStatement asrt: ExecuteAssert(asrt, frame); break;
            case IfStatement i: ExecuteIf(i, frame); break;
            case WhileStatement w: ExecuteWhile(w, frame); break;
            case ForStatement f: ExecuteFor(f, frame); break;
            case ForeachStatement fe: ExecuteForeach(fe, frame); break;
            case ReturnStatement r:
                throw new ReturnException(
                                            r.Value is null ? NullValue.Instance : Eval.Eval(r.Value, frame));
            case BreakStatement: throw new BreakException();
            case ContinueStatement: throw new ContinueException();
            case ExprStatement e: Eval.Eval(e.Value, frame); break;
            case SwapStatement s: ExecuteSwap(s, frame); break;
            case CompareStatement c: ExecuteCompare(c, frame); break;
            case MarkStatement m: ExecuteMark(m, frame); break;
            case UnmarkStatement u: ExecuteUnmark(u, frame); break;
            case AnnotateStatement a: Recorder.SetAnnotation(a.Text); Recorder.Record("annotate", a.NodeId); break;
            case CountStatement c: Recorder.Stats.Count(c.Name, c.Delta); Recorder.Record("count", c.NodeId); break;
            case SnapshotStatement s: Recorder.RecordSnapshot(frame, s.Label); break;
            case MakeNodeStatement mn: ExecuteMakeNode(mn, frame); break;
            case TupleAssignStatement ta: ExecuteTupleAssign(ta, frame); break;
            case TryStatement t: ExecuteTry(t, frame); break;
            case DeleteStatement d: ExecuteDelete(d, frame); break;
            case SwapRefStatement sw: ExecuteSwapRef(sw, frame); break;
            case ForeachPairStatement fp: ExecuteForeachPair(fp, frame); break;
            default:
                throw new YawaRuntimeException($"Unknown statement: {stmt.GetType().Name}");
        }
    }
    private void ExecuteForeachPair(ForeachPairStatement s, Frame frame)
    {
        var collection = Eval.Eval(s.In, frame);
        var items = new List<(RuntimeValue k, RuntimeValue v)>();

        switch (collection)
        {
            case ObjectValue obj:
                foreach (var kv in obj.Fields)
                    if (!kv.Key.StartsWith("__"))
                        items.Add((new StringValue(kv.Key), kv.Value));
                break;
            case ArrayValue arr:
                for (int i = 0; i < arr.Count; i++)
                    items.Add((new IntValue(i), arr[i]));
                break;
            case SetValue st:
                foreach (var v in st.Items)
                    items.Add((v, v));
                break;
            default:
                throw new YawaRuntimeException($"foreach_pair on {collection.TypeName}");
        }

        foreach (var (k, v) in items)
        {
            frame.Declare(s.KeyVar, k);
            frame.Declare(s.ValueVar, v);
            try { ExecuteBlock(s.Body, frame); }
            catch (BreakException) { break; }
            catch (ContinueException) { continue; }
        }
    }

    private void ExecuteSwapRef(SwapRefStatement s, Frame frame)
    {
        // Вычисляем оба значения
        var aVal = Eval.Eval(s.A, frame);
        var bVal = Eval.Eval(s.B, frame);

        // Присваиваем через AssignTo (учитывает index/field/ref)
        AssignTo(s.A, bVal, frame, s.NodeId);
        AssignTo(s.B, aVal, frame, s.NodeId);

        Recorder.Stats.Swaps++;
    }

    private void ExecuteDeclare(DeclareStatement d, Frame frame)
    {
        var value = d.Value is null ? NullValue.Instance : Eval.Eval(d.Value, frame);
        frame.Declare(d.Name, value);
        Recorder.Record("declare", d.NodeId, new[]
        {
            new Trace.StateChange { Target = d.Name, Old = null, New = value.ToJson() }
        });
    }

    private void ExecuteTry(TryStatement s, Frame frame)
    {
        try
        {
            ExecuteBlock(s.Body, frame);
        }
        catch (YawaRuntimeException ex)
        {
            var handled = false;
            foreach (var h in s.Handlers)
            {
                var hf = new Frame("<except>", frame);
                if (!string.IsNullOrEmpty(h.VarName))
                    hf.Declare(h.VarName!, new StringValue(ex.Message));

                ExecuteBlock(h.Body, hf);
                handled = true;
                break;
            }
            if (!handled) throw;
        }
        finally
        {
            if (s.Finally is not null)
                ExecuteBlock(s.Finally, frame);
        }
    }

    private void ExecuteAssert(AssertStatement s, Frame frame)
    {
        var cond = Evaluator.AsBool(Eval.Eval(s.Cond, frame));
        if (cond) return;

        string msg = "Assertion failed";
        if (s.Message is not null)
        {
            var v = Eval.Eval(s.Message, frame);
            msg = v is StringValue sv ? sv.Value : v.ToString() ?? msg;
        }
        throw new YawaRuntimeException(msg);
    }

    private void ExecuteDelete(DeleteStatement s, Frame frame)
    {
        foreach (var target in s.Targets)
        {
            switch (target)
            {
                case RefExpr r:
                    frame.Locals.Remove(r.Name);
                    Recorder.Record("delete", s.NodeId, new[]
                    {
                    new Trace.StateChange { Target = r.Name, Old = null, New = null }
                });
                    break;

                case IndexExpr ix:
                    {
                        var container = Eval.Eval(ix.Target, frame);
                        var idx = Eval.Eval(ix.Index, frame);

                        if (container is ObjectValue obj)
                        {
                            var key = idx is StringValue sv ? sv.Value : idx.ToString() ?? "";
                            if (obj.Fields.ContainsKey(key))
                            {
                                obj.Fields.Remove(key);
                                var pname = RenderTargetPath(ix.Target, frame);
                                Recorder.Record("delete", s.NodeId, new[]
                                {
                                new Trace.StateChange { Target = $"{pname}[\"{key}\"]", Old = null, New = null }
                            });
                            }
                        }
                        else if (container is ArrayValue arr)
                        {
                            var i = (int)Evaluator.AsInt(idx, "delete index");
                            if (i < 0) i += arr.Count;
                            if (i >= 0 && i < arr.Count)
                            {
                                arr.Items.RemoveAt(i);
                                var pname = RenderTargetPath(ix.Target, frame);
                                Recorder.Record("delete", s.NodeId, new[]
                                {
                                new Trace.StateChange { Target = $"{pname}[{i}]", Old = null, New = null }
                            });
                            }
                        }
                        break;
                    }

                case FieldExpr fl:
                    {
                        var container = Eval.Eval(fl.Target, frame);
                        if (container is ObjectValue obj && obj.Fields.ContainsKey(fl.FieldName))
                        {
                            obj.Fields.Remove(fl.FieldName);
                            Recorder.Record("delete", s.NodeId);
                        }
                        break;
                    }
            }
        }
    }

    private void ExecuteTupleAssign(TupleAssignStatement s, Frame frame)
    {
        // Вычисляем все правые значения.
        var evaluated = s.Values.Select(v => Eval.Eval(v, frame)).ToList();

        // Случай 0: splat — head, *rest = [...]
        if (s.SplatIndex >= 0)
        {
            var leftCount = s.Targets.Count;
            var splatIdx = s.SplatIndex;

            // Правая часть должна быть одним массивом/кортежем
            ArrayValue? arr = evaluated.Count == 1 && evaluated[0] is ArrayValue av1
                ? av1
                : evaluated.Count == 1 && evaluated[0] is TupleValue tv1
                    ? new ArrayValue(tv1.Items)
                    : null;

            if (arr is null)
                throw new YawaRuntimeException(
                    "splat-распаковка требует одну последовательность справа");

            var fixedCount = leftCount - 1;
            if (arr.Count < fixedCount)
                throw new YawaRuntimeException(
                    $"splat: нужно минимум {fixedCount} элементов, получено {arr.Count}");

            for (int i = 0; i < splatIdx; i++)
                AssignTo(s.Targets[i], arr[i], frame, s.NodeId);

            var splatCount = arr.Count - fixedCount;
            var middle = new List<RuntimeValue>();
            for (int k = 0; k < splatCount; k++)
                middle.Add(arr[splatIdx + k]);
            AssignTo(s.Targets[splatIdx], new ArrayValue(middle), frame, s.NodeId);

            for (int i = splatIdx + 1; i < leftCount; i++)
            {
                var fromRight = arr.Count - (leftCount - i);
                AssignTo(s.Targets[i], arr[fromRight], frame, s.NodeId);
            }
            return;
        }

        // Случай 1: совпадающие списки
        if (evaluated.Count == s.Targets.Count)
        {
            for (int i = 0; i < s.Targets.Count; i++)
                AssignTo(s.Targets[i], evaluated[i], frame, s.NodeId);
            return;
        }

        // Случай 2: одна функция вернула tuple
        if (evaluated.Count == 1 && evaluated[0] is TupleValue tup)
        {
            if (tup.Count != s.Targets.Count)
                throw new YawaRuntimeException(
                    $"tuple unpack: expected {s.Targets.Count}, got {tup.Count}");
            for (int i = 0; i < s.Targets.Count; i++)
                AssignTo(s.Targets[i], tup[i], frame, s.NodeId);
            return;
        }

        // Случай 3: одна функция вернула array
        if (evaluated.Count == 1 && evaluated[0] is ArrayValue arr2)
        {
            if (arr2.Count != s.Targets.Count)
                throw new YawaRuntimeException(
                    $"array unpack: expected {s.Targets.Count}, got {arr2.Count}");
            for (int i = 0; i < s.Targets.Count; i++)
                AssignTo(s.Targets[i], arr2[i], frame, s.NodeId);
            return;
        }

        throw new YawaRuntimeException(
            $"tuple assign: {s.Targets.Count} targets, {evaluated.Count} values");
    }

    private void ExecuteAssign(AssignStatement a, Frame frame)
    {
        var value = Eval.Eval(a.Value, frame);
        AssignTo(a.Target, value, frame, a.NodeId);
    }

    private void AssignTo(YawaExpression target, RuntimeValue value, Frame frame, string? nodeId)
    {
        switch (target)
        {
            case RefExpr r:
                {
                    var old = frame.Has(r.Name) ? frame.Get(r.Name) : (RuntimeValue?)null;
                    frame.Set(r.Name, value);
                    Recorder.Record("assign", nodeId, new[]
                    {
                    new Trace.StateChange { Target = r.Name, Old = old?.ToJson(), New = value.ToJson() }
                });
                    break;
                }

            case IndexExpr ix:
                {
                    var targetVal = Eval.Eval(ix.Target, frame);
                    var idxVal = Eval.Eval(ix.Index, frame);

                    // Массив
                    if (targetVal is ArrayValue arr)
                    {
                        var idx = (int)Evaluator.AsInt(idxVal, "array index");
                        if (idx < 0) idx += arr.Count;
                        if (idx < 0 || idx >= arr.Count)
                            throw new YawaRuntimeException($"Array index out of range: {idx} (size {arr.Count})");
                        var old = arr[idx];
                        arr[idx] = value;
                        var targetName = RenderTargetPath(ix.Target, frame);
                        Recorder.Record("assign", nodeId, new[]
                        {
                        new Trace.StateChange { Target = $"{targetName}[{idx}]", Old = old.ToJson(), New = value.ToJson() }
                    });
                        break;
                    }

                    // Словарь / ObjectValue
                    if (targetVal is ObjectValue obj)
                    {
                        var key = idxVal is StringValue sv ? sv.Value : idxVal.ToString();
                        var old = obj.HasField(key) ? obj.GetField(key) : null;
                        obj.SetField(key, value);
                        var targetName = RenderTargetPath(ix.Target, frame);
                        Recorder.Record("assign", nodeId, new[]
                        {
                        new Trace.StateChange { Target = $"{targetName}[\"{key}\"]", Old = old?.ToJson(), New = value.ToJson() }
                    });
                        break;
                    }

                    throw new YawaRuntimeException(
                        $"assign to non-array/non-dict index (target={targetVal.TypeName})");
                }

            case FieldExpr fl:
                {
                    var targetVal = Eval.Eval(fl.Target, frame);
                    if (targetVal is ObjectValue obj)
                    {
                        var old = obj.HasField(fl.FieldName) ? obj.GetField(fl.FieldName) : null;
                        obj.SetField(fl.FieldName, value);
                        Recorder.Record("assign", nodeId, new[]
                        {
                        new Trace.StateChange { Target = $"object.{fl.FieldName}", Old = old?.ToJson(), New = value.ToJson() }
                    });
                    }
                    else if (targetVal is TreeNodeValue tn)
                    {
                        switch (fl.FieldName)
                        {
                            case "value": tn.Value = value; break;
                            case "left": tn.Left = value as TreeNodeValue; break;
                            case "right": tn.Right = value as TreeNodeValue; break;
                            default: throw new YawaRuntimeException($"TreeNode has no field '{fl.FieldName}'");
                        }
                        Recorder.Record("assign", nodeId);
                    }
                    else throw new YawaRuntimeException($"assign field on {targetVal.TypeName}");
                    break;
                }

            default:
                throw new YawaRuntimeException($"Invalid assignment target: {target.GetType().Name}");
        }
    }

    private string RenderTargetPath(YawaExpression expr, Frame frame)
    {
        switch (expr)
        {
            case RefExpr r:
                return r.Name;
            case IndexExpr ix:
                {
                    var inner = RenderTargetPath(ix.Target, frame);
                    var idxVal = Eval.Eval(ix.Index, frame);
                    if (idxVal is IntValue iv) return $"{inner}[{iv.Value}]";
                    if (idxVal is StringValue sv) return $"{inner}[\"{sv.Value}\"]";
                    return $"{inner}[?]";
                }
            case FieldExpr fl:
                return RenderTargetPath(fl.Target, frame) + "." + fl.FieldName;
            default:
                return "<expr>";
        }
    }

    private void ExecuteIf(IfStatement s, Frame frame)
    {
        RecordComparisonIfApplicable(s.Cond, frame);
        var cond = Evaluator.AsBool(Eval.Eval(s.Cond, frame));
        var branch = cond ? s.Then : (s.Else ?? new List<YawaStatement>());
        ExecuteBlock(branch, frame);
    }
    /// <summary>
    /// Рекурсивно обходит условие, регистрируя compare-шаги.
    /// Учитывает short-circuit: для `A and B` — если A ложно, B не регистрируется.
    /// Возвращает логический результат (для рекурсии).
    /// </summary>
    private bool RecordComparisonIfApplicable(YawaExpression cond, Frame frame)
    {
        switch (cond)
        {
            case BinaryExpr bin when bin.Op is "==" or "!=" or "<" or "<=" or ">" or ">=":
                {
                    RuntimeValue? a = null, b = null;
                    bool result = false;

                    try
                    {
                        a = Eval.Eval(bin.A, frame);
                        b = Eval.Eval(bin.B, frame);
                        result = EvaluateComparison(bin.Op, a, b);
                    }
                    catch { /* если не удалось — просто false */ }

                    Recorder.Stats.Comparisons++;

                    if (bin.A is IndexExpr ai)
                    {
                        var path = RenderIndexPathSafe(ai, frame);
                        if (path is not null) Recorder.AddHighlight(path);
                    }
                    if (bin.B is IndexExpr bi)
                    {
                        var path = RenderIndexPathSafe(bi, frame);
                        if (path is not null) Recorder.AddHighlight(path);
                    }

                    var diff = new List<Trace.StateChange>();
                    if (a is not null)
                        diff.Add(new Trace.StateChange { Target = "compare.a", Old = null, New = a.ToJson() });
                    if (b is not null)
                        diff.Add(new Trace.StateChange { Target = "compare.b", Old = null, New = b.ToJson() });

                    Recorder.Record("compare", bin.NodeId, diff);
                    return result;
                }

            case BinaryExpr bin when bin.Op == "and":
                {
                    var leftTrue = RecordComparisonIfApplicable(bin.A, frame);
                    if (leftTrue)
                        return RecordComparisonIfApplicable(bin.B, frame);
                    return false;
                }

            case BinaryExpr bin when bin.Op == "or":
                {
                    var leftTrue = RecordComparisonIfApplicable(bin.A, frame);
                    if (leftTrue) return true;
                    return RecordComparisonIfApplicable(bin.B, frame);
                }

            case UnaryExpr un when un.Op == "not":
                return !RecordComparisonIfApplicable(un.A, frame);

            default:
                return true;  // не сравнение — считаем ветку "пройденной"
        }
    }

    private static bool EvaluateComparison(string op, RuntimeValue? a, RuntimeValue? b)
    {
        if (a is null || b is null) return false;
        return op switch
        {
            "==" => a.ValueEquals(b),
            "!=" => !a.ValueEquals(b),
            "<" => CompareNumeric(a, b, (x, y) => x < y),
            "<=" => CompareNumeric(a, b, (x, y) => x <= y),
            ">" => CompareNumeric(a, b, (x, y) => x > y),
            ">=" => CompareNumeric(a, b, (x, y) => x >= y),
            _ => false
        };
    }

    private static bool CompareNumeric(RuntimeValue a, RuntimeValue b, Func<double, double, bool> cmp)
    {
        var x = a is IntValue ii ? ii.Value : a is FloatValue ff ? ff.Value : 0;
        var y = b is IntValue jj ? jj.Value : b is FloatValue gg ? gg.Value : 0;
        return cmp(x, y);
    }

    private void ExecuteWhile(WhileStatement s, Frame frame)
    {
        while (true)
        {
            RecordComparisonIfApplicable(s.Cond, frame);
            if (!Evaluator.AsBool(Eval.Eval(s.Cond, frame))) break;
            CheckLimits();
            try { ExecuteBlock(s.Body, frame); }
            catch (BreakException) { break; }
            catch (ContinueException) { continue; }
        }
    }

    private void ExecuteFor(ForStatement s, Frame frame)
    {
        var from = Evaluator.AsInt(Eval.Eval(s.From, frame), "for.from");
        var to = Evaluator.AsInt(Eval.Eval(s.To, frame), "for.to");
        var step = s.Step is null ? 1 : Evaluator.AsInt(Eval.Eval(s.Step, frame), "for.step");
        if (step == 0) throw new YawaRuntimeException("for step cannot be 0");

        if (step > 0)
        {
            for (var i = from; i < to; i += step)
            {
                var oldVal = frame.Has(s.Var) ? frame.Get(s.Var) : null;
                frame.Declare(s.Var, new IntValue(i));
                Recorder.Record("for", s.NodeId, new[]
                {
            new Trace.StateChange { Target = s.Var, Old = oldVal?.ToJson(), New = i }
        });
                try { ExecuteBlock(s.Body, frame); }
                catch (BreakException) { break; }
                catch (ContinueException) { continue; }
            }
        }
        else
        {
            for (var i = from; i > to; i += step)
            {
                var oldVal = frame.Has(s.Var) ? frame.Get(s.Var) : null;
                frame.Declare(s.Var, new IntValue(i));
                Recorder.Record("for", s.NodeId, new[]
                {
            new Trace.StateChange { Target = s.Var, Old = oldVal?.ToJson(), New = i }
        });
                try { ExecuteBlock(s.Body, frame); }
                catch (BreakException) { break; }
                catch (ContinueException) { continue; }
            }
        }
    }

    private void ExecuteForeach(ForeachStatement s, Frame frame)
    {
        var collection = Eval.Eval(s.In, frame);
        IEnumerable<RuntimeValue> items = collection switch
        {
            ArrayValue a => a.Items,
            StringValue str => str.Value.Select(c => (RuntimeValue)new StringValue(c.ToString())),
            ObjectValue obj => obj.Fields
                .Where(kv => !kv.Key.StartsWith("__"))
                .Select(kv => (RuntimeValue)new StringValue(kv.Key)),
            SetValue s1 => s1.Items,
            _ => throw new YawaRuntimeException($"foreach on {collection.TypeName}")
        };
        foreach (var item in items.ToList())
        {
            var oldVal = frame.Has(s.Var) ? frame.Get(s.Var) : null;
            frame.Declare(s.Var, item);
            Recorder.Record("foreach", s.NodeId, new[]
            {
        new Trace.StateChange { Target = s.Var, Old = oldVal?.ToJson(), New = item.ToJson() }
    });
            try { ExecuteBlock(s.Body, frame); }
            catch (BreakException) { break; }
            catch (ContinueException) { continue; }
        }
    }

    private void ExecuteSwap(SwapStatement s, Frame frame)
    {
        if (s.A is not IndexExpr ai || s.B is not IndexExpr bi)
            throw new YawaRuntimeException("swap only supports array index targets");

        var arrA = Eval.Eval(ai.Target, frame) as ArrayValue
            ?? throw new YawaRuntimeException("swap: target not array");
        var arrB = Eval.Eval(bi.Target, frame) as ArrayValue
            ?? throw new YawaRuntimeException("swap: target not array");
        var i = (int)Evaluator.AsInt(Eval.Eval(ai.Index, frame), "swap index");
        var j = (int)Evaluator.AsInt(Eval.Eval(bi.Index, frame), "swap index");

        var aName = RenderTargetPath(ai.Target, frame);
        var bName = RenderTargetPath(bi.Target, frame);

        var oldA = arrA[i];
        var oldB = arrB[j];
        arrA[i] = oldB;
        arrB[j] = oldA;

        Recorder.Stats.Swaps++;
        Recorder.AddHighlight($"{aName}[{i}]");
        Recorder.AddHighlight($"{bName}[{j}]");

        Recorder.Record("swap", s.NodeId, new[]
        {
            new Trace.StateChange { Target = $"{aName}[{i}]", Old = oldA.ToJson(), New = oldB.ToJson() },
            new Trace.StateChange { Target = $"{bName}[{j}]", Old = oldB.ToJson(), New = oldA.ToJson() }
        });
    }

    private void ExecuteCompare(CompareStatement s, Frame frame)
    {
        var a = Eval.Eval(s.A, frame);
        var b = Eval.Eval(s.B, frame);

        Recorder.Stats.Comparisons++;
        if (s.A is IndexExpr ai)
        {
            var path = RenderIndexPathSafe(ai, frame);
            if (path is not null) Recorder.AddHighlight(path);
        }
        if (s.B is IndexExpr bi)
        {
            var path = RenderIndexPathSafe(bi, frame);
            if (path is not null) Recorder.AddHighlight(path);
        }

        if (s.Label is not null) Recorder.SetAnnotation(s.Label);

        Recorder.Record("compare", s.NodeId, new[]
        {
            new Trace.StateChange { Target = "compare.a", Old = null, New = a.ToJson() },
            new Trace.StateChange { Target = "compare.b", Old = null, New = b.ToJson() }
        });
    }

    private string RenderIndexPath(IndexExpr ix, Frame frame)
    {
        var targetName = RenderTargetPath(ix.Target, frame);
        var idxVal = Eval.Eval(ix.Index, frame);
        if (idxVal is IntValue iv) return $"{targetName}[{iv.Value}]";
        if (idxVal is StringValue sv) return $"{targetName}[\"{sv.Value}\"]";
        return $"{targetName}[?]";
    }

    /// <summary>
    /// Безопасная версия: возвращает null, если индекс не int (например,
    /// словарный доступ node["value"]). Нужна для автоматической подсветки
    /// в RecordComparisonIfApplicable, где падать нельзя.
    /// </summary>
    private string? RenderIndexPathSafe(IndexExpr ix, Frame frame)
    {
        try
        {
            var targetName = RenderTargetPath(ix.Target, frame);
            var idxVal = Eval.Eval(ix.Index, frame);
            if (idxVal is IntValue iv) return $"{targetName}[{iv.Value}]";
            return null;
        }
        catch { return null; }
    }

    private void ExecuteMakeNode(MakeNodeStatement s, Frame frame)
    {
        var value = Eval.Eval(s.Value, frame);
        var node = new TreeNodeValue { Value = value };

        if (s.Left is not null)
        {
            var l = Eval.Eval(s.Left, frame);
            node.Left = l switch
            {
                TreeNodeValue tn => tn,
                NullValue => null,
                _ => throw new YawaRuntimeException($"make_node left: expected TreeNode or null, got {l.TypeName}")
            };
        }
        if (s.Right is not null)
        {
            var r = Eval.Eval(s.Right, frame);
            node.Right = r switch
            {
                TreeNodeValue tn => tn,
                NullValue => null,
                _ => throw new YawaRuntimeException($"make_node right: expected TreeNode or null, got {r.TypeName}")
            };
        }

        AssignTo(s.Target, node, frame, s.NodeId);
    }

    private void ExecuteMark(MarkStatement s, Frame frame)
    {
        if (s.Target is IndexExpr ix)
        {
            var arr = Eval.Eval(ix.Target, frame) as ArrayValue;
            var i = (int)Evaluator.AsInt(Eval.Eval(ix.Index, frame), "mark index");
            if (arr is null) throw new YawaRuntimeException("mark: target not array");
            arr.Highlights[i] = s.Color;
            if (s.Label is not null) arr.Labels[i] = s.Label;
            Recorder.AddHighlight($"{RenderTargetPath(ix.Target, frame)}[{i}]");
        }
        Recorder.Record("mark", s.NodeId);
    }

    private void ExecuteUnmark(UnmarkStatement s, Frame frame)
    {
        if (s.Target is IndexExpr ix)
        {
            var arr = Eval.Eval(ix.Target, frame) as ArrayValue;
            var i = (int)Evaluator.AsInt(Eval.Eval(ix.Index, frame), "unmark index");
            arr?.Highlights.Remove(i);
        }
        Recorder.Record("unmark", s.NodeId);
    }

    private static string ShortRepr(RuntimeValue v)
    {
        var s = v.ToString() ?? "?";
        return s.Length > 40 ? s[..37] + "..." : s;
    }

    private void CheckLimits()
    {
        _stepBudget--;
        if (_stepBudget <= 0)
            throw new YawaRuntimeException($"Max steps exceeded ({_options.MaxSteps})");

        if (_clock.Elapsed.TotalSeconds > _options.MaxSeconds)
            throw new YawaRuntimeException($"Max time exceeded ({_options.MaxSeconds}s)");
    }
}