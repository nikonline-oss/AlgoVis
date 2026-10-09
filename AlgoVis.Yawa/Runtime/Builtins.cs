namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Встроенные функции YAWA. Каждая принимает аргументы и возвращает RuntimeValue.
/// </summary>
public static class Builtins
{
    public static readonly HashSet<string> Names = new()
    {
        "length", "len", "min", "max", "abs", "print", "range",
        "push", "pop", "insert", "remove", "contains", "find",
        "set", "add", "discard", "remove_from_set",
        "chr", "ord", "int", "str", "float", "bool", "sum", "any", "all",
        "enumerate", "zip", "list", "tuple", "sorted", "sorted_by_abs", "sorted_by_len",
        "sorted_with", "make_pair", "sort", "reverse", "min_element", "max_element", "accumulate",
        "erase_first"
    };

    public static RuntimeValue Call(string name, List<RuntimeValue> args, TraceRecorder rec)
    {
        switch (name)
        {
            case "length":
            case "len":
                return args[0] switch
                {
                    ArrayValue a => new IntValue(a.Count),
                    StringValue s => new IntValue(s.Value.Length),
                    SetValue st => new IntValue(st.Count),
                    ObjectValue o => new IntValue(o.Fields.Count(kv => !kv.Key.StartsWith("__"))),
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
                    IntValue i => new IntValue(Math.Abs(i.Value)),
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
                        stop = Evaluator.AsInt(args[1], "range");
                    }
                    else
                    {
                        start = Evaluator.AsInt(args[0], "range");
                        stop = Evaluator.AsInt(args[1], "range");
                        step = Evaluator.AsInt(args[2], "range");
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

            case "add":
                {
                    if (args[0] is not SetValue s) throw new YawaRuntimeException("add() needs set");
                    s.Add(args[1]);
                    return NullValue.Instance;
                }

            case "discard":
                {
                    if (args[0] is not SetValue s) throw new YawaRuntimeException("discard() needs set");
                    s.Remove(args[1]);
                    return NullValue.Instance;
                }
            case "chr":
                {
                    var code = Evaluator.AsInt(args[0], "chr");
                    return new StringValue(((char)code).ToString());
                }

            case "ord":
                {
                    if (args[0] is not StringValue s || s.Value.Length == 0)
                        throw new YawaRuntimeException("ord() needs non-empty string");
                    return new IntValue(s.Value[0]);
                }

            case "int":
                return args[0] switch
                {
                    IntValue i => i,
                    FloatValue f => new IntValue((long)f.Value),
                    StringValue s => long.TryParse(s.Value.Trim(), out var r)
                        ? new IntValue(r)
                        : throw new YawaRuntimeException($"int(): cannot parse '{s.Value}'"),
                    BoolValue b => new IntValue(b.Value ? 1 : 0),
                    _ => throw new YawaRuntimeException($"int() on {args[0].TypeName}")
                };

            case "float":
                return args[0] switch
                {
                    IntValue i => new FloatValue(i.Value),
                    FloatValue f => f,
                    StringValue s => double.TryParse(s.Value.Trim(),
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var r)
                        ? new FloatValue(r)
                        : throw new YawaRuntimeException($"float(): cannot parse '{s.Value}'"),
                    _ => throw new YawaRuntimeException($"float() on {args[0].TypeName}")
                };

            case "bool":
                return Evaluator.AsBool(args[0]) ? BoolValue.True : BoolValue.False;

            case "str":
                return args[0] switch
                {
                    StringValue s => s,
                    IntValue i => new StringValue(i.Value.ToString()),
                    FloatValue f => new StringValue(f.Value.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)),
                    BoolValue b => new StringValue(b.Value ? "True" : "False"),
                    NullValue => new StringValue("None"),
                    _ => new StringValue(args[0].ToString() ?? "")
                };

            case "sum":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("sum() needs array");
                    long total = 0;
                    double ftotal = 0;
                    bool anyFloat = false;
                    foreach (var v in arr.Items)
                    {
                        if (v is IntValue iv) total += iv.Value;
                        else if (v is FloatValue fv) { anyFloat = true; ftotal += fv.Value; }
                        else throw new YawaRuntimeException("sum(): non-numeric value");
                    }
                    return anyFloat ? new FloatValue(total + ftotal) : new IntValue(total);
                }

            case "any":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("any() needs array");
                    foreach (var v in arr.Items)
                        if (Evaluator.AsBool(v)) return BoolValue.True;
                    return BoolValue.False;
                }

