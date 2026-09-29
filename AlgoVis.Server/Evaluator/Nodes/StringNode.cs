using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    /// <summary>
    /// Представляет узел выражения для строкового литерала.
    /// </summary>
    /// <remarks>
    /// Этот класс хранит строковое значение и возвращает его при вычислении.
    /// Используется для представления строковых констант в выражениях.
    /// </remarks>
    public class StringNode : IExpressionNode
    {
        private readonly string _value;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StringNode"/>.
        /// </summary>
        /// <param name="value">Строковое значение, которое будет хранить узел.</param>
        public StringNode(string value) => _value = value;

        /// <summary>
        /// Вычисляет значение узла.
        /// </summary>
        /// <param name="variables">Область видимости переменных для вычисления выражения.</param>
        /// <returns>Экземпляр <see cref="StringValue"/>, содержащий строковое значение.</returns>
        public IVariableValue Evaluate(IVariableScope variables) => new StringValue(_value);

        /// <summary>
        /// Возвращает строковое представление узла.
        /// </summary>
        /// <returns>Строка в формате "значение" (в кавычках).</returns>
        public override string ToString() => $"\"{_value}\"";
    }
}