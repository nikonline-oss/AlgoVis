using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    public class BoolValue : VariableValue
    {
        private readonly bool _value;

        public BoolValue(bool value) => _value = value;

        public override VariableType Type => VariableType.Bool;
        public override object RawValue => _value;

        public override bool ToBool() => _value;
        public override double ToDouble() => _value ? 1.0 : 0.0;
        public override int ToInt() => _value ? 1 : 0;
        public override string ToValueString() => _value.ToString();

    }
}
