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
        writer.WriteStartObject();

        if (!string.IsNullOrEmpty(value.NodeId))
            writer.WriteString("id", value.NodeId);

        switch (value)
        {
            case LiteralExpr lit:
                writer.WritePropertyName("lit");
                lit.Value.WriteTo(writer);
                break;

            case RefExpr r:
                writer.WriteString("ref", r.Name);
                break;

            case IndexExpr ix:
                writer.WritePropertyName("index");
                writer.WriteStartArray();
                WriteExpr(writer, ix.Target, options);
                WriteExpr(writer, ix.Index, options);
                writer.WriteEndArray();
                break;

            case FieldExpr fl:
                writer.WritePropertyName("field");
                writer.WriteStartArray();
                WriteExpr(writer, fl.Target, options);
                writer.WriteStringValue(fl.FieldName);
                writer.WriteEndArray();
                break;

            case BinaryExpr bin:
                writer.WriteString("bin", bin.Op);
                writer.WritePropertyName("a");
                WriteExpr(writer, bin.A, options);
                writer.WritePropertyName("b");
                WriteExpr(writer, bin.B, options);
                break;

            case UnaryExpr un:
                writer.WriteString("un", un.Op);
                writer.WritePropertyName("a");
                WriteExpr(writer, un.A, options);
                break;

            case CallExpr c:
                writer.WriteString("call", c.Name);
                writer.WritePropertyName("args");
                writer.WriteStartArray();
                foreach (var a in c.Args) WriteExpr(writer, a, options);
                writer.WriteEndArray();
                break;

            case CallMethodExpr cm:
                writer.WritePropertyName("call_method");
                WriteExpr(writer, cm.Receiver, options);
                writer.WriteString("name", cm.Name);
                writer.WritePropertyName("args");
                writer.WriteStartArray();
                foreach (var a in cm.Args) WriteExpr(writer, a, options);
                writer.WriteEndArray();
                break;

            case NewObjectExpr no:
                writer.WritePropertyName("new_object");
                writer.WriteStartObject();
                foreach (var (k, v) in no.Fields)
                {
                    writer.WritePropertyName(k);
                    WriteExpr(writer, v, options);
                }
                writer.WriteEndObject();
                break;

            case LenExpr le:
                writer.WritePropertyName("len");
                WriteExpr(writer, le.Target, options);
                break;

            case TernaryExpr te:
                writer.WritePropertyName("ternary");
                writer.WriteStartObject();
                writer.WritePropertyName("cond");
                WriteExpr(writer, te.Parts.Cond, options);
                writer.WritePropertyName("then");
                WriteExpr(writer, te.Parts.Then, options);
                writer.WritePropertyName("else");
                WriteExpr(writer, te.Parts.Else, options);
                writer.WriteEndObject();
                break;

            case ArrayExpr arr:
                writer.WritePropertyName("array");
                writer.WriteStartArray();
                foreach (var x in arr.Items)
                    WriteExpr(writer, x, options);
                writer.WriteEndArray();
                break;

            case SliceExpr sl:
                writer.WritePropertyName("slice");
                writer.WriteStartArray();
                WriteExpr(writer, sl.Target, options);
                if (sl.Start is null) writer.WriteNullValue();
                else WriteExpr(writer, sl.Start, options);
                if (sl.Stop is null) writer.WriteNullValue();
                else WriteExpr(writer, sl.Stop, options);
                if (sl.Step is null) writer.WriteNullValue();
                else WriteExpr(writer, sl.Step, options);
                writer.WriteEndArray();
                break;

            case DictExpr d:
                writer.WritePropertyName("dict");
                writer.WriteStartArray();
                foreach (var item in d.Items)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("k");
                    WriteExpr(writer, item.Key, options);
                    writer.WritePropertyName("v");
                    WriteExpr(writer, item.Value, options);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                break;

            case InstantiateExpr inst:
                writer.WriteString("instantiate", inst.ClassName);
                writer.WritePropertyName("args");
                writer.WriteStartArray();
                foreach (var a in inst.Args) WriteExpr(writer, a, options);
                writer.WriteEndArray();
                break;

            case SetLiteralExpr sl:
                writer.WritePropertyName("set");
                writer.WriteStartArray();
                foreach (var x in sl.Items)
                    WriteExpr(writer, x, options);
                writer.WriteEndArray();
                break;

            case TupleExpr tup:
                writer.WritePropertyName("tuple");
                writer.WriteStartArray();
                foreach (var x in tup.Items)
                    WriteExpr(writer, x, options);
                writer.WriteEndArray();
                break;

            case ListCompExpr lc:
                writer.WritePropertyName("comp_body");
                WriteExpr(writer, lc.Body, options);
                writer.WriteString("comp_var", lc.Var);
                writer.WritePropertyName("comp_source");
                WriteExpr(writer, lc.Source, options);
                if (lc.Filter is not null)
                {
                    writer.WritePropertyName("comp_filter");
                    WriteExpr(writer, lc.Filter, options);
                }
                break;

            default:
                throw new NotSupportedException($"Write not implemented for {value.GetType().Name}");
        }

        writer.WriteEndObject();
    }

    private static void WriteExpr(Utf8JsonWriter writer, YawaExpression expr, JsonSerializerOptions options)
    {
        var converter = new YawaExpressionConverter();
        converter.Write(writer, expr, options);
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

        if (el.TryGetProperty("instantiate", out var instProp))
        {
            if (instProp.ValueKind != JsonValueKind.String)
                throw new JsonException("'instantiate' must be a string");
            return new InstantiateExpr
            {
                ClassName = instProp.GetString() ?? "",
                Args = ParseArgs(el, "args"),
                NodeId = ReadId(el)
            };
        }

        if (el.TryGetProperty("comp_body", out var cbProp))
        {
            var bodyEl = cbProp.Clone();
            var varEl = el.GetProperty("comp_var");
            var sourceEl = el.GetProperty("comp_source");

            var result = new ListCompExpr
            {
                Body = ParseElement(bodyEl),
                Var = varEl.GetString() ?? "",
                Source = ParseElement(sourceEl.Clone()),
                NodeId = ReadId(el)
            };

            if (el.TryGetProperty("comp_filter", out var fEl) && fEl.ValueKind != JsonValueKind.Null)
                result.Filter = ParseElement(fEl.Clone());

            return result;
        }

        if (el.TryGetProperty("tuple", out var tupleProp))
        {
            if (tupleProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("'tuple' must be array");
            return new TupleExpr
            {
                Items = tupleProp.EnumerateArray()
                    .Select(x => ParseElement(x.Clone()))
                    .ToList(),
                NodeId = ReadId(el)
            };
        }

        if (el.TryGetProperty("set", out var setProp))
        {
            if (setProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("'set' must be array");
            return new SetLiteralExpr
            {
                Items = setProp.EnumerateArray()
                    .Select(x => ParseElement(x.Clone()))
                    .ToList(),
                NodeId = ReadId(el)
            };
        }

        if (el.TryGetProperty("dict", out var dictProp))
        {
            if (dictProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("'dict' must be array");

            var result = new DictExpr { NodeId = ReadId(el) };
            foreach (var entry in dictProp.EnumerateArray())
            {
                if (!entry.TryGetProperty("k", out var k) || !entry.TryGetProperty("v", out var v))
                    throw new JsonException("dict entry must have 'k' and 'v'");
                result.Items.Add(new DictEntry
                {
                    Key = ParseElement(k.Clone()),
                    Value = ParseElement(v.Clone())
                });
            }
            return result;
        }

        if (el.TryGetProperty("slice", out var sliceProp))
        {
            if (sliceProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("'slice' must be array of 3 elements [target, start, stop, step]");
            var arr = sliceProp.EnumerateArray().ToArray();
            if (arr.Length != 4)
                throw new JsonException("'slice' must have 4 elements [target, start, stop, step]");

            return new SliceExpr
            {
                Target = ParsePart(arr[0]),
                Start = arr[1].ValueKind == JsonValueKind.Null ? null : ParsePart(arr[1]),
                Stop = arr[2].ValueKind == JsonValueKind.Null ? null : ParsePart(arr[2]),
                Step = arr[3].ValueKind == JsonValueKind.Null ? null : ParsePart(arr[3]),
                NodeId = ReadId(el)
            };
        }

        if (el.TryGetProperty("array", out var arrProp))
        {
            if (arrProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("'array' must be array");
            return new ArrayExpr
            {
                Items = arrProp.EnumerateArray()
                    .Select(x => ParseElement(x.Clone()))
                    .ToList(),
                NodeId = ReadId(el)
            };
        }
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