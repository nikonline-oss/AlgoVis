using AlgoVis.Yawa.Yawa.Expressions;

namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Вычисляет YawaExpression в RuntimeValue.
/// </summary>
public sealed class Evaluator
{
    private readonly Interpreter _interp;

    public Evaluator(Interpreter interp) => _interp = interp;

    public RuntimeValue Eval(YawaExpression expr, Frame frame)
    {
        switch (expr)
        {
            case LiteralExpr lit: return FromJson(lit.Value);
            case RefExpr r: return frame.Get(r.Name);
            case IndexExpr ix: return EvalIndex(ix, frame);
            case FieldExpr fl: return EvalField(fl, frame);
            case BinaryExpr b: return EvalBinary(b, frame);
            case UnaryExpr u: return EvalUnary(u, frame);
            case CallExpr c: return _interp.CallFunction(c.Name, c.Args, frame, c.NodeId);
            case CallMethodExpr cm: return EvalCallMethod(cm, frame);
            case NewObjectExpr no: return EvalNewObject(no, frame);
            case LenExpr le: return EvalLen(le, frame);
            case TernaryExpr te: return EvalTernary(te, frame);
            case ArrayExpr arr:
                return new ArrayValue(arr.Items.Select(x => Eval(x, frame)));
            case SliceExpr sl:
                return EvalSlice(sl, frame);
            case DictExpr d:
                {
                    var obj = new ObjectValue();
                    foreach (var item in d.Items)
                    {
                        var key = Eval(item.Key, frame);
                        var val = Eval(item.Value, frame);
                        var keyStr = key is StringValue sv ? sv.Value : key.ToString();
                        obj.SetField(keyStr, val);
                    }
                    return obj;
                }
            case InstantiateExpr inst:
                return _interp.Instantiate(inst.ClassName, inst.Args, frame, inst.NodeId);
            case SetLiteralExpr sl:
                {
                    var set = new SetValue();
                    foreach (var expr1 in sl.Items)
                        set.Add(Eval(expr1, frame));
                    return set;
                }
            case TupleExpr tup:
                return new TupleValue(tup.Items.Select(x => Eval(x, frame)));
            case ListCompExpr lc:
                return EvalListComp(lc.Body, lc.Clauses, frame);

            case SetCompExpr sc:
                {
                    var set = new SetValue();
                    foreach (var item in IterComprehension(sc.Clauses, frame))
                        set.Add(Eval(sc.Body, item));
                    return set;
                }

            case DictCompExpr dc:
                {
                    var obj = new ObjectValue();
                    foreach (var item in IterComprehension(dc.Clauses, frame))
                    {
                        var key = Eval(dc.BodyKey, item);
                        var val = Eval(dc.BodyValue, item);
                        var keyStr = key is StringValue sv ? sv.Value : key.ToString() ?? "";
                        obj.SetField(keyStr, val);
                    }
                    return obj;
                }

            case FunctionRefExpr fr:
                return new FunctionRefValue(fr.Name, frame);

            default:
                throw new YawaRuntimeException($"Unknown expression type: {expr.GetType().Name}");
        }
    }

    private RuntimeValue EvalIndex(IndexExpr ix, Frame frame)
    {
        var target = Eval(ix.Target, frame);
        var index = Eval(ix.Index, frame);

        if (target is ArrayValue arr)
        {
            var i = AsInt(index, "array index");
            if (i < 0) i += arr.Count;
            if (i < 0 || i >= arr.Count)
                throw new YawaRuntimeException($"Array index out of range: {i} (size {arr.Count})");
            _interp.Recorder.Stats.MemoryAccesses++;
            return arr[(int)i];
        }

        if (target is StringValue s)
        {
            var i = AsInt(index, "string index");
            if (i < 0) i += s.Value.Length;
            if (i < 0 || i >= s.Value.Length)
                throw new YawaRuntimeException($"String index out of range: {i}");
            return new StringValue(s.Value[(int)i].ToString());
        }

        if (target is ObjectValue obj)
        {
            var key = index is StringValue sv ? sv.Value : index.ToString();
            _interp.Recorder.Stats.MemoryAccesses++;
            if (!obj.HasField(key))
                throw new YawaRuntimeException($"Key '{key}' not found in dictionary");
            return obj.GetField(key);
        }

        if (target is TupleValue tup)
        {
            var i = AsInt(index, "tuple index");
            if (i < 0) i += tup.Count;
            if (i < 0 || i >= tup.Count)
                throw new YawaRuntimeException($"Tuple index out of range: {i} (size {tup.Count})");
            return tup[(int)i];
        }

        throw new YawaRuntimeException($"Cannot index value of type {target.TypeName}");
    }

