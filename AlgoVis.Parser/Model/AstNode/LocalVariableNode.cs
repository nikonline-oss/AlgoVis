namespace AlgoVis.Parser.Model;

public sealed class LocalVariableNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.LocalVariable;
    public string Name { get; init; } = "";
    public string? Type { get; init; }
    public string? Value { get; init; }

    public override IEnumerable<AstNode> Children() => Array.Empty<AstNode>();
    public override string ToString()
    {
        var type = string.IsNullOrEmpty(Type) ? "" : $": {Type}";
        var val  = string.IsNullOrEmpty(Value) ? "" : $" = {Value}";
        return $"{Name}{type}{val}";
    }
}