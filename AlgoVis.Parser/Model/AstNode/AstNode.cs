namespace AlgoVis.Parser.Model;

public enum AstNodeKind
{
    SyntaxTree, Import, Class, Function, Parameter, Field, LocalVariable, Call
}

public abstract class AstNode
{
    public AstNode? Parent { get; internal set; }
    public CodeSpan Span { get; internal set; } = CodeSpan.Empty;
    public abstract AstNodeKind Kind { get; }

    public IEnumerable<AstNode> Descendants()
    {
        yield return this;
        foreach (var child in Children())
            foreach (var d in child.Descendants())
                yield return d;
    }

    public abstract IEnumerable<AstNode> Children();

    public IEnumerable<AstNode> Ancestors()
    {
        var p = Parent;
        while (p != null) { yield return p; p = p.Parent; }
    }
}