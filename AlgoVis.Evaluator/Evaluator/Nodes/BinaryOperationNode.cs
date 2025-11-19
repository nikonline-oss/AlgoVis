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

        public IVariableValue Evaluate(IVariableScope variables)
        {
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

        private IVariableValue Add(IVariableValue left, IVariableValue right)
        {
            // Конкатенация строк
            if (left is StringValue || right is StringValue)
                return new StringValue(left.ToString() + right.ToString());

            // Сложение чисел
            return new DoubleValue(left.ToDouble() + right.ToDouble());
        }

        private IVariableValue Subtract(IVariableValue left, IVariableValue right)
            => new DoubleValue(left.ToDouble() - right.ToDouble());

        private IVariableValue Multiply(IVariableValue left, IVariableValue right)
            => new DoubleValue(left.ToDouble() * right.ToDouble());

        private IVariableValue Divide(IVariableValue left, IVariableValue right)
        {
            if (Math.Abs(right.ToDouble()) < 1e-10)
                throw new DivideByZeroException("Division by zero");
            return new DoubleValue(left.ToDouble() / right.ToDouble());
        }

        private IVariableValue Modulo(IVariableValue left, IVariableValue right)
        {
            if (Math.Abs(right.ToDouble()) < 1e-10)
                throw new DivideByZeroException("Modulo by zero");
            return new DoubleValue(left.ToDouble() % right.ToDouble());
        }

        private IVariableValue Power(IVariableValue left, IVariableValue right)
            => new DoubleValue(Math.Pow(left.ToDouble(), right.ToDouble()));

        private bool Equals(IVariableValue left, IVariableValue right)
        {
            // Для строк - строковое сравнение
            if (left is StringValue leftStr && right is StringValue rightStr)
                return leftStr.ToString() == rightStr.ToString();

            // Для чисел - численное сравнение
            return Math.Abs(left.ToDouble() - right.ToDouble()) < 1e-10;
        }

        public override string ToString() => $"({_left} {_operator} {_right})";
    }
}