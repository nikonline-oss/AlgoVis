using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Types
{
    // Вспомогательный класс для хранения значений переменных
    public class VariableValue : IConvertible
    {
        public VariableType Type { get; set; }
        public object Value { get; set; }

        // Динамический массив/список
        public List<VariableValue> ArrayValue => Value as List<VariableValue>
            ?? throw new InvalidOperationException("Переменная не является массивом");

        // Объект (словарь свойств)
        public Dictionary<string, VariableValue> ObjectValue => Value as Dictionary<string, VariableValue>
            ?? throw new InvalidOperationException("Переменная не является объектом");

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
                string _ => VariableType.String,
                List<VariableValue> _ => VariableType.Array,
                Dictionary<string, VariableValue> _ => VariableType.Object,
                _ => VariableType.Object
            };
        }

        // Реализация IConvertible для поддержки преобразований
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

            while (index >= ArrayValue.Count)
                ArrayValue.Add(new VariableValue(0));

            return ArrayValue[index].Value;
        }

        public void SetElement(int index, object value)
        {
            if (Type != VariableType.Array)
                throw new InvalidOperationException("Переменная не является массивом");

            if (index < 0)
                throw new IndexOutOfRangeException($"Отрицательный индекс {index} не допустим");

            // Автоматическое расширение массива
            while (index >= ArrayValue.Count)
                ArrayValue.Add(new VariableValue(0));

            ArrayValue[index] = new VariableValue(value);
        }

        // Доступ к свойствам объекта
        public object GetProperty(string propertyName)
        {
            if (Type != VariableType.Object)
                throw new InvalidOperationException("Переменная не является объектом");

            return ObjectValue.TryGetValue(propertyName, out var value)
                ? value.Value
                : new VariableValue(0); // Возвращаем 0 для несуществующих свойств
        }

        public void SetProperty(string propertyName, object value)
        {
            if (Type != VariableType.Object)
                throw new InvalidOperationException("Переменная не является объектом");

            ObjectValue[propertyName] = new VariableValue(value);
        }
    }
}
