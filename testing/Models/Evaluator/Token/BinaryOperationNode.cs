using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator.Token
{
    public class BinaryOperationNode : IExpressionNode
    {
        private readonly IExpressionNode _left;
        private readonly IExpressionNode _right;
        private readonly string _operator;

        public BinaryOperationNode(IExpressionNode left, IExpressionNode right, string op)
        {
            _left = left;
            _right = right;
            _operator = op;
        }

        public object Evaluate(IVariableScope variables)
        {
            var leftVal = ExtractValue(_left.Evaluate(variables));
            var rightVal = ExtractValue(_right.Evaluate(variables));

            // Для логических операторов
            if (_operator == "&&" || _operator == "||")
            {
                bool leftBool = ConvertToBoolean(leftVal);
                bool rightBool = ConvertToBoolean(rightVal);

                return _operator switch
                {
                    "&&" => leftBool && rightBool,
                    "||" => leftBool || rightBool,
                    _ => throw new ArgumentException($"Неизвестный логический оператор: {_operator}")
                };
            }

            // Для операторов сравнения
            if (_operator == "==" || _operator == "!=")
            {
                bool areEqual = ObjectsEqual(leftVal, rightVal);
                return _operator == "==" ? areEqual : !areEqual;
            }

            // Для числовых операторов
            double leftNum = Convert.ToDouble(leftVal);
            double rightNum = Convert.ToDouble(rightVal);

            return _operator switch
            {
                "+" => leftNum + rightNum,
                "-" => leftNum - rightNum,
                "*" => leftNum * rightNum,
                "/" => rightNum != 0 ? leftNum / rightNum : throw new DivideByZeroException("Деление на ноль"),
                "%" => rightNum != 0 ? leftNum % rightNum : throw new DivideByZeroException("Деление на ноль"),
                "^" => Math.Pow(leftNum, rightNum),
                "<" => leftNum < rightNum,
                ">" => leftNum > rightNum,
                "<=" => leftNum <= rightNum,
                ">=" => leftNum >= rightNum,
                _ => throw new ArgumentException($"Неизвестный оператор: {_operator}")
            };
        }

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
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

        private bool ObjectsEqual(object a, object b)
        {
            var aVal = ExtractValue(a);
            var bVal = ExtractValue(b);

            if (aVal == null && bVal == null) return true;
            if (aVal == null || bVal == null) return false;
            return aVal.Equals(bVal);
        }
    }
}
