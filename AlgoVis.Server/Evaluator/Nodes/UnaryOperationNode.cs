using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    /// <summary>
    /// Представляет узел унарной операции.
    /// </summary>
    /// <remarks>
    /// Выполняет унарные операции над операндом: унарный плюс, унарный минус или логическое НЕ.
    /// Поддерживает операции для различных типов данных.
    /// </remarks>
    public class UnaryOperationNode : IExpressionNode
    {
        private readonly IExpressionNode _operand;
        private readonly string _operator;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UnaryOperationNode"/>.
        /// </summary>
        /// <param name="operand">Операнд унарной операции.</param>
        /// <param name="op">Оператор унарной операции ("u+", "u-", "u!").</param>
        /// <exception cref="ArgumentException">Выбрасывается, если передан неизвестный оператор.</exception>
        public UnaryOperationNode(IExpressionNode operand, string op)
        {
            _operand = operand;
            _operator = op;
        }

        /// <summary>
        /// Вычисляет значение унарной операции.
        /// </summary>
        /// <param name="variables">Область видимости переменных для вычисления выражения.</param>
        /// <returns>Результат унарной операции над операндом.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если оператор неизвестен.</exception>
        public IVariableValue Evaluate(IVariableScope variables)
        {
            var value = _operand.Evaluate(variables);

            return _operator switch
            {
                "u+" => value, // Унарный плюс
                "u-" => NegateValue(value),
                "u!" => LogicalNot(value),
                _ => throw new ArgumentException($"Неизвестный унарный оператор: {_operator}")
            };
        }

        /// <summary>
        /// Выполняет операцию отрицания (унарный минус) над значением.
        /// </summary>
        /// <param name="value">Значение для отрицания.</param>
        /// <returns>Отрицательное значение.</returns>
        /// <remarks>
        /// Для целых чисел возвращает IntValue, для вещественных - DoubleValue.
        /// Для других типов пытается преобразовать в DoubleValue.
        /// </remarks>
        private IVariableValue NegateValue(IVariableValue value)
        {
            return value.Type switch
            {
                VariableType.Int => new IntValue(-value.ToInt()),
                VariableType.Double => new DoubleValue(-value.ToDouble()),
                _ => new DoubleValue(-value.ToDouble()) // Пробуем преобразовать
            };
        }

        /// <summary>
        /// Выполняет операцию логического НЕ над значением.
        /// </summary>
        /// <param name="value">Значение для логического отрицания.</param>
        /// <returns>Логическое отрицание значения.</returns>
        private IVariableValue LogicalNot(IVariableValue value)
        {
            return new BoolValue(!value.ToBool());
        }

        /// <summary>
        /// Возвращает строковое представление узла унарной операции.
        /// </summary>
        /// <returns>Строка в формате "оператор операнд".</returns>
        public override string ToString() => $"{_operator}{_operand}";
    }
}