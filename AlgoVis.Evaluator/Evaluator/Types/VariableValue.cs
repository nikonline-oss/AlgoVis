using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Types
{
    public class VariableValue : IConvertible
    {
        public VariableType Type { get; set; }
        public object Value { get; set; }

        // Сделаем свойства только для чтения и добавим JsonIgnore
        [JsonIgnore]
        public List<VariableValue> ArrayValue
        {
            get
            {
                if (Type != VariableType.Array)
                    throw new InvalidOperationException("Переменная не является массивом");
                return Value as List<VariableValue> ?? new List<VariableValue>();
            }
        }

        [JsonIgnore]
        public Dictionary<string, VariableValue> ObjectValue
        {
            get
            {
                if (Type != VariableType.Object)
                    throw new InvalidOperationException("Переменная не является объектом");
                return Value as Dictionary<string, VariableValue> ?? new Dictionary<string, VariableValue>();
            }
        }

        // Добавим свойство для безопасной сериализации
        [JsonPropertyName("value")]
        public object SerializableValue
        {
            get
            {
                return Type switch
                {
                    VariableType.Array => ConvertArrayToSerializable(),
                    VariableType.Object => ConvertObjectToSerializable(),
                    _ => Value
                };
            }
        }

        [JsonPropertyName("type")]
        public string SerializableType => Type.ToString();

        public VariableValue(VariableType type, object value)
        {
            Type = type;
            Value = value;
        }

        public VariableValue(object value)
        {
            Value = value;
            Type = DetectType(value);
        }

        private VariableType DetectType(object value)
        {
            return value switch
            {
                int _ => VariableType.Int,
                double _ => VariableType.Double,
                bool _ => VariableType.Bool,
                List<VariableValue> _ => VariableType.Array,
                Dictionary<string, VariableValue> _ => VariableType.Object,
                string _ => VariableType.String,
                _ => VariableType.Object
            };
        }

        private List<object> ConvertArrayToSerializable()
        {
            var result = new List<object>();
            if (Type == VariableType.Array && Value is List<VariableValue> array)
            {
                foreach (var item in array)
                {
                    result.Add(item.SerializableValue);
                }
            }
            return result;
        }

        private Dictionary<string, object> ConvertObjectToSerializable()
        {
            var result = new Dictionary<string, object>();
            if (Type == VariableType.Object && Value is Dictionary<string, VariableValue> obj)
            {
                foreach (var prop in obj)
                {
                    result[prop.Key] = prop.Value.SerializableValue;
                }
            }
            return result;
        }

        public object GetNestedProperty(string[] path, int depth = 0)
        {
            if (depth >= path.Length) return this;

            if (Type == VariableType.Object && Value is Dictionary<string, VariableValue> dict)
            {
                if (dict.TryGetValue(path[depth], out var nextValue))
                {
                    return nextValue.GetNestedProperty(path, depth + 1);
                }
            }

            throw new InvalidOperationException($"Свойство '{path[depth]}' не найдено на глубине {depth}");
        }

        public void SetNestedProperty(string[] path, object value, int depth = 0)
        {
            if (depth >= path.Length)
            {
                Value = value;
                Type = DetectType(value);
                return;
            }

            if (Type != VariableType.Object)
            {
                Value = new Dictionary<string, VariableValue>();
                Type = VariableType.Object;
            }

            var dict = Value as Dictionary<string, VariableValue>;
            if (!dict.ContainsKey(path[depth]))
            {
                dict[path[depth]] = new VariableValue(0);
            }

            dict[path[depth]].SetNestedProperty(path, value, depth + 1);
        }

        // Реализация IConvertible
        public TypeCode GetTypeCode() => TypeCode.Object;
        public bool ToBoolean(IFormatProvider provider) => Convert.ToBoolean(Value, provider);
        public byte ToByte(IFormatProvider provider) => Convert.ToByte(Value, provider);
        public char ToChar(IFormatProvider provider) => Convert.ToChar(Value, provider);
        public DateTime ToDateTime(IFormatProvider provider) => Convert.ToDateTime(Value, provider);
        public decimal ToDecimal(IFormatProvider provider) => Convert.ToDecimal(Value, provider);
        public double ToDouble(IFormatProvider provider) => Convert.ToDouble(Value, provider);
        public short ToInt16(IFormatProvider provider) => Convert.ToInt16(Value, provider);
        public int ToInt32(IFormatProvider provider) => Convert.ToInt32(Value, provider);
        public long ToInt64(IFormatProvider provider) => Convert.ToInt64(Value, provider);
        public sbyte ToSByte(IFormatProvider provider) => Convert.ToSByte(Value, provider);
        public float ToSingle(IFormatProvider provider) => Convert.ToSingle(Value, provider);
        public string ToString(IFormatProvider provider) => Convert.ToString(Value, provider);
        public object ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(Value, conversionType, provider);
        public ushort ToUInt16(IFormatProvider provider) => Convert.ToUInt16(Value, provider);
        public uint ToUInt32(IFormatProvider provider) => Convert.ToUInt32(Value, provider);
        public ulong ToUInt64(IFormatProvider provider) => Convert.ToUInt64(Value, provider);

        public override string ToString() => Value?.ToString() ?? "null";

        public object GetElement(int index)
        {
            if (Type != VariableType.Array)
                throw new InvalidOperationException("Переменная не является массивом");

            if (index < 0)
                throw new IndexOutOfRangeException($"Отрицательный индекс {index} не допустим");

            var array = Value as List<VariableValue>;
            while (index >= array.Count)
                array.Add(new VariableValue(0));

            return array[index].Value;
        }

        public void SetElement(int index, object value)
        {
            if (Type != VariableType.Array)
                throw new InvalidOperationException("Переменная не является массивом");

            if (index < 0)
                throw new IndexOutOfRangeException($"Отрицательный индекс {index} не допустим");

            var array = Value as List<VariableValue>;
            while (index >= array.Count)
                array.Add(new VariableValue(0));

            array[index] = new VariableValue(value);
        }

        public void SetProperty(string propertyName, object value)
        {
            if (Type != VariableType.Object)
            {
                // Автоматически преобразуем в объект
                Console.WriteLine($"🔍 SetProperty: преобразование в объект, было {Type}");

                var newObj = new Dictionary<string, VariableValue>();

                // Сохраняем текущее значение как свойство "value"
                if (Value != null)
                {
                    newObj["value"] = new VariableValue(Value);
                }

                Value = newObj;
                Type = VariableType.Object;
            }

            var obj = Value as Dictionary<string, VariableValue>;
            obj[propertyName] = value is VariableValue variableValue ? variableValue : new VariableValue(value);

            Console.WriteLine($"🔍 SetProperty: установлено {propertyName} = {value}");
        }

        public object GetProperty(string propertyName)
        {
            if (Type != VariableType.Object)
            {
                Console.WriteLine($"⚠️ GetProperty: переменная не объект, тип = {Type}");

                // Если запрашивают свойство "value" у не-объекта, возвращаем значение
                if (propertyName == "value")
                {
                    return Value;
                }

                return 0;
            }

            var obj = Value as Dictionary<string, VariableValue>;
            if (obj.TryGetValue(propertyName, out var value))
            {
                Console.WriteLine($"🔍 GetProperty: найдено {propertyName} = {value.Value} (тип: {value.Type})");

                // Если значение - это объект, возвращаем его как словарь для дальнейшего доступа
                if (value.Type == VariableType.Object)
                {
                    return value.Value; // Возвращаем Dictionary<string, VariableValue>
                }

                return value.Value;
            }

            Console.WriteLine($"⚠️ GetProperty: свойство '{propertyName}' не найдено");
            return 0;
        }
    }
}
