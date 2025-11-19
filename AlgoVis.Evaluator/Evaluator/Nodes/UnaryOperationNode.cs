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
    public class UnaryOperationNode : IExpressionNode
    {
        private readonly IExpressionNode _operand;
        private readonly string _operator;

        public UnaryOperationNode(IExpressionNode operand, string op)
        {
            _operand = operand;
            _operator = op;
        }

        public IVariableValue Evaluate(IVariableScope variables)
        {
            var value = _operand.Evaluate(variables);

            return _operator switch
            {
                "u+" => value, // Унарный плюс
                "u-" => NegateValue(value),
                "u!" => LogicalNot(value),
                _ => throw new ArgumentException($"Неизвестный унарный оператор: {_operator}")
            };
        }

        private IVariableValue NegateValue(IVariableValue value)
        {
            return value.Type switch
            {
                VariableType.Int => new IntValue(-value.ToInt()),
                VariableType.Double => new DoubleValue(-value.ToDouble()),
                _ => new DoubleValue(-value.ToDouble()) // Пробуем преобразовать
            };
        }

        private IVariableValue LogicalNot(IVariableValue value)
        {
            return new BoolValue(!value.ToBool());
        }

        public override string ToString() => $"{_operator}{_operand}";
    }
}
