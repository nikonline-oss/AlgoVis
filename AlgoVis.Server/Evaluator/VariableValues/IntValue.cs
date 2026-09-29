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
    /// Представляет целочисленное значение.
    /// </summary>
    /// <remarks>
    /// Этот класс инкапсулирует значение типа int и предоставляет методы
    /// преобразования в другие типы данных.
    /// </remarks>
    public class IntValue : VariableValue
    {
        private readonly int _value;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="IntValue"/>.
        /// </summary>
        /// <param name="value">Целочисленное значение.</param>
        public IntValue(int value) => _value = value;

        /// <summary>
        /// Получает тип значения - <see cref="VariableType.Int"/>.
        /// </summary>
        public override VariableType Type => VariableType.Int;

        /// <summary>
        /// Получает необработанное целочисленное значение.
        /// </summary>
        public override object RawValue => _value;

        /// <summary>
        /// Преобразует целочисленное значение в целое число.
        /// </summary>
        /// <returns>Исходное значение int.</returns>
        public override int ToInt() => _value;

        /// <summary>
        /// Преобразует целочисленное значение в число с плавающей запятой.
        /// </summary>
        /// <returns>Преобразованное значение double.</returns>
        public override double ToDouble() => _value;

        /// <summary>
        /// Преобразует целочисленное значение в логическое значение.
        /// </summary>
        /// <returns>
        /// <c>true</c>, если значение не равно 0; в противном случае <c>false</c>.
        /// </returns>
        public override bool ToBool() => _value != 0;

        /// <summary>
        /// Возвращает строковое представление целочисленного значения.
        /// </summary>
        public override string ToValueString() => _value.ToString();
    }
}