namespace AlgoVis.Parser.Model;

public sealed class SyntaxTree : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.SyntaxTree;

    public string FileName { get; init; } = "";
    public string SourceCode { get; init; } = "";

    public List<ImportNode> Imports { get; } = new();
    public List<ClassNode> Classes { get; } = new();
    public List<FunctionNode> TopLevelFunctions { get; } = new();
    public List<FieldNode> GlobalVariables { get; } = new();

    public override IEnumerable<AstNode> Children()
    {
        foreach (var i in Imports) yield return i;
        foreach (var c in Classes) yield return c;
        foreach (var f in TopLevelFunctions) yield return f;
        foreach (var g in GlobalVariables) yield return g;
    }

    public IEnumerable<FunctionNode> AllFunctions =>
        Descendants().OfType<FunctionNode>();
    public IEnumerable<CallNode> AllCalls =>
        Descendants().OfType<CallNode>();
    public IEnumerable<ClassNode> AllClasses =>
        Descendants().OfType<ClassNode>();

    public IEnumerable<FunctionNode> FindFunctions(string name) =>
        AllFunctions.Where(f => f.Name == name);
    public ClassNode? FindClass(string name) =>
        AllClasses.FirstOrDefault(c => c.Name == name);

    public override string ToString() => $"SyntaxTree({FileName})";
}