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
    /// Представляет логическое (булево) значение.
    /// </summary>
    /// <remarks>
    /// Этот класс инкапсулирует логическое значение и предоставляет методы 
    /// преобразования в другие типы данных.
    /// </remarks>
    public class BoolValue : VariableValue
    {
        private readonly bool _value;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BoolValue"/>.
        /// </summary>
        /// <param name="value">Логическое значение.</param>
        public BoolValue(bool value) => _value = value;

        /// <summary>
        /// Получает тип значения - <see cref="VariableType.Bool"/>.
        /// </summary>
        public override VariableType Type => VariableType.Bool;

        /// <summary>
        /// Получает необработанное логическое значение.
        /// </summary>
        public override object RawValue => _value;

        /// <summary>
        /// Преобразует логическое значение в логический тип (возвращает само значение).
        /// </summary>
        public override bool ToBool() => _value;

        /// <summary>
        /// Преобразует логическое значение в число с плавающей запятой.
        /// </summary>
        /// <returns>
        /// 1.0 для <c>true</c>, 0.0 для <c>false</c>.
        /// </returns>
        public override double ToDouble() => _value ? 1.0 : 0.0;

        /// <summary>
        /// Преобразует логическое значение в целое число.
        /// </summary>
        /// <returns>
        /// 1 для <c>true</c>, 0 для <c>false</c>.
        /// </returns>
        public override int ToInt() => _value ? 1 : 0;

        /// <summary>
        /// Возвращает строковое представление логического значения.
        /// </summary>
        public override string ToValueString() => _value.ToString();
    }
}