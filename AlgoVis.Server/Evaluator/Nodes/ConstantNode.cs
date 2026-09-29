using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    /// <summary>
    /// Узел выражения, представляющий константное значение.
    /// </summary>
    /// <remarks>
    /// Этот класс реализует узел AST (Abstract Syntax Tree), который хранит
    /// неизменяемое значение. При вычислении всегда возвращает сохраненное значение,
    /// независимо от контекста переменных.
    /// </remarks>
    public class ConstantNode : IExpressionNode
    {
        private readonly IVariableValue _value;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ConstantNode"/> с указанным значением.
        /// </summary>
        /// <param name="value">Константное значение, которое будет хранить узел.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="value"/> равен <c>null</c>.</exception>
        public ConstantNode(IVariableValue value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Вычисляет значение узла.
        /// </summary>
        /// <param name="variables">Область видимости переменных (игнорируется для константного узла).</param>
        /// <returns>Константное значение, переданное при создании узла.</returns>
        /// <remarks>
        /// Для <see cref="ConstantNode"/> этот метод всегда возвращает одно и то же значение,
        /// независимо от переданной области видимости переменных.
        /// </remarks>
        public IVariableValue Evaluate(IVariableScope variables) => _value;

        /// <summary>
        /// Возвращает строковое представление константного значения.
        /// </summary>
        /// <returns>Строковое представление значения, возвращаемое методом <c>ToString()</c> хранимого объекта <see cref="IVariableValue"/>.</returns>
        public override string ToString() => _value.ToString();
    }
}