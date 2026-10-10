using AlgoVis.Parser.Native;

namespace AlgoVis.Parser.Parsing;

/// <summary>
/// Парсер C++ через tree-sitter.
/// </summary>
public sealed class CppParser
{
    static CppParser()
    {
        LibLoader.Register();
    }

    public TsTree Parse(string source)
    {
        using var p = new TsParser(CppLanguage.tree_sitter_cpp());
        return p.Parse(source);
    }

    public TsNode ParseRoot(string source)
    {
        using var tree = Parse(source);
        return tree.Root;
    }
}
