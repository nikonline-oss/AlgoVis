namespace AlgoVis.Parser.Model;

public sealed class FunctionNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.Function;

    public string Name { get; init; } = "";
    public string? ReturnType { get; init; }
    public List<string> Annotations { get; init; } = new();
    public bool IsStatic { get; init; }

    public List<ParameterNode> Parameters { get; } = new();
    public List<LocalVariableNode> LocalVariables { get; } = new();
    public List<CallNode> Calls { get; } = new();
    public List<FunctionNode> InnerFunctions { get; } = new();

    public ClassNode? DeclaringClass => Parent as ClassNode;

    public override IEnumerable<AstNode> Children()
    {
        foreach (var p in Parameters) yield return p;
        foreach (var v in LocalVariables) yield return v;
        foreach (var c in Calls) yield return c;
        foreach (var f in InnerFunctions) yield return f;
    }

    public override string ToString()
    {
        var prms = string.Join(", ", Parameters.Select(p => p.ToString()));
        var ret = string.IsNullOrEmpty(ReturnType) ? "" : $" -> {ReturnType}";
        return $"def {Name}({prms}){ret}";
    }
}