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

        // Builtin?
        if (Builtins.Names.Contains(name))
        {
            var result = Builtins.Call(name, args, Recorder);
            if (name == "print")
                Recorder.Record("call", nodeId);
            return result;
        }

        if (!_functions.TryGetValue(name, out var fn))
            throw new YawaRuntimeException($"Unknown function: {name}");

        var frame = new Frame(name, callerFrame);
        if (args.Count != fn.Params.Count)
            throw new YawaRuntimeException(
                $"{name}: expected {fn.Params.Count} args, got {args.Count}");

        for (int i = 0; i < fn.Params.Count; i++)
            frame.Declare(fn.Params[i].Name, args[i]);

        Recorder.SetFrame(frame);
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
                    var idx = (int)Evaluator.AsInt(Eval.Eval(ix.Index, frame), "index");
                    if (targetVal is not ArrayValue arr)
                        throw new YawaRuntimeException("assign to non-array index");
                    var old = arr[idx];
                    arr[idx] = value;
                    var targetName = RenderTargetPath(ix.Target, frame);
                    Recorder.Record("assign", nodeId, new[]
                    {
                    new Trace.StateChange { Target = $"{targetName}[{idx}]", Old = old.ToJson(), New = value.ToJson() }
                });
                    break;
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
        var cond = Evaluator.AsBool(Eval.Eval(s.Cond, frame));
        var branch = cond ? s.Then : (s.Else ?? new List<YawaStatement>());
        ExecuteBlock(branch, frame);
    }

    private void ExecuteWhile(WhileStatement s, Frame frame)
    {
        while (Evaluator.AsBool(Eval.Eval(s.Cond, frame)))
        {
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
        if (s.A is IndexExpr ai) Recorder.AddHighlight(RenderIndexPath(ai, frame));
        if (s.B is IndexExpr bi) Recorder.AddHighlight(RenderIndexPath(bi, frame));

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
        var idx = (int)Evaluator.AsInt(Eval.Eval(ix.Index, frame), "index");
        return $"{targetName}[{idx}]";
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

    private void CheckLimits()
    {
        _stepBudget--;
        if (_stepBudget <= 0)
            throw new YawaRuntimeException($"Max steps exceeded ({_options.MaxSteps})");

        if (_clock.Elapsed.TotalSeconds > _options.MaxSeconds)
            throw new YawaRuntimeException($"Max time exceeded ({_options.MaxSeconds}s)");
    }
}