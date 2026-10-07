using System.Text.Json.Serialization;
using AlgoVis.Yawa.Yawa.Statements;
using AlgoVis.Yawa.Yawa.Expressions;

namespace AlgoVis.Yawa.Yawa;

public sealed class YawaProgram
{
    [JsonPropertyName("yawa_version")]
    public string YawaVersion { get; set; } = "1.0";

    [JsonPropertyName("metadata")]
    public YawaMetadata Metadata { get; set; } = new();

    [JsonPropertyName("globals")]
    public Dictionary<string, YawaGlobal> Globals { get; set; } = new();

    [JsonPropertyName("functions")]
    public List<YawaFunction> Functions { get; set; } = new();

    [JsonPropertyName("entry")]
    public YawaEntry Entry { get; set; } = new();

    [JsonPropertyName("limits")]
    public YawaLimits Limits { get; set; } = new();
}

public sealed class YawaMetadata
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("generator")] public string? Generator { get; set; }
}

public sealed class YawaGlobal
{
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("value")] public YawaExpression? Value { get; set; }
}

public sealed class YawaFunction
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("params")] public List<YawaParam> Params { get; set; } = new();
    [JsonPropertyName("returns")] public string? Returns { get; set; }
    [JsonPropertyName("body")] public List<YawaStatement> Body { get; set; } = new();
}

public sealed class YawaParam
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }
    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public YawaExpression? DefaultValue { get; set; }
}

public sealed class YawaEntry
{
    [JsonPropertyName("function")] public string Function { get; set; } = "";
    [JsonPropertyName("args")] public List<YawaExpression> Args { get; set; } = new();
}

public sealed class YawaLimits
{
    [JsonPropertyName("max_steps")] public int MaxSteps { get; set; } = 100_000;
    [JsonPropertyName("max_depth")] public int MaxDepth { get; set; } = 1_000;
    [JsonPropertyName("max_seconds")] public double MaxSeconds { get; set; } = 10.0;
    [JsonPropertyName("snapshot_every")] public int SnapshotEvery { get; set; } = 0;
}