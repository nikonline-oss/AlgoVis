using AlgoVis.Parser.Cli.Renderers;
using AlgoVis.Parser.Mapping;
using AlgoVis.Parser.Parsing;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: algovis-parser parse <file.py> [--format=tree|stats|raw]");
    return 1;
}

if (args.Length >= 2 && args[0] == "debug-class")
{
    var src = File.ReadAllText(args[1]);
    using var tr = new PythonParser().Parse(src);
    var root = tr.Root;

    Console.WriteLine($"module named children = {root.NamedChildCount}");
    foreach (var top in root.NamedChildren())
    {
        Console.WriteLine($"  top: {top.Type}");

        if (top.Type == "class_definition")
        {
            Console.WriteLine($"    class named children = {top.NamedChildCount}");
            foreach (var c in top.NamedChildren())
            {
                Console.WriteLine($"      class child: {c.Type}");

                if (c.Type == "block")
                {
                    Console.WriteLine($"        block named children = {c.NamedChildCount}");
                    foreach (var bc in c.NamedChildren())
                    {
                        Console.WriteLine($"          block child: {bc.Type}");
                        if (bc.Type == "function_definition")
                        {
                            Console.WriteLine($"            func named children = {bc.NamedChildCount}");
                            foreach (var fc in bc.NamedChildren())
                                Console.WriteLine($"              func child: {fc.Type}");
                        }
                    }
                }
            }
        }
    }
    return 0;
}

var command = args[0];
if (command != "parse")
{
    Console.Error.WriteLine($"Unknown command: {command}");
    return 1;
}

var positional = new List<string>();
var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

for (int i = 1; i < args.Length; i++)
{
    var a = args[i];
    if (a.StartsWith("--"))
    {
        var key = a[2..];
        var eq = key.IndexOf('=');
        if (eq > 0) options[key[..eq]] = key[(eq + 1)..];
        else if (i + 1 < args.Length && !args[i + 1].StartsWith("--")) options[key] = args[++i];
        else options[key] = "true";
    }
    else positional.Add(a);
}

if (positional.Count == 0)
{
    Console.Error.WriteLine("Usage: algovis-parser parse <file.py> [--format=tree|stats|raw]");
    return 1;
}

var path = positional[0];
if (!File.Exists(path))
{
    Console.Error.WriteLine($"File not found: {path}");
    return 1;
}

var source = File.ReadAllText(path);
var format = options.TryGetValue("format", out var f) ? f : "tree";

var parser = new PythonParser();
using var tree = parser.Parse(source);

switch (format.ToLowerInvariant())
{
    case "raw":
        RawRenderer.Render(tree.Root, source);
        break;

    case "tree":
        {
            var model = AstMapper.ToModel(tree.Root, Path.GetFileName(path), source);
            Console.Write(TreeRenderer.Render(model));
            break;
        }

    case "stats":
        {
            var model = AstMapper.ToModel(tree.Root, Path.GetFileName(path), source);
            Console.Write(StatsRenderer.Render(model));
            break;
        }

    default:
        Console.Error.WriteLine($"Unknown format: {format}");
        return 1;
}

return 0;