    private RuntimeValue EvalField(FieldExpr fl, Frame frame)
    {
        var target = Eval(fl.Target, frame);
        if (target is ObjectValue obj)
        {
            _interp.Recorder.Stats.MemoryAccesses++;
            return obj.GetField(fl.FieldName);
        }
        if (target is TreeNodeValue tn)
        {
            return fl.FieldName switch
            {
                "value" => tn.Value,
                "left" => (RuntimeValue?)tn.Left ?? NullValue.Instance,
                "right" => (RuntimeValue?)tn.Right ?? NullValue.Instance,
                _ => throw new YawaRuntimeException($"TreeNode has no field '{fl.FieldName}'")
            };
        }
        if (target is TupleValue tup)
        {
            var idx = fl.FieldName switch
            {
                "first" => 0,
                "second" => 1,
                _ => -1
            };
            if (idx >= 0 && idx < tup.Count)
                return tup[idx];
        }
        throw new YawaRuntimeException($"Cannot access field '{fl.FieldName}' on {target.TypeName}");
    }

    private RuntimeValue EvalBinary(BinaryExpr b, Frame frame)
    {
        // Short-circuit для and / or
        if (b.Op == "and")
        {
            var a = Eval(b.A, frame);
            if (!AsBool(a)) return BoolValue.False;
            return AsBool(Eval(b.B, frame)) ? BoolValue.True : BoolValue.False;
        }
        if (b.Op == "or")
        {
            var a = Eval(b.A, frame);
            if (AsBool(a)) return BoolValue.True;
            return AsBool(Eval(b.B, frame)) ? BoolValue.True : BoolValue.False;
        }

        var left = Eval(b.A, frame);
        var right = Eval(b.B, frame);

        return b.Op switch
        {
            "+" => ArithmeticAdd(left, right),
            "-" => Arith(left, right, (x, y) => x - y, (x, y) => x - y),
            "*" => Multiply(left, right),
            "//" => IntDiv(left, right),
            "/" => Div(left, right),
            "%" => Mod(left, right),
            "**" => Power(left, right),
            "==" => Bool(left.ValueEquals(right)),
            "!=" => Bool(!left.ValueEquals(right)),
            "<" => Compare(left, right, (x, y) => x < y),
            "<=" => Compare(left, right, (x, y) => x <= y),
            ">" => Compare(left, right, (x, y) => x > y),
            ">=" => Compare(left, right, (x, y) => x >= y),
            "in" => In(left, right),
            "not_in" => Bool(!AsBool(In(left, right))),
            _ => throw new YawaRuntimeException($"Unknown binary op: {b.Op}")
        };
    }

    private RuntimeValue EvalUnary(UnaryExpr u, Frame frame)
    {
        var a = Eval(u.A, frame);
        return u.Op switch
        {
            "-" => a is IntValue i ? new IntValue(-i.Value)
                   : a is FloatValue f ? new FloatValue(-f.Value)
                   : throw new YawaRuntimeException("Unary '-' on non-number"),
            "+" => a,
            "not" => Bool(!AsBool(a)),
            _ => throw new YawaRuntimeException($"Unknown unary op: {u.Op}")
        };
    }

    private RuntimeValue EvalCallMethod(CallMethodExpr cm, Frame frame)
    {
        var receiver = Eval(cm.Receiver, frame);
        return _interp.CallMethod(receiver, cm.Name, cm.Args, frame, cm.NodeId);
    }

