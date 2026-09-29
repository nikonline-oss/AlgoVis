using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    /// <summary>
    /// Представляет узел выражения для вызова метода объекта.
    /// </summary>
    /// <remarks>
    /// Данный узел используется в синтаксическом дереве для инкапсуляции операции
    /// вызова метода (<c>target.methodName(arguments)</c>). При выполнении сначала
    /// вычисляется целевой объект, затем все аргументы, после чего производится
    /// вызов указанного метода с вычисленными аргументами.
    /// </remarks>
    public class MethodCallNode : IExpressionNode
    {
        private readonly IExpressionNode _target;
        private readonly string _methodName;
        private readonly IReadOnlyList<IExpressionNode> _arguments;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MethodCallNode"/>.
        /// </summary>
        /// <param name="target">Узел выражения, вычисляющий целевой объект, у которого вызывается метод.</param>
        /// <param name="methodName">Имя вызываемого метода (регистрозависимое).</param>
        /// <param name="arguments">
        /// Список узлов выражений, представляющих аргументы метода.
        /// Если значение равно <c>null</c>, используется пустой список.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Вызывается, если <paramref name="target"/> или <paramref name="methodName"/> равны <c>null</c>.
        /// </exception>
        public MethodCallNode(IExpressionNode target, string methodName, IList<IExpressionNode> arguments)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            _arguments = (arguments ?? Array.Empty<IExpressionNode>()).AsReadOnly();
        }

        /// <summary>
        /// Выполняет вычисление выражения вызова метода.
        /// </summary>
        /// <param name="variables">Область видимости переменных для использования при вычислении выражений.</param>
        /// <returns>
        /// Значение, возвращаемое методом, или исключение, если вызов не удался.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Вызывается, если метод с именем <see cref="_methodName"/> не найден у целевого объекта,
        /// не может быть вызван с заданными аргументами или во время его выполнения возникает ошибка.
        /// </exception>
        public IVariableValue Evaluate(IVariableScope variables)
        {
            var targetValue = _target.Evaluate(variables);
            var argumentValues = _arguments.Select(arg => arg.Evaluate(variables)).ToArray();

            return targetValue.CallMethod(_methodName, argumentValues);
        }

        /// <summary>
        /// Возвращает строковое представление узла вызова метода.
        /// </summary>
        /// <returns>
        /// Строка в формате: <c>"target.methodName(arg1, arg2, ...)"</c>.
        /// </returns>
        public override string ToString() =>
            $"{_target}.{_methodName}({string.Join(", ", _arguments)})";
    }
}