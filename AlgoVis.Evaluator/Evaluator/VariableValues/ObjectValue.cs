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
    public class ObjectValue : VariableValue
    {
        private readonly Dictionary<string, IVariableValue> _properties;

        public ObjectValue(Dictionary<string, IVariableValue> properties = null)
        {
            _properties = properties ?? new Dictionary<string, IVariableValue>();
        }

        public override VariableType Type => VariableType.Object;
        public override object RawValue => _properties;

        public override bool HasProperty(string name) => _properties.ContainsKey(name);

        public override IVariableValue GetProperty(string name)
        {
            if (_properties.TryGetValue(name, out var value))
                return value;

            // Автоматическое создание свойства при обращении
            var newValue = new NullValue();
            _properties[name] = newValue;
            return newValue;
        }

        public override void SetProperty(string name, IVariableValue value)
        {
            _properties[name] = value ?? new NullValue();
        }

        public override IVariableValue CallMethod(string methodName, IVariableValue[] arguments)
        {
            return methodName.ToLower() switch
            {
                "keys" => GetKeys(),
                "values" => GetValues(),
                "has" => HasPropertyMethod(arguments),
                "get" => GetPropertyMethod(arguments),
                "set" => SetPropertyMethod(arguments),
                "remove" => RemoveProperty(arguments),
                "toString" => ToStringMethod(),
                "toJSON" => ToJsonMethod(),
                _ => base.CallMethod(methodName, arguments)
            };
        }

        private IVariableValue GetKeys()
        {
            var keys = _properties.Keys.Select(k => new StringValue(k)).ToArray();
            return new ArrayValue(keys);
        }

        private IVariableValue GetValues()
        {
            return new ArrayValue(_properties.Values.ToList());
        }

        private IVariableValue HasPropertyMethod(IVariableValue[] args)
        {
            if (args.Length == 0) return new BoolValue(false);
            return new BoolValue(HasProperty(args[0].ToString()));
        }

        private IVariableValue GetPropertyMethod(IVariableValue[] args)
        {
            if (args.Length == 0) return new NullValue();
            return GetProperty(args[0].ToString());
        }

        private IVariableValue SetPropertyMethod(IVariableValue[] args)
        {
            if (args.Length < 2) return new BoolValue(false);
            SetProperty(args[0].ToString(), args[1]);
            return new BoolValue(true);
        }

        private IVariableValue RemoveProperty(IVariableValue[] args)
        {
            if (args.Length == 0) return new BoolValue(false);
            return new BoolValue(_properties.Remove(args[0].ToString()));
        }

        private IVariableValue ToStringMethod()
        {
            return new StringValue(ToString());
        }

        private IVariableValue ToJsonMethod()
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(ToSerializableDictionary());
                return new StringValue(json);
            }
            catch
            {
                return new StringValue("{}");
            }
        }

        private Dictionary<string, object> ToSerializableDictionary()
        {
            var result = new Dictionary<string, object>();
            foreach (var prop in _properties)
            {
                result[prop.Key] = ConvertToSerializable(prop.Value);
            }
            return result;
        }

        private object ConvertToSerializable(IVariableValue value) => value switch
        {
            IntValue intVal => intVal.RawValue,
            DoubleValue doubleVal => doubleVal.RawValue,
            BoolValue boolVal => boolVal.RawValue,
            StringValue stringVal => stringVal.RawValue,
            NullValue => null,
            ArrayValue arrayVal => ConvertArrayToSerializable(arrayVal),
            ObjectValue objVal => objVal.ToSerializableDictionary(),
            _ => value.ToString()
        };

        private List<object> ConvertArrayToSerializable(ArrayValue array)
        {
            var result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add(ConvertToSerializable(array[i]));
            }
            return result;
        }

        public void SetNestedProperty(string path, IVariableValue value)
        {
            var parts = path.Split('.');
            IVariableValue current = this;

            // Проходим по всем частям пути, кроме последней
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var next = current.GetProperty(parts[i]);
                if (next is not ObjectValue)
                {
                    // Если следующий элемент не объект, создаем новый объект
                    var newObj = new ObjectValue();
                    current.SetProperty(parts[i], newObj);
                    current = newObj;
                }
                else
                {
                    current = next;
                }
            }

            // Устанавливаем финальное значение
            current.SetProperty(parts[^1], value);
        }

        public override int ToInt() => _properties.Count;
        public override double ToDouble() => _properties.Count;
        public override bool ToBool() => _properties.Count > 0;
        public override string ToValueString() => $"{{{string.Join(", ", _properties.Select(kv => $"{kv.Key}: {kv.Value}"))}}}";
    }
}
