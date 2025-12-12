using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class NumberNode : IExpressionNode
    {
        private readonly double _value;

        public NumberNode(double value) => _value = value;

        public IVariableValue Evaluate(IVariableScope variables)
        {
            // Определяем, целое это число или дробное
            return _value % 1 == 0
                ? new IntValue((int)_value)
                : new DoubleValue(_value);
        }

        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
    }
}
