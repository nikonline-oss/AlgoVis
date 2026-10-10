namespace AlgoVis.Parser.Model;

public sealed class ParameterNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.Parameter;
    public string Name { get; init; } = "";
    public string? Type { get; init; }
    public string? DefaultValue { get; init; }

    public override IEnumerable<AstNode> Children() => Array.Empty<AstNode>();
    public override string ToString() =>
        string.IsNullOrEmpty(Type) ? Name : $"{Name}: {Type}";
}