using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
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
        private readonly IExpressionNode _arrayNode;
        private readonly IExpressionNode _indexNode;

        public ArrayAccessNode(IExpressionNode arrayNode, IExpressionNode indexNode)
        {
            _arrayNode = arrayNode;
            _indexNode = indexNode;
        }

        public object Evaluate(IVariableScope variables)
        {
            var array = _arrayNode.Evaluate(variables);
            var index = _indexNode.Evaluate(variables);

            int indexValue = Convert.ToInt32(ExtractValue(index));

            // Если это VariableValue, используем его методы
            if (array is VariableValue variableValue)
            {
                return variableValue.GetElement(indexValue);
            }

            // Получаем через VariableScope
            string arrayName = GetArrayName();
            return variables.GetElement(arrayName, indexValue);
        }

        public string GetArrayName()
        {
            // Рекурсивно получаем имя массива
            if (_arrayNode is VariableNode variableNode)
            {
                return variableNode.Name;
            }
            else if (_arrayNode is MemberAccessNode memberAccess)
            {
                return $"{memberAccess.GetBaseVariableName()}.{memberAccess._propertyName}";
            }
            else if (_arrayNode is ArrayAccessNode arrayAccess)
            {
                return arrayAccess.GetArrayName();
            }

            throw new InvalidOperationException("Не удается определить имя массива");
        }

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
        }
    }
}
