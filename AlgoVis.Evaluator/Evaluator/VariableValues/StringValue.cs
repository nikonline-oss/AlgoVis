using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    public class StringValue : VariableValue
    {
        private readonly string _value;

        public StringValue(string value) => _value = value ?? string.Empty;
        public override VariableType Type => VariableType.String;
        public override object RawValue => _value;

        public override bool ToBool() => !string.IsNullOrEmpty(_value);
        public override double ToDouble() => double.TryParse(_value, out double result) ? result : 0;
        public override int ToInt() => int.TryParse(_value, out int result) ? result : 0;
        public override string ToValueString() => _value;

        private static readonly Dictionary<string, Func<StringValue, IVariableValue[], IVariableValue>> _methods = new()
        {
            ["toUpper"] = (self, args) => new StringValue(self._value.ToUpper()),
            ["toLower"] = (self, args) => new StringValue(self._value.ToLower()),
            ["substring"] = (self, args) =>
            {
                if (args.Length == 1)
                    return new StringValue(self._value.Substring(args[0].ToInt()));
                if (args.Length == 2)
                    return new StringValue(self._value.Substring(args[0].ToInt(), args[1].ToInt()));
                throw new("substring requires 1 or 2 arguments");
            },
            ["contains"] = (self, args) =>
                new BoolValue(self._value.Contains(args[0].ToString()))
        };

        public override bool HasProperty(string name) => name == "length";
        public override bool HasMethod(string name) => _methods.ContainsKey(name);

        public override IVariableValue GetProperty(string name)
        {
            return name switch
            {
                "length" => new IntValue(_value.Length),
                _ => base.GetProperty(name)
            };
        }

        public override IVariableValue CallMethod(string methodName, IVariableValue[] arguments)
        {
            if (_methods.TryGetValue(methodName, out var method))
                return method(this, arguments);

            return base.CallMethod(methodName, arguments);
        }
    }
}
