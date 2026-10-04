using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlgoVis.Yawa.Yawa.Expressions;

public sealed class LiteralExpr : YawaExpression
{
    /// <summary>Сырое значение — int, double, string, bool, null или JsonElement-массив.</summary>
    [JsonPropertyName("lit")]
    public JsonElement Value { get; set; }
}

public sealed class RefExpr : YawaExpression
{
    [JsonPropertyName("ref")]
    public string Name { get; set; } = "";
}

public sealed class IndexExpr : YawaExpression
{
    /// <summary>A[i] — Target и Index, парсятся вручную в конвертере.</summary>
    public YawaExpression Target { get; set; } = null!;
    public YawaExpression Index { get; set; } = null!;
}

public sealed class FieldExpr : YawaExpression
{
    /// <summary>obj.field — Target и имя поля, парсятся вручную в конвертере.</summary>
    public YawaExpression Target { get; set; } = null!;
    public string FieldName { get; set; } = "";
}

public sealed class BinaryExpr : YawaExpression
{
    [JsonPropertyName("bin")] public string Op { get; set; } = "";
    [JsonPropertyName("a")] public YawaExpression A { get; set; } = null!;
    [JsonPropertyName("b")] public YawaExpression B { get; set; } = null!;
}

public sealed class UnaryExpr : YawaExpression
{
    [JsonPropertyName("un")] public string Op { get; set; } = "";
    [JsonPropertyName("a")] public YawaExpression A { get; set; } = null!;
}

public sealed class CallExpr : YawaExpression
{
    [JsonPropertyName("call")] public string Name { get; set; } = "";
    [JsonPropertyName("args")] public List<YawaExpression> Args { get; set; } = new();
}

public sealed class CallMethodExpr : YawaExpression
{
    [JsonPropertyName("call_method")] public YawaExpression Receiver { get; set; } = null!;
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("args")] public List<YawaExpression> Args { get; set; } = new();
}

public sealed class NewObjectExpr : YawaExpression
{
    [JsonPropertyName("new_object")]
    public Dictionary<string, YawaExpression> Fields { get; set; } = new();
}

public sealed class LenExpr : YawaExpression
{
    [JsonPropertyName("len")] public YawaExpression Target { get; set; } = null!;
}

public sealed class TernaryExpr : YawaExpression
{
    [JsonPropertyName("ternary")]
    public TernaryParts Parts { get; set; } = new();
}

public sealed class TernaryParts
{
    [JsonPropertyName("cond")] public YawaExpression Cond { get; set; } = null!;
    [JsonPropertyName("then")] public YawaExpression Then { get; set; } = null!;
    [JsonPropertyName("else")] public YawaExpression Else { get; set; } = null!;
}
public sealed class ArrayExpr : YawaExpression
{
    /// <summary>Список элементов массива. Каждый — подвыражение.</summary>
    [JsonPropertyName("array")]
    public List<YawaExpression> Items { get; set; } = new();
}

public sealed class SliceExpr : YawaExpression
{
    /// <summary>A[start:stop:step] — любой из компонентов опционален.</summary>
    public YawaExpression Target { get; set; } = null!;
    public YawaExpression? Start { get; set; }
    public YawaExpression? Stop { get; set; }
    public YawaExpression? Step { get; set; }
}

public sealed class DictEntry
{
    public YawaExpression Key { get; set; } = null!;
    public YawaExpression Value { get; set; } = null!;
}

public sealed class DictExpr : YawaExpression
{
    /// <summary>Список пар ключ-значение.</summary>
    public List<DictEntry> Items { get; set; } = new();
}
public sealed class InstantiateExpr : YawaExpression
{
    [JsonPropertyName("instantiate")]
    public string ClassName { get; set; } = "";

    [JsonPropertyName("args")]
    public List<YawaExpression> Args { get; set; } = new();
}