    private RuntimeValue EvalNewObject(NewObjectExpr no, Frame frame)
    {
        var obj = new ObjectValue();
        foreach (var (name, expr) in no.Fields)
            obj.SetField(name, Eval(expr, frame));
        return obj;
    }

    private RuntimeValue EvalLen(LenExpr le, Frame frame)
    {
        var v = Eval(le.Target, frame);
        return v switch
        {
            ArrayValue a => new IntValue(a.Count),
            StringValue s => new IntValue(s.Value.Length),
            SetValue st => new IntValue(st.Count),
            ObjectValue o => new IntValue(o.Fields.Count(kv => !kv.Key.StartsWith("__"))),
            _ => throw new YawaRuntimeException($"len() on unsupported type {v.TypeName}")
        };
    }
    private RuntimeValue EvalListComp(YawaExpression body, List<CompClause> clauses, Frame frame)
    {
        var result = new List<RuntimeValue>();
        foreach (var itemFrame in IterComprehension(clauses, frame))
            result.Add(Eval(body, itemFrame));
        return new ArrayValue(result);
    }

    /// <summary>
    /// Разворачивает вложенные clauses comprehension в плоский поток фреймов.
    /// </summary>
    private IEnumerable<Frame> IterComprehension(List<CompClause> clauses, Frame outer)
    {
        if (clauses.Count == 0)
            yield break;

        foreach (var f in IterClause(clauses, 0, outer))
            yield return f;
    }

    private IEnumerable<Frame> IterClause(List<CompClause> clauses, int idx, Frame parent)
    {
        var clause = clauses[idx];
        var source = Eval(clause.Source, parent);
        IEnumerable<RuntimeValue> items = source switch
        {
            ArrayValue a => a.Items,
            SetValue s => s.Items,
            TupleValue t => t.Items,
            StringValue str => str.Value.Select(c => (RuntimeValue)new StringValue(c.ToString())),
            ObjectValue o => o.Fields
                .Where(kv => !kv.Key.StartsWith("__"))
                .Select(kv => (RuntimeValue)new StringValue(kv.Key)),
            _ => throw new YawaRuntimeException($"Cannot iterate {source.TypeName}")
        };

        var isLast = idx == clauses.Count - 1;

        foreach (var item in items)
        {
            var inner = new Frame($"<comp_{idx}>", parent);
            inner.Declare(clause.Var, item);

            if (clause.Filter is not null)
            {
                var cond = Eval(clause.Filter, inner);
                if (!AsBool(cond)) continue;
            }

            if (isLast)
                yield return inner;
            else
                foreach (var deeper in IterClause(clauses, idx + 1, inner))
                    yield return deeper;
        }
    }

    private RuntimeValue EvalTernary(TernaryExpr te, Frame frame)
    {
        var cond = Eval(te.Parts.Cond, frame);
        return AsBool(cond) ? Eval(te.Parts.Then, frame) : Eval(te.Parts.Else, frame);
    }

    // ───────── Helpers ─────────

    public static RuntimeValue FromJson(System.Text.Json.JsonElement el)
    {
        switch (el.ValueKind)
        {
            case System.Text.Json.JsonValueKind.Number:
                if (el.TryGetInt64(out var l)) return new IntValue(l);
                return new FloatValue(el.GetDouble());
            case System.Text.Json.JsonValueKind.String:
                return new StringValue(el.GetString() ?? "");
            case System.Text.Json.JsonValueKind.True: return BoolValue.True;
            case System.Text.Json.JsonValueKind.False: return BoolValue.False;
            case System.Text.Json.JsonValueKind.Null: return NullValue.Instance;
            case System.Text.Json.JsonValueKind.Array:
                {
                    var items = el.EnumerateArray().Select(FromJson).ToList();
                    return new ArrayValue(items);
                }
            default:
                throw new YawaRuntimeException($"Unsupported literal kind: {el.ValueKind}");
        }
    }

    public static long AsInt(RuntimeValue v, string what)
        => v is IntValue i ? i.Value
         : v is FloatValue f ? (long)f.Value
         : throw new YawaRuntimeException($"{what}: expected int, got {v.TypeName}");

