using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator.Token
{
    public class VariableNode : IExpressionNode
    {
        private readonly string _name;

        public VariableNode(string name) => _name = name;

        public object Evaluate(IVariableScope variables)
        {
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
