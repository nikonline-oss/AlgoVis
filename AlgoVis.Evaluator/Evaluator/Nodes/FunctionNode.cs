using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{

    public class FunctionNode : IExpressionNode
    {
        private readonly string _functionName;
        private readonly List<IExpressionNode> _arguments;

        public FunctionNode(string functionName, List<IExpressionNode> arguments)
        {
            _functionName = functionName;
            _arguments = arguments;
        }

        public object Evaluate(IVariableScope variables)
        {
            var args = _arguments.Select(arg => Convert.ToDouble(ExtractValue(arg.Evaluate(variables)))).ToArray();

            return _functionName.ToLower() switch
            {
                "sin" => Math.Sin(args[0]),
                "cos" => Math.Cos(args[0]),
                "tan" => Math.Tan(args[0]),
                "sqrt" => args[0] >= 0 ? Math.Sqrt(args[0]) : throw new ArgumentException("Квадратный корень из отрицательного числа"),
                "abs" => Math.Abs(args[0]),
                "min" => Math.Min(args[0], args[1]),
                "max" => Math.Max(args[0], args[1]),
                "pow" => Math.Pow(args[0], args[1]),
                _ => throw new ArgumentException($"Неизвестная функция: {_functionName}")
            };
        }

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
        }
    }
}