    public static bool AsBool(RuntimeValue v)
        => v is BoolValue b ? b.Value
         : v is IntValue i ? i.Value != 0
         : v is NullValue ? false
         : throw new YawaRuntimeException($"Expected bool, got {v.TypeName}");

    private static BoolValue Bool(bool b) => b ? BoolValue.True : BoolValue.False;

    private RuntimeValue ArithmeticAdd(RuntimeValue a, RuntimeValue b)
    {
        if (a is StringValue sa && b is StringValue sb) return new StringValue(sa.Value + sb.Value);
        if (a is ArrayValue aa && b is ArrayValue ab) return new ArrayValue(aa.Items.Concat(ab.Items));
        return Arith(a, b, (x, y) => x + y, (x, y) => x + y);
    }

    private static RuntimeValue Multiply(RuntimeValue a, RuntimeValue b)
    {
        // list * int  /  int * list  → повторение списка
        if (a is ArrayValue arr && b is IntValue n)
            return RepeatArray(arr, n.Value);
        if (a is IntValue n2 && b is ArrayValue arr2)
            return RepeatArray(arr2, n2.Value);

        // string * int  /  int * string
        if (a is StringValue s && b is IntValue ni)
            return new StringValue(string.Concat(Enumerable.Repeat(s.Value, (int)Math.Max(0, ni.Value))));
        if (a is IntValue ni2 && b is StringValue s2)
            return new StringValue(string.Concat(Enumerable.Repeat(s2.Value, (int)Math.Max(0, ni2.Value))));

        return Arith(a, b, (x, y) => x * y, (x, y) => x * y);
    }

    private static ArrayValue RepeatArray(ArrayValue arr, long times)
    {
        if (times < 0) times = 0;
        var items = new List<RuntimeValue>(arr.Count * (int)times);
        for (long k = 0; k < times; k++)
            items.AddRange(arr.Items);
        return new ArrayValue(items, arr.ElementType);
    }

    private static RuntimeValue Arith(RuntimeValue a, RuntimeValue b,
        Func<long, long, long> intOp, Func<double, double, double> floatOp)
    {
        if (a is IntValue ai && b is IntValue bi) return new IntValue(intOp(ai.Value, bi.Value));
        if ((a is IntValue || a is FloatValue) && (b is IntValue || b is FloatValue))
        {
            var x = a is IntValue ii ? ii.Value : ((FloatValue)a).Value;
            var y = b is IntValue jj ? jj.Value : ((FloatValue)b).Value;
            return new FloatValue(floatOp(x, y));
        }
        throw new YawaRuntimeException($"Arithmetic on {a.TypeName} and {b.TypeName}");
    }
    private static RuntimeValue IntDiv(RuntimeValue a, RuntimeValue b)
    {
        // Целочисленное деление с округлением вниз (как Python)
        if (a is IntValue ai && b is IntValue bi)
        {
            if (bi.Value == 0) throw new YawaRuntimeException("Division by zero");
            long q = ai.Value / bi.Value;
            long r = ai.Value % bi.Value;
            // Python: -17 // 5 = -4 (округление вниз, не к нулю)
            if ((r != 0) && ((r < 0) != (bi.Value < 0))) q--;
            return new IntValue(q);
        }
        var x = a is IntValue ii ? ii.Value : a is FloatValue ff ? ff.Value : throw new YawaRuntimeException("// on non-number");
        var y = b is IntValue jj ? jj.Value : b is FloatValue gg ? gg.Value : throw new YawaRuntimeException("// on non-number");
        if (y == 0) throw new YawaRuntimeException("Division by zero");
        return new FloatValue(Math.Floor(x / y));
    }

    private static RuntimeValue Div(RuntimeValue a, RuntimeValue b)
    {
        var denom = b is IntValue bi ? bi.Value : b is FloatValue bf ? bf.Value : throw new YawaRuntimeException("div by non-number");
        if (denom == 0) throw new YawaRuntimeException("Division by zero");
        if (a is IntValue ai && b is IntValue)
            return new IntValue(ai.Value / (long)denom);
        var x = a is IntValue ii ? ii.Value : ((FloatValue)a).Value;
        return new FloatValue(x / denom);
    }

