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
            var args = _arguments.Select(arg => ExtractValue(arg.Evaluate(variables))).ToArray();

            return _functionName.ToLower() switch
            {
                // Математические функции
                "sin" => Math.Sin(Convert.ToDouble(args[0])),
                "cos" => Math.Cos(Convert.ToDouble(args[0])),
                "tan" => Math.Tan(Convert.ToDouble(args[0])),
                "sqrt" => args[0] is double d && d >= 0 ? Math.Sqrt(d) : throw new ArgumentException("Квадратный корень из отрицательного числа"),
                "abs" => Math.Abs(Convert.ToDouble(args[0])),
                "min" => Math.Min(Convert.ToDouble(args[0]), Convert.ToDouble(args[1])),
                "max" => Math.Max(Convert.ToDouble(args[0]), Convert.ToDouble(args[1])),
                "pow" => Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1])),

                // ДОБАВЛЕНО: Строковые функции
                "length" => args[0] is string str ? str.Length : throw new ArgumentException("Функция length ожидает строку"),
                "substring" => args[0] is string s ? s.Substring(
                    Convert.ToInt32(args[1]),
                    args.Length > 2 ? Convert.ToInt32(args[2]) : s.Length - Convert.ToInt32(args[1])
                ) : throw new ArgumentException("Функция substring ожидает строку"),
                "concat" => string.Concat(args.Select(arg => arg?.ToString() ?? "")),
                "toupper" => args[0] is string upperStr ? upperStr.ToUpper() : throw new ArgumentException("Функция toupper ожидает строку"),
                "tolower" => args[0] is string lowerStr ? lowerStr.ToLower() : throw new ArgumentException("Функция tolower ожидает строку"),

                _ => throw new ArgumentException($"Неизвестная функция: {_functionName}")
            };
        }

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
        }
    }
}