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
    /// Представляет узел выражения для переменной.
    /// </summary>
    /// <remarks>
    /// Выполняет поиск значения переменной по имени в области видимости.
    /// Специальная обработка для переменной "null", которая возвращает NullValue.
    /// </remarks>
    public class VariableNode : IExpressionNode
    {
        private readonly string _name;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="VariableNode"/>.
        /// </summary>
        /// <param name="name">Имя переменной.</param>
        public VariableNode(string name) => _name = name;

        /// <summary>
        /// Получает имя переменной.
        /// </summary>
        /// <value>Имя переменной, которую представляет этот узел.</value>
        public string Name => _name;

        /// <summary>
        /// Вычисляет значение переменной.
        /// </summary>
        /// <param name="variables">Область видимости переменных для поиска значения.</param>
        /// <returns>
        /// Значение переменной из области видимости, NullValue для переменной "null",
        /// или StringValue с текстовым представлением значения.
        /// </returns>
        public IVariableValue Evaluate(IVariableScope variables)
        {
            if (_name == "null") return new NullValue();

            var result = variables.Get(_name);
            return result as IVariableValue ?? new StringValue(result?.ToString() ?? "null");
        }

        /// <summary>
        /// Возвращает строковое представление узла переменной.
        /// </summary>
        /// <returns>Имя переменной.</returns>
        public override string ToString() => _name;
    }
}