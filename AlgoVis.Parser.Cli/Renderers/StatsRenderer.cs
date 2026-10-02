using System.Text;
using AlgoVis.Parser.Model;

namespace AlgoVis.Parser.Cli.Renderers;

public static class StatsRenderer
{
    public static string Render(SyntaxTree tree)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"File: {tree.FileName}");

        var classes = tree.AllClasses.ToList();
        var functions = tree.AllFunctions.ToList();
        var calls = tree.AllCalls.ToList();

        sb.AppendLine($"Classes:              {classes.Count}");
        sb.AppendLine($"  Top-level:          {tree.Classes.Count}");
        sb.AppendLine($"  Nested:             {classes.Count - tree.Classes.Count}");
        sb.AppendLine($"Functions:            {functions.Count}");
        sb.AppendLine($"  Top-level:          {tree.TopLevelFunctions.Count}");
        sb.AppendLine($"  Methods:            {functions.Count - tree.TopLevelFunctions.Count - functions.Sum(f => f.InnerFunctions.Count)}");
        sb.AppendLine($"  Nested:             {functions.Sum(f => f.InnerFunctions.Count)}");
        sb.AppendLine($"Calls (all):          {calls.Count}");
        sb.AppendLine($"Imports:              {tree.Imports.Count}");
        sb.AppendLine($"Fields:               {tree.AllClasses.Sum(c => c.Fields.Count)}");
        sb.AppendLine($"Global variables:     {tree.GlobalVariables.Count}");
        sb.AppendLine($"Local variables:      {functions.Sum(f => f.LocalVariables.Count)}");

        if (calls.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Top called functions:");
            var top = calls.GroupBy(c => c.Name)
                .OrderByDescending(g => g.Count())
                .Take(10);
            foreach (var g in top)
                sb.AppendLine($"  {g.Key,-30} × {g.Count()}");
        }

        return sb.ToString();
    }
}