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

    public RuntimeValue CallFunction(string name, List<RuntimeValue> args, Frame callerFrame, string? nodeId,
    bool syncLocalsBackToCaller = false)
    {
        CheckLimits();

        // Сначала ищем пользовательскую функцию — она перекрывает builtin.
        if (!_functions.TryGetValue(name, out var fn))
        {
            // Пользовательской нет — пробуем builtin.
            if (Builtins.Names.Contains(name))
            {
                var result = Builtins.Call(name, args, Recorder);
                if (name == "print")
                    Recorder.Record("call", nodeId);
                return result;
            }
            throw new YawaRuntimeException($"Unknown function: {name}");
        }

        var frame = new Frame(name, callerFrame);
        if (args.Count != fn.Params.Count)
            throw new YawaRuntimeException(
                $"{name}: expected {fn.Params.Count} args, got {args.Count}");

        for (int i = 0; i < fn.Params.Count; i++)
            frame.Declare(fn.Params[i].Name, args[i]);

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
            if (syncLocalsBackToCaller)
                SyncLocals(fn, frame, callerFrame);
            return NullValue.Instance;
        }
        catch (ReturnException rex)
        {
            if (syncLocalsBackToCaller)
                SyncLocals(fn, frame, callerFrame);
            return rex.Value;
        }
        finally
        {
            Recorder.SetFrame(callerFrame);
            Recorder.RecordSnapshot(frame, $"exit {name}");
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

        // 2. Builtin-метод для коллекций (append → push, pop → pop, ...)
        var builtinName = name switch
        {
            "append" => "push",
            "pop" => "pop",
            "insert" => "insert",
            "remove" => "remove",
            _ => name
        };

        if (Builtins.Names.Contains(builtinName))
        {
            var args = new List<RuntimeValue> { receiver };
            foreach (var expr in argExprs)
                args.Add(Eval.Eval(expr, frame));
            return Builtins.Call(builtinName, args, Recorder);
        }

        // 3. Fallback — глобальная функция с receiver первым аргументом.
        var defaultArgs = new List<RuntimeValue> { receiver };
        foreach (var expr in argExprs)
            defaultArgs.Add(Eval.Eval(expr, frame));
        return CallFunction(name, defaultArgs, frame, nodeId);
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
            default:
                throw new YawaRuntimeException($"Unknown statement: {stmt.GetType().Name}");
        }
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

    private void ExecuteTupleAssign(TupleAssignStatement s, Frame frame)
    {
        if (s.Targets.Count != s.Values.Count)
            throw new YawaRuntimeException(
                $"tuple assign: {s.Targets.Count} targets vs {s.Values.Count} values");

        // Сначала вычисляем ВСЕ значения (Python-семантика: правая часть вычисляется до присваивания)
        var evaluated = s.Values.Select(v => Eval.Eval(v, frame)).ToList();

        // Потом присваиваем по порядку
        for (int i = 0; i < s.Targets.Count; i++)
            AssignTo(s.Targets[i], evaluated[i], frame, s.NodeId);
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
        return expr switch
        {
            RefExpr r => r.Name,
            IndexExpr ix => RenderTargetPath(ix.Target, frame),
            FieldExpr fl => RenderTargetPath(fl.Target, frame) + "." + fl.FieldName,
            _ => "<expr>"
        };
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