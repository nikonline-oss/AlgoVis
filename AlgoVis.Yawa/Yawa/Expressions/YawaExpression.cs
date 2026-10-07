using System.Text.Json.Serialization;
using AlgoVis.Yawa.Yawa.Loader;

namespace AlgoVis.Yawa.Yawa.Expressions;

[JsonConverter(typeof(YawaExpressionConverter))]
public abstract class YawaExpression
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NodeId { get; set; }
}