using AlgoVis.Parser.Parsing;

namespace AlgoVis.Parser.Cli.Renderers;

/// <summary>Печатает сырое CST от tree-sitter — для отладки.</summary>
public static class RawRenderer
{
    public static void Render(TsNode node, string source)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(source);
        PrintNode(node, bytes, 0);
    }

    private static void PrintNode(TsNode node, byte[] bytes, int depth)
    {
        var indent = new string(' ', depth * 2);
        var start = node.StartPoint;
        var end = node.EndPoint;

        var slice = "";
        if (node.ChildCount == 0)
        {
            var s = (int)node.StartByte;
            var e = (int)node.EndByte;
            if (s >= 0 && e > s && e <= bytes.Length)
            {
                var text = System.Text.Encoding.UTF8.GetString(bytes, s, e - s)
                    .Replace("\n", "\\n");
                slice = $" \"{text}\"";
            }
        }

        Console.WriteLine($"{indent}{node.Type} [{start.Row}:{start.Col}..{end.Row}:{end.Col}]{slice}");

        foreach (var child in node.Children())
            PrintNode(child, bytes, depth + 1);
    }
}