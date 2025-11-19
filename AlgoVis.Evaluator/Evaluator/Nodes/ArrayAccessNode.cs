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
    // Узел для доступа к элементам массива: array[index]
    public class ArrayAccessNode : IExpressionNode
    {
        private readonly IExpressionNode _arrayExpression;
        private readonly IExpressionNode _indexExpression;

        public ArrayAccessNode(IExpressionNode arrayExpression, IExpressionNode indexExpression)
        {
            _arrayExpression = arrayExpression;
            _indexExpression = indexExpression;
        }

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

        public override string ToString() => $"{_arrayExpression}[{_indexExpression}]";
    }
}