    private static RuntimeValue Mod(RuntimeValue a, RuntimeValue b)
    {
        var x = AsInt(a, "mod lhs");
        var y = AsInt(b, "mod rhs");
        if (y == 0) throw new YawaRuntimeException("Modulo by zero");

        // Python-семантика: результат имеет знак делителя.
        long r = x % y;
        if (r != 0 && ((r < 0) != (y < 0))) r += y;
        return new IntValue(r);
    }
    private static RuntimeValue Power(RuntimeValue a, RuntimeValue b)
    {
        var x = a is IntValue ii ? ii.Value : ((FloatValue)a).Value;
        var y = b is IntValue jj ? jj.Value : ((FloatValue)b).Value;
        return new FloatValue(Math.Pow(x, y));
    }

    private static RuntimeValue Compare(RuntimeValue a, RuntimeValue b, Func<double, double, bool> op)
    {
        var x = a is IntValue ii ? ii.Value : a is FloatValue ff ? ff.Value : throw new YawaRuntimeException("compare on non-number");
        var y = b is IntValue jj ? jj.Value : b is FloatValue gg ? gg.Value : throw new YawaRuntimeException("compare on non-number");
        return Bool(op(x, y));
    }

    private static RuntimeValue In(RuntimeValue needle, RuntimeValue haystack)
    {
        if (haystack is ArrayValue arr)
            return Bool(arr.Items.Any(v => v.ValueEquals(needle)));
        if (haystack is StringValue s && needle is StringValue ns)
            return Bool(s.Value.Contains(ns.Value));
        // if (haystack is ObjectValue obj && needle is StringValue key)
        //     return Bool(obj.HasField(key.Value));
        if (haystack is SetValue set)
            return Bool(set.Contains(needle));
        if (haystack is ObjectValue obj)
        {
            var key = needle is StringValue ss ? ss.Value : needle.ToString() ?? "";
            return Bool(obj.HasField(key));
        }
        throw new YawaRuntimeException($"'in' not supported for {haystack.TypeName}");
    }

    private RuntimeValue EvalSlice(SliceExpr sl, Frame frame)
    {
        var target = Eval(sl.Target, frame);

        // Поддерживаем только массивы (строки тоже полезно, но позже)
        if (target is not ArrayValue arr)
            throw new YawaRuntimeException($"Slice on non-array ({target.TypeName})");

        int n = arr.Count;
        int? start = sl.Start is null ? null : (int)AsInt(Eval(sl.Start, frame), "slice start");
        int? stop = sl.Stop is null ? null : (int)AsInt(Eval(sl.Stop, frame), "slice stop");
        int step = sl.Step is null ? 1 : (int)AsInt(Eval(sl.Step, frame), "slice step");

        if (step == 0)
            throw new YawaRuntimeException("Slice step cannot be 0");

        // Приводим к Python-семантике (как slice.indices)
        int lo, hi;
        if (step > 0)
        {
            lo = start ?? 0;
            hi = stop ?? n;
            if (lo < 0) lo += n;
            if (hi < 0) hi += n;
            lo = Math.Max(0, Math.Min(n, lo));
            hi = Math.Max(0, Math.Min(n, hi));
            if (hi <= lo) return new ArrayValue(Enumerable.Empty<RuntimeValue>(), arr.ElementType);
            var items = new List<RuntimeValue>();
            for (int i = lo; i < hi; i += step) items.Add(arr[i]);
            return new ArrayValue(items, arr.ElementType);
        }
        else
        {
            lo = start ?? (n - 1);
            hi = stop ?? -1;
            if (start is not null && lo < 0) lo += n;
            if (stop is not null && hi < 0) hi += n;
            lo = Math.Max(-1, Math.Min(n - 1, lo));
            var items2 = new List<RuntimeValue>();
            for (int i = lo; i > hi; i += step)
                if (i >= 0 && i < n) items2.Add(arr[i]);
            return new ArrayValue(items2, arr.ElementType);
        }
    }
}