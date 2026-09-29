using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    /// <summary>
    /// Представляет числовое значение с плавающей запятой двойной точности.
    /// </summary>
    /// <remarks>
    /// Этот класс инкапсулирует значение типа double и предоставляет методы
    /// преобразования в другие типы данных.
    /// </remarks>
    public class DoubleValue : VariableValue
    {
        private readonly double _value;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DoubleValue"/>.
        /// </summary>
        /// <param name="value">Числовое значение.</param>
        public DoubleValue(double value) => _value = value;

        /// <summary>
        /// Получает тип значения - <see cref="VariableType.Double"/>.
        /// </summary>
        public override VariableType Type => VariableType.Double;

        /// <summary>
        /// Получает необработанное числовое значение.
        /// </summary>
        public override object RawValue => _value;

        /// <summary>
        /// Преобразует числовое значение в целое число.
        /// </summary>
        /// <returns>Целая часть числа (приводится к int).</returns>
        public override int ToInt() => (int)_value;

        /// <summary>
        /// Преобразует числовое значение в число с плавающей запятой.
        /// </summary>
        /// <returns>Исходное значение double.</returns>
        public override double ToDouble() => _value;

        /// <summary>
        /// Преобразует числовое значение в логическое значение.
        /// </summary>
        /// <returns>
        /// <c>true</c>, если значение не равно 0 (с учетом погрешности); в противном случае <c>false</c>.
        /// </returns>
        public override bool ToBool() => Math.Abs(_value) > 1e-10;

        /// <summary>
        /// Возвращает строковое представление числового значения.
        /// </summary>
        /// <returns>
        /// Строковое представление числа в инвариантном формате.
        /// </returns>
        public override string ToValueString() => _value.ToString(CultureInfo.InvariantCulture);
    }
}