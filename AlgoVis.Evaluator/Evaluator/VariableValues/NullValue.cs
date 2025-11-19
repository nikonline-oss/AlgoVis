using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    public class NullValue : VariableValue
    {
        public override VariableType Type => VariableType.Object;
        public override object RawValue => null;

        public override int ToInt() => 0;
        public override double ToDouble() => 0;
        public override bool ToBool() => false;
        public override string ToValueString() => "null";
    }
}
