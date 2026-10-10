namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Массив YAWA. Внутри — список значений, плюс метаданные для визуализации
/// (highlight / labels) — они НЕ участвуют в diff-ах, только в отображении.
/// </summary>
public sealed class ArrayValue : RuntimeValue
{
    public List<RuntimeValue> Items { get; }
    public string? ElementType { get; }

    /// <summary>index → цвет подсветки (например, "red", "#ff0000").</summary>
    public Dictionary<int, string> Highlights { get; } = new();

    /// <summary>index → текстовая метка (например, "min", "max").</summary>
    public Dictionary<int, string> Labels { get; } = new();

    public ArrayValue(IEnumerable<RuntimeValue> items, string? elementType = null)
    {
        Items = items.ToList();
        ElementType = elementType;
    }

    public override string TypeName =>
        ElementType is null ? "array" : $"array<{ElementType}>";

    public override object? ToJson()
    {
        var arr = new object?[Items.Count];
        for (int i = 0; i < Items.Count; i++)
            arr[i] = Items[i].ToJson();
        return arr;
    }

    public override string ToString() =>
        "[" + string.Join(", ", Items.Select(v => v.ToString())) + "]";

    public override bool ValueEquals(RuntimeValue other)
    {
        if (other is not ArrayValue a) return false;
        if (a.Items.Count != Items.Count) return false;
        for (int i = 0; i < Items.Count; i++)
            if (!Items[i].ValueEquals(a.Items[i])) return false;
        return true;
    }

    public int Count => Items.Count;
    public RuntimeValue this[int i]
    {
        get => Items[i];
        set => Items[i] = value;
    }
}