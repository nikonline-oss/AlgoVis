using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlgoVis.Server.converters
{
    /// <summary>
    /// Конвертер JSON для гибкого преобразования строковых значений в тип <see cref="bool"/>.
    /// </summary>
    /// <remarks>
    /// Этот конвертер позволяет десериализовать логические значения из различных строковых представлений,
    /// таких как "true"/"false", "1"/"0", "yes"/"no", "y"/"n" (без учета регистра).
    /// При сериализации всегда используется стандартное представление true/false.
    /// </remarks>
    public class FlexibleBoolConverter : JsonConverter<bool>
    {
        /// <summary>
        /// Десериализует JSON-значение в <see cref="bool"/>.
        /// </summary>
        /// <param name="reader">Объект <see cref="Utf8JsonReader"/> для чтения JSON-ввода.</param>
        /// <param name="typeToConvert">Целевой тип для преобразования.</param>
        /// <param name="options">Параметры сериализатора.</param>
        /// <returns>Десериализованное логическое значение.</returns>
        /// <exception cref="JsonException">
        /// Выбрасывается, если значение не может быть преобразовано в <see cref="bool"/>.
        /// </exception>
        /// <example>
        /// Примеры поддерживаемых значений:
        /// <code>
        /// true, false (стандартные JSON boolean)
        /// "true", "false" (строки)
        /// "1", "0" (числа как строки)
        /// "yes", "no" (утвердительные/отрицательные ответы)
        /// "y", "n" (сокращенные формы)
        /// </code>
        /// </example>
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Стандартная обработка JSON boolean значений
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;

            // Обработка строковых значений с приведением к нижнему регистру
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

        /// <summary>
        /// Сериализует значение <see cref="bool"/> в JSON.
        /// </summary>
        /// <param name="writer">Объект <see cref="Utf8JsonWriter"/> для записи JSON.</param>
        /// <param name="value">Логическое значение для сериализации.</param>
        /// <param name="options">Параметры сериализатора.</param>
        /// <remarks>
        /// Всегда использует стандартное JSON-представление boolean (true/false).
        /// </remarks>
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            => writer.WriteBooleanValue(value);
    }
}