using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator.Token
{
    public class NumberNode : IExpressionNode
    {
        private readonly double _value;

        public NumberNode(double value) => _value = value;

        public object Evaluate(IVariableScope variables) => _value;
    }
}
