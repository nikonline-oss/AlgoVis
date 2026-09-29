using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    /// <summary>
    /// Представляет строковое значение.
    /// </summary>
    /// <remarks>
    /// Этот класс инкапсулирует строковые операции и предоставляет методы для работы со строками,
    /// аналогичные JavaScript. Поддерживает свойство "length" и различные строковые методы.
    /// </remarks>
    public class StringValue : VariableValue
    {
        private readonly string _value;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StringValue"/>.
        /// </summary>
        /// <param name="value">
        /// Строковое значение. Если <c>null</c>, преобразуется в пустую строку.
        /// </param>
        public StringValue(string value) => _value = value ?? string.Empty;

        /// <summary>
        /// Получает тип значения - <see cref="VariableType.String"/>.
        /// </summary>
        public override VariableType Type => VariableType.String;

        /// <summary>
        /// Получает необработанное строковое значение.
        /// </summary>
        public override object RawValue => _value;

        /// <summary>
        /// Преобразует строку в логическое значение.
        /// </summary>
        /// <returns>
        /// <c>true</c>, если строка не пустая и не <c>null</c>; в противном случае <c>false</c>.
        /// </returns>
        public override bool ToBool() => !string.IsNullOrEmpty(_value);

        /// <summary>
        /// Преобразует строку в число с плавающей запятой.
        /// </summary>
        /// <returns>
        /// Числовое значение строки или 0, если преобразование невозможно.
        /// </returns>
        public override double ToDouble() => double.TryParse(_value, out double result) ? result : 0;

        /// <summary>
        /// Преобразует строку в целое число.
        /// </summary>
        /// <returns>
        /// Целочисленное значение строки или 0, если преобразование невозможно.
        /// </returns>
        public override int ToInt() => int.TryParse(_value, out int result) ? result : 0;

        /// <summary>
        /// Возвращает строковое представление значения (исходную строку).
        /// </summary>
        public override string ToValueString() => _value;

        /// <summary>
        /// Словарь поддерживаемых методов строки.
        /// </summary>
        /// <remarks>
        /// Ключ - имя метода, значение - делегат для его выполнения.
        /// </remarks>
        private static readonly Dictionary<string, Func<StringValue, IVariableValue[], IVariableValue>> _methods = new()
        {
            ["toUpper"] = (self, args) => new StringValue(self._value.ToUpper()),
            ["toLower"] = (self, args) => new StringValue(self._value.ToLower()),
            ["substring"] = (self, args) =>
            {
                if (args.Length == 1)
                    return new StringValue(self._value.Substring(args[0].ToInt()));
                if (args.Length == 2)
                    return new StringValue(self._value.Substring(args[0].ToInt(), args[1].ToInt()));
                throw new InvalidOperationException("substring requires 1 or 2 arguments");
            },
            ["contains"] = (self, args) =>
                new BoolValue(self._value.Contains(args[0].ToString()))
        };

        /// <summary>
        /// Определяет, поддерживает ли строка указанное свойство.
        /// </summary>
        /// <param name="name">Имя свойства для проверки.</param>
        /// <returns>
        /// <c>true</c>, если свойство поддерживается (свойство "length"); в противном случае <c>false</c>.
        /// </returns>
        public override bool HasProperty(string name) => name == "length";

        /// <summary>
        /// Определяет, поддерживает ли строка указанный метод.
        /// </summary>
        /// <param name="name">Имя метода для проверки.</param>
        /// <returns>
        /// <c>true</c>, если метод существует в словаре <see cref="_methods"/>; в противном случае <c>false</c>.
        /// </returns>
        public override bool HasMethod(string name) => _methods.ContainsKey(name);

        /// <summary>
        /// Получает значение свойства строки.
        /// </summary>
        /// <param name="name">Имя свойства для получения.</param>
        /// <returns>
        /// Значение свойства или результат базовой реализации, если свойство не поддерживается.
        /// </returns>
        public override IVariableValue GetProperty(string name)
        {
            return name switch
            {
                "length" => new IntValue(_value.Length),
                _ => base.GetProperty(name)
            };
        }

        /// <summary>
        /// Вызывает метод строки.
        /// </summary>
        /// <param name="methodName">Имя вызываемого метода.</param>
        /// <param name="arguments">Аргументы метода.</param>
        /// <returns>
        /// Результат выполнения метода или исключение, если метод не поддерживается.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, когда метод не найден в словаре <see cref="_methods"/>.
        /// </exception>
        public override IVariableValue CallMethod(string methodName, IVariableValue[] arguments)
        {
            if (_methods.TryGetValue(methodName, out var method))
                return method(this, arguments);

            return base.CallMethod(methodName, arguments);
        }
    }
}