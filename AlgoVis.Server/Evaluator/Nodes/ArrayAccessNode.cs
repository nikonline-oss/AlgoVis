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
    /// Узел для доступа к элементам массива или свойствам объектов через индекс.
    /// Поддерживает синтаксис: array[index] или object[index].
    /// </summary>
    /// <remarks>
    /// Этот класс реализует интерфейс <see cref="IExpressionNode"/> и обеспечивает
    /// доступ к элементам массивов по индексу с автоматическим расширением массива
    /// при обращении к несуществующим индексам, а также доступ к свойствам объектов
    /// через строковые индексы.
    /// </remarks>
    public class ArrayAccessNode : IExpressionNode
    {
        private readonly IExpressionNode _arrayExpression;
        private readonly IExpressionNode _indexExpression;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ArrayAccessNode"/>.
        /// </summary>
        /// <param name="arrayExpression">Выражение, возвращающее массив или объект.</param>
        /// <param name="indexExpression">Выражение, возвращающее индекс или имя свойства.</param>
        /// <exception cref="ArgumentNullException">
        /// Вызывается, если <paramref name="arrayExpression"/> или 
        /// <paramref name="indexExpression"/> равны <c>null</c>.
        /// </exception>
        public ArrayAccessNode(IExpressionNode arrayExpression, IExpressionNode indexExpression)
        {
            _arrayExpression = arrayExpression ?? throw new ArgumentNullException(nameof(arrayExpression));
            _indexExpression = indexExpression ?? throw new ArgumentNullException(nameof(indexExpression));
        }

        /// <summary>
        /// Выполняет вычисление значения доступа к массиву или свойству.
        /// </summary>
        /// <param name="variables">Область видимости переменных для вычисления выражений.</param>
        /// <returns>
        /// Значение элемента массива по указанному индексу или значение свойства объекта.
        /// При обращении к несуществующему индексу массива создается новый элемент.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Вызывается, когда индекс массива отрицательный.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Вызывается, когда выражение массива не возвращает массив или объект.
        /// </exception>
        /// <remarks>
        /// <para>Алгоритм работы:</para>
        /// <list type="number">
        /// <item>
        /// <description>Вычисляются значения выражений массива и индекса.</description>
        /// </item>
        /// <item>
        /// <description>Если у значения массива есть свойство "values", оно используется как массив.</description>
        /// </item>
        /// <item>
        /// <description>Если значение является массивом (<see cref="ArrayValue"/>):
        /// <list type="bullet">
        /// <item>
        /// <description>При корректном индексе возвращается элемент массива.</description>
        /// </item>
        /// <item>
        /// <description>При индексе за пределами массива (но неотрицательном) массив автоматически расширяется.</description>
        /// </item>
        /// <item>
        /// <description>При отрицательном индексе выбрасывается исключение.</description>
        /// </item>
        /// </list>
        /// </description>
        /// </item>
        /// <item>
        /// <description>Если значение не является массивом, производится доступ к свойству с именем индекса.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public IVariableValue Evaluate(IVariableScope variables)
        {
            var arrayValue = _arrayExpression.Evaluate(variables);
            var indexValue = _indexExpression.Evaluate(variables);

            if (arrayValue.HasProperty("values"))
                arrayValue = arrayValue.GetProperty("values");

            if (arrayValue is ArrayValue arrayVal)
            {
                var index = indexValue.ToInt();

                // Автоматическое расширение массива при обращении к несуществующему индексу
                if (index >= 0 && index < arrayVal.Length)
                {
                    return arrayVal[index];
                }
                else if (index >= 0)
                {
                    // Автоматически расширяем массив до нужного размера
                    var newValue = new IntValue(0);
                    // Массив автоматически расширяется через индексатор
                    arrayVal[index] = newValue;
                    return newValue;
                }
                else
                {
                    throw new IndexOutOfRangeException($"Array index cannot be negative: {index}");
                }
            }

            // Если это не массив, пытаемся получить свойство с именем индекса
            return arrayValue.GetProperty(indexValue.ToValueString());
        }

        /// <summary>
        /// Возвращает строковое представление узла доступа к массиву.
        /// </summary>
        /// <returns>Строка в формате: выражение_массива[выражение_индекса].</returns>
        public override string ToString() => $"{_arrayExpression}[{_indexExpression}]";
    }
}