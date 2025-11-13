using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class VariableNode : IExpressionNode
    {
        private readonly string _name;

        public VariableNode(string name) => _name = name;
        public string Name => _name;
        public object Evaluate(IVariableScope variables)
        {
            if(_name == "null")
            {
                return (object)null;
            }

            var result = variables.Get(_name);

            // Если результат - VariableValue, извлекаем его значение
            if (result is VariableValue variableValue)
            {
                return variableValue.Value;
            }

            return result;
        }
    }
}
