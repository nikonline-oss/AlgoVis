using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class ConstantNode : IExpressionNode
    {
        private readonly IVariableValue _value;

        public ConstantNode(IVariableValue value)
        {
            _value = value;
        }

        public IVariableValue Evaluate(IVariableScope variables) => _value;

        public override string ToString() => _value.ToString();
    }
}
