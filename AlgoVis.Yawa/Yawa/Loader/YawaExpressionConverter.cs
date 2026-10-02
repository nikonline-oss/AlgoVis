using System.Text.Json;
using System.Text.Json.Serialization;
using AlgoVis.Yawa.Yawa.Expressions;

namespace AlgoVis.Yawa.Yawa.Loader;

public sealed class YawaExpressionConverter : JsonConverter<YawaExpression>
{
    public override YawaExpression? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ParseElement(doc.RootElement);
    }

    public override void Write(Utf8JsonWriter writer, YawaExpression value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Serialization of YawaExpression is not supported.");
    }

    internal static YawaExpression ParseElement(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object)
            throw new JsonException($"YawaExpression must be object, got {el.ValueKind}");

        // Определяем тип по первому совпадающему свойству.
        if (el.TryGetProperty("lit", out var lit))
            return new LiteralExpr { Value = lit.Clone(), NodeId = ReadId(el) };

        if (el.TryGetProperty("ref", out var refProp))
            return new RefExpr { Name = refProp.GetString() ?? "", NodeId = ReadId(el) };

        if (el.TryGetProperty("index", out var idxProp))
        {
            if (idxProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("'index' must be array");
            var arr = idxProp.EnumerateArray().ToArray();
            if (arr.Length != 2)
                throw new JsonException("'index' must have exactly 2 elements");
            return new IndexExpr
            {
                Target = ParsePart(arr[0]),
                Index = ParsePart(arr[1]),
                NodeId = ReadId(el)
            };
        }
        if (el.TryGetProperty("field", out var fldProp))
        {
            if (fldProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("'field' must be an array of 2 elements");
            var arr = fldProp.EnumerateArray().ToArray();
            if (arr.Length != 2)
                throw new JsonException("'field' must have exactly 2 elements");

            if (arr[1].ValueKind != JsonValueKind.String)
                throw new JsonException("second element of 'field' must be a string (field name)");

            return new FieldExpr
            {
                Target = ParsePart(arr[0]),
                FieldName = arr[1].GetString() ?? "",
                NodeId = ReadId(el)
            };
        }

        if (el.TryGetProperty("bin", out var binProp))
            return new BinaryExpr
            {
                Op = binProp.GetString() ?? "",
                A = ParseRequired(el, "a"),
                B = ParseRequired(el, "b"),
                NodeId = ReadId(el)
            };

        if (el.TryGetProperty("un", out var unProp))
            return new UnaryExpr
            {
                Op = unProp.GetString() ?? "",
                A = ParseRequired(el, "a"),
                NodeId = ReadId(el)
            };

        if (el.TryGetProperty("call", out var callProp))
            return new CallExpr
            {
                Name = callProp.GetString() ?? "",
                Args = ParseArgs(el, "args"),
                NodeId = ReadId(el)
            };

        if (el.TryGetProperty("call_method", out var cmProp))
            return new CallMethodExpr
            {
                Receiver = ParseElement(cmProp.Clone()),
                Name = el.TryGetProperty("name", out var n) ? (n.GetString() ?? "") : "",
                Args = ParseArgs(el, "args"),
                NodeId = ReadId(el)
            };

        if (el.TryGetProperty("new_object", out var noProp))
        {
            var fields = new Dictionary<string, YawaExpression>();
            foreach (var prop in noProp.EnumerateObject())
                fields[prop.Name] = ParseObjectFieldValue(prop.Value);
            return new NewObjectExpr { Fields = fields, NodeId = ReadId(el) };
        }

        if (el.TryGetProperty("len", out var lenProp))
            return new LenExpr { Target = ParseElement(lenProp.Clone()), NodeId = ReadId(el) };

        if (el.TryGetProperty("ternary", out var ternProp))
            return new TernaryExpr
            {
                Parts = new TernaryParts
                {
                    Cond = ParseElement(ternProp.GetProperty("cond").Clone()),
                    Then = ParseElement(ternProp.GetProperty("then").Clone()),
                    Else = ParseElement(ternProp.GetProperty("else").Clone())
                },
                NodeId = ReadId(el)
            };

        throw new JsonException($"Unknown expression shape: {el.GetRawText()}");
    }

    private static YawaExpression ParseRequired(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var prop))
            throw new JsonException($"Missing required field '{key}'");
        return ParseElement(prop.Clone());
    }

    private static List<YawaExpression> ParseArgs(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var prop)) return new();
        if (prop.ValueKind != JsonValueKind.Array)
            throw new JsonException($"'{key}' must be array");
        return prop.EnumerateArray()
            .Select(x => ParseElement(x.Clone()))
            .ToList();
    }

    /// <summary>
    /// Парсит "часть" index/field: либо объект-выражение, либо строку-shorthand (→ RefExpr).
    /// Например: {"ref":"i"} — объект, "A" — shorthand для {"ref":"A"}.
    /// </summary>
    private static YawaExpression ParsePart(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.String)
            return new RefExpr { Name = el.GetString() ?? "" };
        return ParseElement(el);
    }

    /// <summary>
    /// Значение поля new_object. В отличие от ParsePart, строка здесь — литерал,
    /// а не ссылка на переменную (например, "__type__": "TreeNode").
    /// </summary>
    private static YawaExpression ParseObjectFieldValue(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
            case JsonValueKind.Array:
                return new LiteralExpr { Value = el.Clone() };
            case JsonValueKind.Object:
                return ParseElement(el);
            default:
                throw new JsonException($"Unsupported new_object field value: {el.ValueKind}");
        }
    }

    private static string? ReadId(JsonElement el)
        => el.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
            ? id.GetString()
            : null;
}