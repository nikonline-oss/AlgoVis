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
    /// Представляет узел дерева выражений для бинарной операции.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Этот класс реализует интерфейс <see cref="IExpressionNode"/> и предназначен для вычисления бинарных операций
    /// между двумя дочерними узлами (операндами). Поддерживаются арифметические, сравнения и логические операции.
    /// </para>
    /// <para>
    /// Класс автоматически обрабатывает приведение типов: строки конкатенируются, логические значения преобразуются,
    /// числовые значения приводятся к <see cref="double"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="IExpressionNode"/>
    /// <seealso cref="IVariableValue"/>
    public class BinaryOperationNode : IExpressionNode
    {
        /// <summary>Левый операнд (выражение)</summary>
        private readonly IExpressionNode _left;
        /// <summary>Правый операнд (выражение)</summary>
        private readonly IExpressionNode _right;
        /// <summary>Строковое представление оператора</summary>
        private readonly string _operator;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BinaryOperationNode"/>.
        /// </summary>
        /// <param name="left">Левый дочерний узел-операнд. Не может быть <c>null</c>.</param>
        /// <param name="right">Правый дочерний узел-операнд. Не может быть <c>null</c>.</param>
        /// <param name="op">Строка, представляющая оператор (например, "+", "&&", "==").</param>
        /// <exception cref="ArgumentNullException">Если <paramref name="left"/>, <paramref name="right"/> или <paramref name="op"/> равны <c>null</c>.</exception>
        public BinaryOperationNode(IExpressionNode left, IExpressionNode right, string op)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _operator = op ?? throw new ArgumentNullException(nameof(op));
        }

        /// <summary>
        /// Вычисляет значение бинарной операции для текущего узла.
        /// </summary>
        /// <param name="variables">Область видимости переменных, используемая для вычисления дочерних выражений.</param>
        /// <returns>Значение типа <see cref="IVariableValue"/>, представляющее результат операции.</returns>
        /// <exception cref="ArgumentNullException">Если <paramref name="variables"/> равен <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Если оператор <see cref="_operator"/> не поддерживается.</exception>
        /// <exception cref="DivideByZeroException">При операциях деления (<c>/</c>) или взятия остатка (<c>%</c>) на ноль.</exception>
        /// <remarks>
        /// <para>
        /// Метод последовательно:
        /// <list type="number">
        /// <item><description>Вычисляет значения левого и правого дочерних узлов.</description></item>
        /// <item><description>В зависимости от оператора вызывает соответствующий метод или создает значение.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Поддерживаемые операторы:
        /// <list type="table">
        /// <listheader><term>Оператор</term><description>Описание</description></listheader>
        /// <item><term>+</term><description>Сложение чисел или конкатенация строк</description></item>
        /// <item><term>-</term><description>Вычитание чисел</description></item>
        /// <item><term>*</term><description>Умножение чисел</description></item>
        /// <item><term>/</term><description>Деление чисел</description></item>
        /// <item><term>%</term><description>Остаток от деления</description></item>
        /// <item><term>^</term><description>Возведение в степень</description></item>
        /// <item><term>==, !=, &lt;, &gt;, &lt;=, &gt;=</term><description>Операторы сравнения</description></item>
        /// <item><term>&amp;&amp;, ||</term><description>Логические И и ИЛИ</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public IVariableValue Evaluate(IVariableScope variables)
        {
            if (variables == null) throw new ArgumentNullException(nameof(variables));

            var leftVal = _left.Evaluate(variables);
            var rightVal = _right.Evaluate(variables);

            return _operator switch
            {
                // Арифметические операции
                "+" => Add(leftVal, rightVal),
                "-" => Subtract(leftVal, rightVal),
                "*" => Multiply(leftVal, rightVal),
                "/" => Divide(leftVal, rightVal),
                "%" => Modulo(leftVal, rightVal),
                "^" => Power(leftVal, rightVal),

                // Операции сравнения
                "==" => new BoolValue(Equals(leftVal, rightVal)),
                "!=" => new BoolValue(!Equals(leftVal, rightVal)),
                "<" => new BoolValue(leftVal.ToDouble() < rightVal.ToDouble()),
                ">" => new BoolValue(leftVal.ToDouble() > rightVal.ToDouble()),
                "<=" => new BoolValue(leftVal.ToDouble() <= rightVal.ToDouble()),
                ">=" => new BoolValue(leftVal.ToDouble() >= rightVal.ToDouble()),

                // Логические операции
                "&&" => new BoolValue(leftVal.ToBool() && rightVal.ToBool()),
                "||" => new BoolValue(leftVal.ToBool() || rightVal.ToBool()),

                _ => throw new ArgumentException($"Unknown operator: {_operator}")
            };
        }

        /// <summary>
        /// Выполняет операцию сложения или конкатенации.
        /// </summary>
        /// <param name="left">Левый операнд.</param>
        /// <param name="right">Правый операнд.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="StringValue"/> - если хотя бы один операнд является строкой (конкатенация).</description></item>
        /// <item><description><see cref="DoubleValue"/> - результат сложения чисел в противном случае.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Приоритет строк: если <paramref name="left"/> или <paramref name="right"/> - <see cref="StringValue"/>,
        /// оба операнда преобразуются в строки и конкатенируются.
        /// </remarks>
        private IVariableValue Add(IVariableValue left, IVariableValue right)
        {
            // Конкатенация строк
            if (left is StringValue || right is StringValue)
                return new StringValue(left.ToString() + right.ToString());

            // Сложение чисел
            return new DoubleValue(left.ToDouble() + right.ToDouble());
        }

        /// <summary>
        /// Выполняет операцию вычитания.
        /// </summary>
        /// <param name="left">Левый операнд.</param>
        /// <param name="right">Правый операнд.</param>
        /// <returns><see cref="DoubleValue"/>, представляющий разность чисел.</returns>
        private IVariableValue Subtract(IVariableValue left, IVariableValue right)
            => new DoubleValue(left.ToDouble() - right.ToDouble());

        /// <summary>
        /// Выполняет операцию умножения.
        /// </summary>
        /// <param name="left">Левый операнд.</param>
        /// <param name="right">Правый операнд.</param>
        /// <returns><see cref="DoubleValue"/>, представляющий произведение чисел.</returns>
        private IVariableValue Multiply(IVariableValue left, IVariableValue right)
            => new DoubleValue(left.ToDouble() * right.ToDouble());

        /// <summary>
        /// Выполняет операцию деления.
        /// </summary>
        /// <param name="left">Делимое.</param>
        /// <param name="right">Делитель.</param>
        /// <returns><see cref="DoubleValue"/>, представляющий частное.</returns>
        /// <exception cref="DivideByZeroException">Если абсолютное значение <paramref name="right"/> меньше 1e-10.</exception>
        private IVariableValue Divide(IVariableValue left, IVariableValue right)
        {
            if (Math.Abs(right.ToDouble()) < 1e-10)
                throw new DivideByZeroException("Division by zero");
            return new DoubleValue(left.ToDouble() / right.ToDouble());
        }

        /// <summary>
        /// Выполняет операцию взятия остатка от деления.
        /// </summary>
        /// <param name="left">Делимое.</param>
        /// <param name="right">Делитель.</param>
        /// <returns><see cref="DoubleValue"/>, представляющий остаток.</returns>
        /// <exception cref="DivideByZeroException">Если абсолютное значение <paramref name="right"/> меньше 1e-10.</exception>
        private IVariableValue Modulo(IVariableValue left, IVariableValue right)
        {
            if (Math.Abs(right.ToDouble()) < 1e-10)
                throw new DivideByZeroException("Modulo by zero");
            return new DoubleValue(left.ToDouble() % right.ToDouble());
        }

        /// <summary>
        /// Выполняет операцию возведения в степень.
        /// </summary>
        /// <param name="left">Основание.</param>
        /// <param name="right">Показатель степени.</param>
        /// <returns><see cref="DoubleValue"/>, представляющий результат возведения в степень.</returns>
        private IVariableValue Power(IVariableValue left, IVariableValue right)
            => new DoubleValue(Math.Pow(left.ToDouble(), right.ToDouble()));

        /// <summary>
        /// Сравнивает два значения на равенство с учетом типа.
        /// </summary>
        /// <param name="left">Первое значение.</param>
        /// <param name="right">Второе значение.</param>
        /// <returns><c>true</c>, если значения считаются равными; иначе <c>false</c>.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>Для двух <see cref="StringValue"/> выполняется строковое сравнение.</description></item>
        /// <item><description>Для остальных случаев значения преобразуются в <see cref="double"/> и сравниваются с точностью до 1e-10.</description></item>
        /// </list>
        /// </remarks>
        private bool Equals(IVariableValue left, IVariableValue right)
        {
            // Для строк - строковое сравнение
            if (left is StringValue leftStr && right is StringValue rightStr)
                return leftStr.ToString() == rightStr.ToString();

            // Для чисел - численное сравнение
            return Math.Abs(left.ToDouble() - right.ToDouble()) < 1e-10;
        }

        /// <summary>
        /// Возвращает строковое представление узла в формате "(левый_операнд оператор правый_операнд)".
        /// </summary>
        /// <returns>Строка, представляющая текущий узел.</returns>
        public override string ToString() => $"({_left} {_operator} {_right})";
    }
}