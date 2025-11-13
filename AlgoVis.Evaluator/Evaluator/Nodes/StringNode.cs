using AlgoVis.Evaluator.Evaluator.Interfaces;
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

        public StringNode(string value)
        {
            _value = value;
        }

        public object Evaluate(IVariableScope variables)
        {
            return _value;
        }

        public override string ToString()
        {
            return $"\"{_value}\"";
        }
    }
}
