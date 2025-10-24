using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator.Token
{
    public class UnaryOperationNode : IExpressionNode
    {
        private readonly IExpressionNode _operand;
        private readonly string _operator;

        public UnaryOperationNode(IExpressionNode operand, string op)
        {
            _operand = operand;
            _operator = op;
        }

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
        }

        public object Evaluate(IVariableScope variables)
        {
            var value = ExtractValue(_operand.Evaluate(variables));

            return _operator switch
            {
                "u+" => Convert.ToDouble(value),
                "u-" => -Convert.ToDouble(value),
                "u!" => !ConvertToBoolean(value),
                _ => throw new ArgumentException($"Неизвестный унарный оператор: {_operator}")
            };
        }

        private bool ConvertToBoolean(object value)
        {
            var extractedValue = ExtractValue(value);

            return extractedValue switch
            {
                bool b => b,
                int i => i != 0,
                double d => Math.Abs(d) > 1e-10,
                string s => !string.IsNullOrEmpty(s),
                _ => Convert.ToBoolean(extractedValue)
            };
        }
    }
}
