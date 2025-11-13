using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Types
{
    public class VariableValueConverter : JsonConverter<VariableValue>
    {
        public override VariableValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Реализация десериализации при необходимости
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, VariableValue value, JsonSerializerOptions options)
        {
            // Безопасная сериализация
            switch (value.Type)
            {
                case VariableType.Array:
                    writer.WriteStartArray();
                    if (value.Value is List<VariableValue> array)
                    {
                        foreach (var item in array)
                        {
                            JsonSerializer.Serialize(writer, item.SerializableValue, options);
                        }
                    }
                    writer.WriteEndArray();
                    break;

                case VariableType.Object:
                    writer.WriteStartObject();
                    if (value.Value is Dictionary<string, VariableValue> obj)
                    {
                        foreach (var prop in obj)
                        {
                            writer.WritePropertyName(prop.Key);
                            JsonSerializer.Serialize(writer, prop.Value.SerializableValue, options);
                        }
                    }
                    writer.WriteEndObject();
                    break;

                default:
                    JsonSerializer.Serialize(writer, value.Value, options);
                    break;
            }
        }
    }
}
