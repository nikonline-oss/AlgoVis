namespace AlgoVis.Parser.Model;

public sealed class ImportNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.Import;
    public string Source { get; init; } = "";
    public string? Alias { get; init; }
    public List<string> ImportedNames { get; init; } = new();
    public bool IsFromImport { get; init; }

    public override IEnumerable<AstNode> Children() => Array.Empty<AstNode>();
    public override string ToString()
    {
        if (IsFromImport)
            return ImportedNames.Count > 0
                ? $"from {Source} import {string.Join(", ", ImportedNames)}"
                : $"from {Source} import *";
        return Alias is null ? $"import {Source}" : $"import {Source} as {Alias}";
    }
}