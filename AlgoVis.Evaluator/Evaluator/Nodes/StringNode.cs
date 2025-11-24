using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class StringNode : IExpressionNode
    {
        private readonly string _value;

        public StringNode(string value) => _value = value;

        public IVariableValue Evaluate(IVariableScope variables) => new StringValue(_value);

        public override string ToString() => $"\"{_value}\"";
    }
}
