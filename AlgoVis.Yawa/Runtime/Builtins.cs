namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Встроенные функции YAWA. Каждая принимает аргументы и возвращает RuntimeValue.
/// </summary>
public static class Builtins
{
    public static readonly HashSet<string> Names = new()
    {
        "length", "len", "min", "max", "abs", "print", "range",
        "push", "pop", "insert", "remove", "contains", "find"
    };

    public static RuntimeValue Call(string name, List<RuntimeValue> args, TraceRecorder rec)
    {
        switch (name)
        {
            case "length":
            case "len":
                return args[0] switch
                {
                    ArrayValue a  => new IntValue(a.Count),
                    StringValue s => new IntValue(s.Value.Length),
                    _ => throw new YawaRuntimeException($"length() on {args[0].TypeName}")
                };

            case "min":
                return args.Count == 1 && args[0] is ArrayValue arrMin
                    ? arrMin.Items.OrderBy(v => (v as IntValue)?.Value ?? 0).First()
                    : args.OrderBy(v => (v as IntValue)?.Value ?? 0).First();

            case "max":
                return args.Count == 1 && args[0] is ArrayValue arrMax
                    ? arrMax.Items.OrderBy(v => (v as IntValue)?.Value ?? 0).Last()
                    : args.OrderBy(v => (v as IntValue)?.Value ?? 0).Last();

            case "abs":
                return args[0] switch
                {
                    IntValue i   => new IntValue(Math.Abs(i.Value)),
                    FloatValue f => new FloatValue(Math.Abs(f.Value)),
                    _ => throw new YawaRuntimeException($"abs() on {args[0].TypeName}")
                };

            case "print":
                {
                    var text = string.Join(" ", args.Select(v => v is StringValue s ? s.Value : v.ToString()));
                    rec.SetAnnotation(text);
                    return NullValue.Instance;
                }

            case "range":
                {
                    long start = 0, stop, step = 1;
                    if (args.Count == 1) stop = Evaluator.AsInt(args[0], "range");
                    else if (args.Count == 2)
                    {
                        start = Evaluator.AsInt(args[0], "range");
                        stop  = Evaluator.AsInt(args[1], "range");
                    }
                    else
                    {
                        start = Evaluator.AsInt(args[0], "range");
                        stop  = Evaluator.AsInt(args[1], "range");
                        step  = Evaluator.AsInt(args[2], "range");
                    }
                    var items = new List<RuntimeValue>();
                    if (step > 0) for (var i = start; i < stop; i += step) items.Add(new IntValue(i));
                    else if (step < 0) for (var i = start; i > stop; i += step) items.Add(new IntValue(i));
                    return new ArrayValue(items, "int");
                }

            case "push":
                {
                    if (args[0] is not ArrayValue arr) throw new YawaRuntimeException("push() needs array");
                    arr.Items.Add(args[1]);
                    return new IntValue(arr.Count);
                }

            case "pop":
                {
                    if (args[0] is not ArrayValue arr) throw new YawaRuntimeException("pop() needs array");
                    if (arr.Count == 0) throw new YawaRuntimeException("pop() on empty array");
                    var last = arr.Items[^1];
                    arr.Items.RemoveAt(arr.Count - 1);
                    return last;
                }

            case "insert":
                {
                    if (args[0] is not ArrayValue arr) throw new YawaRuntimeException("insert() needs array");
                    var idx = (int)Evaluator.AsInt(args[1], "insert index");
                    arr.Items.Insert(idx, args[2]);
                    return NullValue.Instance;
                }

            case "remove":
                {
                    if (args[0] is not ArrayValue arr) throw new YawaRuntimeException("remove() needs array");
                    var idx = (int)Evaluator.AsInt(args[1], "remove index");
                    var v = arr.Items[idx];
                    arr.Items.RemoveAt(idx);
                    return v;
                }

            case "contains":
                {
                    if (args[0] is not ArrayValue arr) throw new YawaRuntimeException("contains() needs array");
                    return arr.Items.Any(x => x.ValueEquals(args[1])) ? BoolValue.True : BoolValue.False;
                }

            case "find":
                {
                    if (args[0] is not ArrayValue arr) throw new YawaRuntimeException("find() needs array");
                    for (int i = 0; i < arr.Count; i++)
                        if (arr.Items[i].ValueEquals(args[1])) return new IntValue(i);
                    return new IntValue(-1);
                }

            default:
                throw new YawaRuntimeException($"Unknown builtin: {name}");
        }
    }
}