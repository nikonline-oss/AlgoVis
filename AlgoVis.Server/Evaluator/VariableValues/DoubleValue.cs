using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    public class DoubleValue : VariableValue
    {
        private readonly double _value;

        public DoubleValue(double value) => _value = value;
        public override VariableType Type => VariableType.Double;
        public override object RawValue => _value;

        public override int ToInt() => (int)_value;
        public override double ToDouble() => _value;
        public override bool ToBool() => Math.Abs(_value) > 1e-10;
        public override string ToValueString() => _value.ToString(CultureInfo.InvariantCulture);
    }
}
