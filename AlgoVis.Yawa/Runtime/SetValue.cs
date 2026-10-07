namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Множество. Внутри — список (не HashSet), потому что RuntimeValue
/// не имеет ValueEquals-хеша. Для алгоритмов размеры небольшие — ок.
/// </summary>
public sealed class SetValue : RuntimeValue
{
    public List<RuntimeValue> Items { get; } = new();
    public string? ElementType { get; init; }

    public override string TypeName =>
        ElementType is null ? "set" : $"set<{ElementType}>";

    public override object ToJson()
    {
        var arr = new object?[Items.Count];
        for (int i = 0; i < Items.Count; i++) arr[i] = Items[i].ToJson();
        return arr;
    }

    public override string ToString() =>
        "{" + string.Join(", ", Items.Select(x => x.ToString())) + "}";

    public override bool ValueEquals(RuntimeValue other)
    {
        if (other is not SetValue s) return false;
        if (s.Items.Count != Items.Count) return false;
        foreach (var item in Items)
            if (!s.Contains(item)) return false;
        return true;
    }

    public int Count => Items.Count;

    public bool Contains(RuntimeValue v) =>
        Items.Any(x => x.ValueEquals(v));

    public bool Add(RuntimeValue v)
    {
        if (Contains(v)) return false;
        Items.Add(v);
        return true;
    }

    public bool Remove(RuntimeValue v)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].ValueEquals(v))
            {
                Items.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
}
