namespace AlgoVis.Parser.Model;

public sealed class CallNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.Call;
    public string Name { get; init; } = "";
    public string? Receiver { get; init; }
    public List<string> Arguments { get; init; } = new();

    public override IEnumerable<AstNode> Children() => Array.Empty<AstNode>();
    public override string ToString()
    {
        var recv = string.IsNullOrEmpty(Receiver) ? "" : $"{Receiver}.";
        var args = string.Join(", ", Arguments);
        return $"{recv}{Name}({args})";
    }
}