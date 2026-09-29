using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    /// <summary>
    /// Узел выражения, представляющий числовое значение.
    /// </summary>
    /// <remarks>
    /// Этот класс является листовым узлом в дереве выражений и хранит числовую константу.
    /// В зависимости от значения (целое или дробное) создает соответствующее значение переменной.
    /// </remarks>
    /// <seealso cref="AlgoVis.Evaluator.Evaluator.Interfaces.IExpressionNode" />
    public class NumberNode : IExpressionNode
    {
        /// <summary>
        /// Числовое значение, хранимое в узле.
        /// </summary>
        /// <remarks>
        /// Значение хранится в формате double для поддержки как целых, так и дробных чисел.
        /// </remarks>
        private readonly double _value;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="NumberNode"/>.
        /// </summary>
        /// <param name="value">Числовое значение, которое будет храниться в узле.</param>
        public NumberNode(double value) => _value = value;

        /// <summary>
        /// Вычисляет значение узла.
        /// </summary>
        /// <remarks>
        /// Метод проверяет, является ли число целым (остаток от деления на 1 равен 0)
        /// и возвращает соответствующее значение: <see cref="IntValue"/> для целых чисел
        /// или <see cref="DoubleValue"/> для дробных чисел.
        /// </remarks>
        /// <param name="variables">Область видимости переменных (в данном узле не используется).</param>
        /// <returns>Значение переменной типа <see cref="IntValue"/> или <see cref="DoubleValue"/>.</returns>
        public IVariableValue Evaluate(IVariableScope variables)
        {
            // Определяем, целое это число или дробное
            return _value % 1 == 0
                ? new IntValue((int)_value)
                : new DoubleValue(_value);
        }

        /// <summary>
        /// Возвращает строковое представление числового значения.
        /// </summary>
        /// <remarks>
        /// Для преобразования используется инвариантная культура (с точкой в качестве разделителя дробной части),
        /// чтобы обеспечить единообразное представление независимо от региональных настроек системы.
        /// </remarks>
        /// <returns>Строковое представление числового значения.</returns>
        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
    }
}