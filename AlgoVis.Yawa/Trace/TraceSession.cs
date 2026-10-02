using System.Text.Json.Serialization;

namespace AlgoVis.Yawa.Trace;

public sealed class TraceSession
{
    [JsonPropertyName("yawa_version")] public string YawaVersion { get; set; } = "1.0";
    [JsonPropertyName("session_id")]   public string SessionId   { get; set; } = Guid.NewGuid().ToString("N");
    [JsonPropertyName("metadata")]     public Dictionary<string, object?> Metadata { get; set; } = new();
    [JsonPropertyName("structure")]    public StructureDescriptor Structure { get; set; } = new();
    [JsonPropertyName("steps")]        public List<TraceStep> Steps { get; set; } = new();
    [JsonPropertyName("final_state")]  public Dictionary<string, object?> FinalState { get; set; } = new();
    [JsonPropertyName("statistics")]   public TraceStatistics Statistics { get; set; } = new();
}

public sealed class StructureDescriptor
{
    [JsonPropertyName("variables")]
    public List<VariableDescriptor> Variables { get; set; } = new();
}

public sealed class VariableDescriptor
{
    [JsonPropertyName("name")]  public string Name { get; set; } = "";
    [JsonPropertyName("type")]  public string? Type { get; set; }
    [JsonPropertyName("visual")] public string? VisualKind { get; set; }   // array, tree, graph, object, ...
}

public sealed class TraceStep
{
    [JsonPropertyName("n")]          public int N { get; set; }
    [JsonPropertyName("kind")]       public string Kind { get; set; } = "";
    [JsonPropertyName("node_id")]    public string? NodeId { get; set; }
    [JsonPropertyName("diff")]       public List<StateChange> Diff { get; set; } = new();
    [JsonPropertyName("highlight")]  public List<string> Highlight { get; set; } = new();
    [JsonPropertyName("annotation")] public string? Annotation { get; set; }
    [JsonPropertyName("stats")]      public TraceStepStats? Stats { get; set; }
    [JsonPropertyName("snapshot")]   public Dictionary<string, object?>? Snapshot { get; set; }
}

public sealed class StateChange
{
    [JsonPropertyName("target")] public string Target { get; set; } = "";
    [JsonPropertyName("old")]    public object? Old { get; set; }
    [JsonPropertyName("new")]    public object? New { get; set; }
}

public sealed class TraceStepStats
{
    [JsonPropertyName("comparisons")]     public int Comparisons { get; set; }
    [JsonPropertyName("swaps")]           public int Swaps { get; set; }
    [JsonPropertyName("memory_accesses")] public int MemoryAccesses { get; set; }
    [JsonPropertyName("steps_total")]     public int StepsTotal { get; set; }
    [JsonPropertyName("user_counters")]   public Dictionary<string, long> UserCounters { get; set; } = new();
}

public sealed class TraceStatistics
{
    [JsonPropertyName("total_steps")]      public int TotalSteps { get; set; }
    [JsonPropertyName("comparisons")]      public int Comparisons { get; set; }
    [JsonPropertyName("swaps")]            public int Swaps { get; set; }
    [JsonPropertyName("memory_accesses")]  public int MemoryAccesses { get; set; }
    [JsonPropertyName("user_counters")]    public Dictionary<string, long> UserCounters { get; set; } = new();
    [JsonPropertyName("structure_sizes")]  public Dictionary<string, int> StructureSizes { get; set; } = new();
    [JsonPropertyName("big_o_hint")]       public string? BigOHint { get; set; }
}