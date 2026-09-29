using AlgoVis.Evaluator.Evaluator.Interfaces;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    /// <summary>
    /// Представляет узел абстрактного синтаксического дерева (AST) для вычисления выражений.
    /// </summary>
    /// <remarks>
    /// Все узлы выражения в системе визуализации алгоритмов должны реализовывать этот интерфейс.
    /// Это обеспечивает единый контракт для вычисления выражений в различных контекстах.
    /// </remarks>
    public interface IExpressionNode
    {
        /// <summary>
        /// Вычисляет значение узла выражения в заданной области видимости переменных.
        /// </summary>
        /// <param name="variables">Область видимости переменных, предоставляющая доступ к значениям переменных.
        /// Может содержать как глобальные, так и локальные переменные в зависимости от контекста.</param>
        /// <returns>Результат вычисления выражения как <see cref="IVariableValue"/>.
        /// Тип возвращаемого значения зависит от конкретной реализации узла (логическое, числовое, строковое и т.д.).</returns>
        /// <exception cref="System.ArgumentNullException">Выбрасывается, если <paramref name="variables"/> равен null.</exception>
        /// <exception cref="AlgoVis.Evaluator.Evaluator.Exceptions.UndefinedVariableException">
        /// Может быть выброшено, если выражение ссылается на переменную, отсутствующую в области видимости.</exception>
        /// <exception cref="AlgoVis.Evaluator.Evaluator.Exceptions.TypeMismatchException">
        /// Может быть выброшено при несовместимости типов в операции (например, попытка сложить число и строку).</exception>
        /// <example>
        /// Пример использования:
        /// <code>
        /// IVariableScope scope = new VariableScope();
        /// scope.DeclareVariable("x", new NumberValue(5));
        /// IExpressionNode node = new BinaryOperationNode(Operation.Add, 
        ///     new VariableNode("x"), new ConstantNode(new NumberValue(3)));
        /// IVariableValue result = node.Evaluate(scope); // Вернет NumberValue(8)
        /// </code>
        /// </example>
        IVariableValue Evaluate(IVariableScope variables);
    }
}