using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class NumberNode : IExpressionNode
    {
        private readonly double _value;

        public NumberNode(double value) => _value = value;

        public object Evaluate(IVariableScope variables) => _value;
    }
}
