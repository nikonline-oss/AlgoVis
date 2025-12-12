using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    public class IntValue : VariableValue
    {
        private readonly int _value;

        public IntValue(int value) => _value = value;
        public override VariableType Type => VariableType.Int;
        public override object RawValue => _value;

        public override int ToInt() => _value;
        public override double ToDouble() => _value;
        public override bool ToBool() => _value != 0;
        public override string ToValueString() => _value.ToString();
    }
}
