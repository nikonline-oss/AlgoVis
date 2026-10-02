namespace AlgoVis.Parser.Parsing;

public sealed class PythonParser
{
    /// <summary>
    /// Парсит Python-код и возвращает сырое дерево tree-sitter.
    /// </summary>
    public TsTree Parse(string source)
    {
        using var p = new TsParser();
        return p.Parse(source);
    }

    /// <summary>
    /// Парсит и сразу возвращает корневой узел.
    /// ВНИМАНИЕ: TsTree внутри уничтожается, использовать только пока сами ноды нужны.
    /// Для отладки удобно, для серьёзной работы — используйте Parse и храните TsTree.
    /// </summary>
    public TsNode ParseRoot(string source)
    {
        using var tree = Parse(source);
        return tree.Root;
    }
}