namespace AlgoVis.Parser.Model;

public sealed class FieldNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.Field;
    public string Name { get; init; } = "";
    public string? Type { get; init; }
    public string? Value { get; init; }
    public List<string> Modifiers { get; init; } = new();

    public override IEnumerable<AstNode> Children() => Array.Empty<AstNode>();
    public override string ToString()
    {
        var type = string.IsNullOrEmpty(Type) ? "" : $": {Type}";
        var val  = string.IsNullOrEmpty(Value) ? "" : $" = {Value}";
        return $"{Name}{type}{val}";
    }
}