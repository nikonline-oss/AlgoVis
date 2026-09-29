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
    /// Представляет нулевое (null) значение.
    /// </summary>
    /// <remarks>
    /// Этот класс используется для представления отсутствия значения.
    /// При преобразованиях всегда возвращает нейтральные значения (0, false и т.д.).
    /// </remarks>
    public class NullValue : VariableValue
    {
        /// <summary>
        /// Получает тип значения - <see cref="VariableType.Object"/> (для совместимости).
        /// </summary>
        public override VariableType Type => VariableType.Object;

        /// <summary>
        /// Получает необработанное значение - <c>null</c>.
        /// </summary>
        public override object RawValue => null;

        /// <summary>
        /// Преобразует нулевое значение в целое число.
        /// </summary>
        /// <returns>Всегда возвращает 0.</returns>
        public override int ToInt() => 0;

        /// <summary>
        /// Преобразует нулевое значение в число с плавающей запятой.
        /// </summary>
        /// <returns>Всегда возвращает 0.0.</returns>
        public override double ToDouble() => 0;

        /// <summary>
        /// Преобразует нулевое значение в логическое значение.
        /// </summary>
        /// <returns>Всегда возвращает <c>false</c>.</returns>
        public override bool ToBool() => false;

        /// <summary>
        /// Возвращает строковое представление нулевого значения.
        /// </summary>
        /// <returns>Всегда возвращает строку "null".</returns>
        public override string ToValueString() => "null";
    }
}