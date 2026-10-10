namespace AlgoVis.Yawa.Runtime;

public sealed class GraphValue : RuntimeValue
{
    /// <summary>Узлы: id → метка (значение).</summary>
    public Dictionary<string, RuntimeValue> Nodes { get; } = new();

    /// <summary>Рёбра: (from, to, weight?). Неориентированный граф — добавляем оба направления.</summary>
    public List<GraphEdge> Edges { get; } = new();

    public bool Directed { get; init; }

    public override string TypeName => Directed ? "digraph" : "graph";

    public override object ToJson()
    {
        var nodes = new Dictionary<string, object?>();
        foreach (var (k, v) in Nodes) nodes[k] = v.ToJson();

        var edges = new List<object?>();
        foreach (var e in Edges)
            edges.Add(new Dictionary<string, object?>
            {
                ["from"] = e.From,
                ["to"] = e.To,
                ["weight"] = e.Weight
            });

        return new Dictionary<string, object?>
        {
            ["directed"] = Directed,
            ["nodes"] = nodes,
            ["edges"] = edges
        };
    }

    public override string ToString() =>
        $"graph({Nodes.Count} nodes, {Edges.Count} edges)";

    public override bool ValueEquals(RuntimeValue other) => ReferenceEquals(this, other);
}

public sealed class GraphEdge
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public double? Weight { get; set; }
}