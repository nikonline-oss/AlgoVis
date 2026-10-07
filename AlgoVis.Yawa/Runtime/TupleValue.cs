namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Кортеж — неизменяемая последовательность значений фиксированной длины.
/// </summary>
public sealed class TupleValue : RuntimeValue
{
    public List<RuntimeValue> Items { get; }

    public TupleValue(IEnumerable<RuntimeValue> items)
    {
        Items = items.ToList();
    }

    public override string TypeName => $"tuple({Items.Count})";

    public override object ToJson()
    {
        var arr = new object?[Items.Count];
        for (int i = 0; i < Items.Count; i++) arr[i] = Items[i].ToJson();
        return arr;
    }

    public override string ToString() =>
        "(" + string.Join(", ", Items.Select(x => x.ToString())) + ")";

    public override bool ValueEquals(RuntimeValue other)
    {
        if (other is not TupleValue t) return false;
        if (t.Items.Count != Items.Count) return false;
        for (int i = 0; i < Items.Count; i++)
            if (!Items[i].ValueEquals(t.Items[i])) return false;
        return true;
    }

    public int Count => Items.Count;
    public RuntimeValue this[int i] => Items[i];
}
