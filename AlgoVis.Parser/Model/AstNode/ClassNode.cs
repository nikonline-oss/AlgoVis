namespace AlgoVis.Parser.Model;

public sealed class ClassNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.Class;

    public string Name { get; init; } = "";
    public List<string> Annotations { get; init; } = new();
    public List<string> Bases { get; init; } = new();

    public List<FieldNode> Fields { get; } = new();
    public List<FunctionNode> Methods { get; } = new();
    public List<ClassNode> InnerClasses { get; } = new();

    public override IEnumerable<AstNode> Children()
    {
        foreach (var f in Fields) yield return f;
        foreach (var m in Methods) yield return m;
        foreach (var c in InnerClasses) yield return c;
    }

    public override string ToString()
    {
        var bases = Bases.Count == 0 ? "" : $"({string.Join(", ", Bases)})";
        return $"class {Name}{bases}";
    }
}