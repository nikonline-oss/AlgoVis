namespace AlgoVis.Yawa.Runtime;

public sealed class GraphValue : RuntimeValue
{
    /// <summary>Узлы: id → метка (значение).</summary>
    public Dictionary<string, RuntimeValue> Nodes { get; } = new();

    /// <summary>Рёбра: (from, to, weight?). Неориентированный граф — добавляем оба направления.</summary>
    public List<GraphEdge> Edges { get; } = new();

    public bool Directed { get; init; }

    public override string TypeName => Directed ? "digraph" : "graph";

    public override object ToJson() => new Dictionary<string, object?>
    {
        ["directed"] = Directed,
        ["nodes"] = Nodes.ToDictionary(kv => kv.Key, kv => kv.Value.ToJson()),
        ["edges"] = Edges.Select(e => new Dictionary<string, object?>
        {
            ["from"] = e.From,
            ["to"]   = e.To,
            ["weight"] = e.Weight
        }).ToList()
    };

    public override string ToString() =>
        $"graph({Nodes.Count} nodes, {Edges.Count} edges)";

    public override bool ValueEquals(RuntimeValue other) => ReferenceEquals(this, other);
}

public sealed class GraphEdge
{
    public string From { get; set; } = "";
    public string To   { get; set; } = "";
    public double? Weight { get; set; }
}