            case "all":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("all() needs array");
                    foreach (var v in arr.Items)
                        if (!Evaluator.AsBool(v)) return BoolValue.False;
                    return BoolValue.True;
                }

            case "list":
                {
                    if (args.Count == 0) return new ArrayValue(Enumerable.Empty<RuntimeValue>());
                    return new ArrayValue(ToIterable(args[0]).ToList());
                }

            case "tuple":
                {
                    if (args.Count == 0) return new TupleValue(Enumerable.Empty<RuntimeValue>());
                    return new TupleValue(ToIterable(args[0]).ToList());
                }

            case "set":
                {
                    var s = new SetValue();
                    if (args.Count > 0)
                        foreach (var v in ToIterable(args[0])) s.Add(v);
                    return s;
                }

            case "enumerate":
                {
                    var arr = args[0] as ArrayValue
                        ?? throw new YawaRuntimeException("enumerate() needs array");
                    var result = new List<RuntimeValue>();
                    for (int i = 0; i < arr.Count; i++)
                    {
                        result.Add(new TupleValue(new[]
                        {
                (RuntimeValue)new IntValue(i),
                arr[i]
            }));
                    }
                    return new ArrayValue(result);
                }

            case "zip":
                {
                    if (args.Count == 0) return new ArrayValue(Enumerable.Empty<RuntimeValue>());
                    var arrays = new List<ArrayValue>();
                    foreach (var a in args)
                    {
                        if (a is not ArrayValue av)
                            throw new YawaRuntimeException($"zip() needs arrays, got {a.TypeName}");
                        arrays.Add(av);
                    }
                    int minLen = arrays.Min(a => a.Count);
                    var result = new List<RuntimeValue>();
                    for (int i = 0; i < minLen; i++)
                    {
                        result.Add(new TupleValue(arrays.Select(a => a[i])));
                    }
                    return new ArrayValue(result);
                }

            case "sorted":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("sorted() needs array");
                    var items = arr.Items.ToList();
                    items.Sort(CompareValues);
                    return new ArrayValue(items, arr.ElementType);
                }

            case "sorted_by_abs":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("sorted_by_abs() needs array");
                    var items = arr.Items
                        .OrderBy(v => Math.Abs(ToNumber(v)))
                        .ToList();
                    return new ArrayValue(items, arr.ElementType);
                }

            case "sorted_by_len":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("sorted_by_len() needs array");
                    var items = arr.Items
                        .OrderBy(v => ValueLength(v))
                        .ToList();
                    return new ArrayValue(items, arr.ElementType);
                }

            case "make_pair":
                {
                    if (args.Count != 2)
                        throw new YawaRuntimeException("make_pair() needs 2 args");
                    return new TupleValue(new[] { args[0], args[1] });
                }

            case "sort":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("sort() needs array");
                    var items = arr.Items.ToList();
                    items.Sort(CompareValues);
                    arr.Items.Clear();
                    arr.Items.AddRange(items);
                    return NullValue.Instance;
                }

            case "reverse":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("reverse() needs array");
                    arr.Items.Reverse();
                    return NullValue.Instance;
                }

            case "min_element":
                {
                    if (args[0] is not ArrayValue arr || arr.Count == 0)
                        throw new YawaRuntimeException("min_element() needs non-empty array");
                    var min = arr.Items[0];
                    foreach (var v in arr.Items)
                        if (CompareValues(v, min) < 0) min = v;
                    return min;
                }

            case "max_element":
                {
                    if (args[0] is not ArrayValue arr || arr.Count == 0)
                        throw new YawaRuntimeException("max_element() needs non-empty array");
                    var max = arr.Items[0];
                    foreach (var v in arr.Items)
                        if (CompareValues(v, max) > 0) max = v;
                    return max;
                }

            case "accumulate":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("accumulate() needs array");
                    var init = args.Count > 1 ? args[1] : (RuntimeValue)new IntValue(0);

                    // Числовое суммирование
                    long total = 0;
                    double ftotal = 0;
                    bool isFloat = false;
                    if (init is IntValue ii) total = ii.Value;
                    else if (init is FloatValue ff) { isFloat = true; ftotal = ff.Value; }

                    foreach (var v in arr.Items)
                    {
                        if (v is IntValue iv) { total += iv.Value; }
                        else if (v is FloatValue fv) { isFloat = true; ftotal += fv.Value; }
                    }
                    return isFloat ? new FloatValue(total + ftotal) : new IntValue(total);
                }
            case "erase_first":
                {
                    if (args[0] is not ArrayValue arr)
                        throw new YawaRuntimeException("erase_first() needs array");
                    if (arr.Count == 0) return NullValue.Instance;
                    arr.Items.RemoveAt(0);
                    return NullValue.Instance;
                }

            default:
                throw new YawaRuntimeException($"Unknown builtin: {name}");
        }
    }
    // ─────────── Helpers ───────────

    private static IEnumerable<RuntimeValue> ToIterable(RuntimeValue v) => v switch
    {
        ArrayValue a => a.Items,
        SetValue s => s.Items,
        TupleValue t => t.Items,
        StringValue st => st.Value.Select(c => (RuntimeValue)new StringValue(c.ToString())),
        ObjectValue o => o.Fields
            .Where(kv => !kv.Key.StartsWith("__"))
            .Select(kv => (RuntimeValue)new StringValue(kv.Key)),
        _ => throw new YawaRuntimeException($"Cannot iterate {v.TypeName}")
    };

    private static double ToNumber(RuntimeValue v) => v switch
    {
        IntValue i => i.Value,
        FloatValue f => f.Value,
        _ => throw new YawaRuntimeException($"Expected number, got {v.TypeName}")
    };

    private static int ValueLength(RuntimeValue v) => v switch
    {
        StringValue s => s.Value.Length,
        ArrayValue a => a.Count,
        SetValue s => s.Count,
        TupleValue t => t.Count,
        _ => throw new YawaRuntimeException($"len() not applicable to {v.TypeName}")
    };

    private static int CompareValues(RuntimeValue a, RuntimeValue b)
    {
        // Числа
        if ((a is IntValue || a is FloatValue) && (b is IntValue || b is FloatValue))
            return ToNumber(a).CompareTo(ToNumber(b));
        // Строки
        if (a is StringValue sa && b is StringValue sb)
            return string.CompareOrdinal(sa.Value, sb.Value);
        // Bool
        if (a is BoolValue ba && b is BoolValue bb)
            return ba.Value.CompareTo(bb.Value);
        throw new YawaRuntimeException(
            $"Cannot compare {a.TypeName} and {b.TypeName}");
    }
}