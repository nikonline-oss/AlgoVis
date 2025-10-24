using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator
{
    // Вспомогательный класс для хранения значений переменных
    public class VariableValue : IConvertible
    {
        public VariableType Type { get; set; }
        public object Value { get; set; }

        public VariableValue(VariableType type, object value)
        {
            Type = type;
            Value = value;
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

            if (Value is Array array)
            {
                if (index >= 0 && index < array.Length)
                    return array.GetValue(index);
                throw new IndexOutOfRangeException($"Индекс {index} вне границ массива");
            }

            throw new InvalidOperationException("Некорректное значение массива");
        }

        public void SetElement(int index, object value)
        {
            if (Type != VariableType.Array)
                throw new InvalidOperationException("Переменная не является массивом");

            if (Value is Array array)
            {
                if (index >= 0 && index < array.Length)
                    array.SetValue(value, index);
                else
                    throw new IndexOutOfRangeException($"Индекс {index} вне границ массива");
            }
        }
    }
}
