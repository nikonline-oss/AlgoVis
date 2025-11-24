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
    public class VariableNode : IExpressionNode
    {
        private readonly string _name;

        public VariableNode(string name) => _name = name;
        public string Name => _name;

        public IVariableValue Evaluate(IVariableScope variables)
        {
            if (_name == "null") return new NullValue();

            var result = variables.Get(_name);
            return result as IVariableValue ?? new StringValue(result?.ToString() ?? "null");
        }

        public override string ToString() => _name;
    }
}
