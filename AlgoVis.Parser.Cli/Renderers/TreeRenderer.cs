using System.Text;
using AlgoVis.Parser.Model;

namespace AlgoVis.Parser.Cli.Renderers;

public static class TreeRenderer
{
    public static string Render(SyntaxTree tree)
    {
        var sb = new StringBuilder();
        sb.AppendLine(tree.ToString());

        var rootChildren = new List<AstNode>();
        rootChildren.AddRange(tree.Imports);
        rootChildren.AddRange(tree.Classes);
        rootChildren.AddRange(tree.TopLevelFunctions);
        rootChildren.AddRange(tree.GlobalVariables);

        for (int i = 0; i < rootChildren.Count; i++)
            RenderNode(rootChildren[i], sb, "", i == rootChildren.Count - 1);

        return sb.ToString();
    }

    private static void RenderNode(AstNode node, StringBuilder sb, string prefix, bool isLast)
    {
        sb.Append(prefix);
        sb.Append(isLast ? "└─ " : "├─ ");
        sb.Append(node.ToString());

        if (node.Span != CodeSpan.Empty)
            sb.Append($"  [{node.Span}]");

        sb.AppendLine();

        var children = node.Children().ToList();
        if (children.Count == 0) return;

        var childPrefix = prefix + (isLast ? "   " : "│  ");
        for (int i = 0; i < children.Count; i++)
            RenderNode(children[i], sb, childPrefix, i == children.Count - 1);
    }
}