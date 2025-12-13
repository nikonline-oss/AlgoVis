using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlgoVis.Server.converters
{
    public class FlexibleBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;

            string? value = reader.GetString()?.Trim().ToLower();
            return value switch
            {
                "true" => true,
                "1" => true,
                "yes" => true,
                "y" => true,
                "false" => false,
                "0" => false,
                "no" => false,
                "n" => false,
                _ => throw new JsonException($"Не удалось преобразовать '{reader.GetString()}' в bool")
            };
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            => writer.WriteBooleanValue(value);
    }
}
