namespace AlgoVis.Yawa.Runtime;

public sealed class TreeValue : RuntimeValue
{
    public TreeNodeValue? Root { get; set; }

    public override string TypeName => "tree";
    public override object? ToJson() => Root?.ToJson();

    public override string ToString() =>
        Root is null ? "tree()" : $"tree({Root})";

    public override bool ValueEquals(RuntimeValue other) => ReferenceEquals(this, other);
}

public sealed class TreeNodeValue : RuntimeValue
{
    public RuntimeValue Value { get; set; } = NullValue.Instance;
    public TreeNodeValue? Left { get; set; }
    public TreeNodeValue? Right { get; set; }
    public string? HighlightColor { get; set; }

    public override string TypeName => "tree_node";

    public override object ToJson()
    {
        return new Dictionary<string, object?>
        {
            ["value"]  = Value.ToJson(),
            ["left"]   = Left?.ToJson(),
            ["right"]  = Right?.ToJson(),
            ["color"]  = HighlightColor
        };
    }

    public override string ToString() =>
        $"({Value}, L={Left?.ToString() ?? "-"}, R={Right?.ToString() ?? "-"})";

    public override bool ValueEquals(RuntimeValue other) => ReferenceEquals(this, other);
}