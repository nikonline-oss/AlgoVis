using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    public class ArrayValue : VariableValue
    {
        private readonly List<IVariableValue> _items;

        public ArrayValue(IEnumerable<IVariableValue> items = null)
        {
            _items = items?.ToList() ?? new List<IVariableValue>();
        }

        public override VariableType Type => VariableType.Array;
        public override object RawValue => _items;

        public IVariableValue this[int index]
        {
            get => index >= 0 && index < _items.Count ? _items[index] : new NullValue();
            set
            {
                while (_items.Count <= index)
                    _items.Add(new NullValue());
                _items[index] = value ?? new NullValue();
            }
        }

        public int Length => _items.Count;

        public override bool HasProperty(string name) => _arrayProperties.ContainsKey(name);
        public override bool HasMethod(string name) => _arrayMethods.ContainsKey(name);

        public override IVariableValue GetProperty(string name)
        {
            if (_arrayProperties.TryGetValue(name, out var property))
                return property(this);

            return base.GetProperty(name);
        }

        public override IVariableValue CallMethod(string methodName, IVariableValue[] arguments)
        {
            if (_arrayMethods.TryGetValue(methodName, out var method))
                return method(this, arguments);

            return base.CallMethod(methodName, arguments);
        }

        private static readonly Dictionary<string, Func<ArrayValue, IVariableValue>> _arrayProperties = new()
        {
            ["length"] = array => new IntValue(array._items.Count),
            ["len"] = array => new IntValue(array._items.Count),
            ["first"] = array => array._items.Count > 0 ? array._items[0] : new NullValue(),
            ["last"] = array => array._items.Count > 0 ? array._items[^1] : new NullValue(),
            ["isEmpty"] = array => new BoolValue(array._items.Count == 0)
        };

        private static readonly Dictionary<string, Func<ArrayValue, IVariableValue[], IVariableValue>> _arrayMethods = new()
        {
            ["push"] = (self, args) =>
            {
                foreach (var arg in args)
                    self._items.Add(arg ?? new NullValue());
                return new IntValue(self._items.Count);
            },
            ["pop"] = (self, args) =>
            {
                if (self._items.Count == 0) return new NullValue();
                var last = self._items[^1];
                self._items.RemoveAt(self._items.Count - 1);
                return last;
            },
            ["get"] = (self, args) =>
            {
                if (args.Length == 0) return new NullValue();
                var index = args[0].ToInt();
                return self[index];
            },
            ["set"] = (self, args) =>
            {
                if (args.Length < 2) return new BoolValue(false);
                var index = args[0].ToInt();
                self[index] = args[1];
                return new BoolValue(true);
            },
            ["insert"] = (self, args) =>
            {
                if (args.Length < 2) return new BoolValue(false);
                var index = args[0].ToInt();
                var value = args[1];

                if (index >= 0 && index <= self._items.Count)
                {
                    self._items.Insert(index, value);
                    return new BoolValue(true);
                }
                return new BoolValue(false);
            },
            ["remove"] = (self, args) =>
            {
                if (args.Length == 0) return new BoolValue(false);
                var index = args[0].ToInt();

                if (index >= 0 && index < self._items.Count)
                {
                    self._items.RemoveAt(index);
                    return new BoolValue(true);
                }
                return new BoolValue(false);
            },
            ["indexOf"] = (self, args) =>
            {
                if (args.Length == 0) return new IntValue(-1);
                var target = args[0];

                for (int i = 0; i < self._items.Count; i++)
                {
                    if (ValuesEqual(self._items[i], target))
                        return new IntValue(i);
                }
                return new IntValue(-1);
            },
            ["contains"] = (self, args) =>
            {
                if (args.Length == 0) return new BoolValue(false);
                var target = args[0];

                foreach (var item in self._items)
                {
                    if (ValuesEqual(item, target))
                        return new BoolValue(true);
                }
                return new BoolValue(false);
            },
            ["clear"] = (self, args) =>
            {
                self._items.Clear();
                return new BoolValue(true);
            },
            ["slice"] = (self, args) =>
            {
                if (args.Length == 0) return new ArrayValue();

                var start = args[0].ToInt();
                var end = args.Length > 1 ? args[1].ToInt() : self._items.Count;

                start = Math.Max(0, Math.Min(start, self._items.Count));
                end = Math.Max(start, Math.Min(end, self._items.Count));

                var sliced = self._items.Skip(start).Take(end - start).ToList();
                return new ArrayValue(sliced);
            },
            ["reverse"] = (self, args) =>
            {
                self._items.Reverse();
                return new BoolValue(true);
            },
            ["join"] = (self, args) =>
            {
                var separator = args.Length > 0 ? args[0].ToString() : ",";
                var strings = self._items.Select(item => item.ToString());
                return new StringValue(string.Join(separator, strings));
            },
            ["map"] = (self, args) =>
            {
                // Для простоты - преобразуем все элементы в строки
                var strings = self._items.Select(item => new StringValue(item.ToString()));
                return new ArrayValue(strings);
            },
            ["filter"] = (self, args) =>
            {
                if (args.Length == 0) return new ArrayValue(self._items);

                var filtered = self._items.Where(item => item.ToBool()).ToList();
                return new ArrayValue(filtered);
            }
        };

        private static bool ValuesEqual(IVariableValue a, IVariableValue b)
        {
            if (a.Type != b.Type) return false;

            return a.Type switch
            {
                VariableType.Int => a.ToInt() == b.ToInt(),
                VariableType.Double => Math.Abs(a.ToDouble() - b.ToDouble()) < 1e-10,
                VariableType.Bool => a.ToBool() == b.ToBool(),
                VariableType.String => a.ToString() == b.ToString(),
                VariableType.Null => true,
                _ => a.ToString() == b.ToString()
            };
        }

        public override int ToInt() => _items.Count;
        public override double ToDouble() => _items.Count;
        public override bool ToBool() => _items.Count > 0;
        public override string ToValueString() => $"[{string.Join(", ", _items.Select(item=>item.ToValueString()))}]";

        // Методы для удобной работы из кода
        public void Add(IVariableValue value) => _items.Add(value);
        public void Insert(int index, IVariableValue value) => _items.Insert(index, value);
        public void RemoveAt(int index) => _items.RemoveAt(index);
        public bool Contains(IVariableValue value) => _items.Any(item => ValuesEqual(item, value));
        public int IndexOf(IVariableValue value)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (ValuesEqual(_items[i], value))
                    return i;
            }
            return -1;
        }

        // Статические методы для создания массивов
        public static ArrayValue CreateIntArray(params int[] values)
        {
            return new ArrayValue(values.Select(v => new IntValue(v) as IVariableValue));
        }

        public static ArrayValue CreateDoubleArray(params double[] values)
        {
            return new ArrayValue(values.Select(v => new DoubleValue(v) as IVariableValue));
        }

        public static ArrayValue CreateStringArray(params string[] values)
        {
            return new ArrayValue(values.Select(v => new StringValue(v) as IVariableValue));
        }

        public static ArrayValue CreateBoolArray(params bool[] values)
        {
            return new ArrayValue(values.Select(v => new BoolValue(v) as IVariableValue));
        }

        public static ArrayValue CreateFromObjects(params object[] values)
        {
            var items = new List<IVariableValue>();
            foreach (var value in values)
            {
                items.Add(value switch
                {
                    null => new NullValue(),
                    int i => new IntValue(i),
                    double d => new DoubleValue(d),
                    bool b => new BoolValue(b),
                    string s => new StringValue(s),
                    IVariableValue v => v,
                    _ => new StringValue(value.ToString())
                });
            }
            return new ArrayValue(items);
        }

        public T[] ToArray<T>(Func<IVariableValue, T> converter)
        {
            var result = new T[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                result[i] = converter(_items[i]);
            }
            return result;
        }

        // Статические методы для создания массивов с объектами
        public static ArrayValue CreateObjectArray(params Dictionary<string, object>[] objects)
        {
            var items = new List<IVariableValue>();
            foreach (var obj in objects)
            {
                var objectValue = new ObjectValue();
                foreach (var prop in obj)
                {
                    objectValue.SetProperty(prop.Key, ConvertToVariableValue(prop.Value));
                }
                items.Add(objectValue);
            }
            return new ArrayValue(items);
        }

        public static ArrayValue CreateFromJsonArray(string jsonArray)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonArray);
                return ParseJsonArray(document.RootElement);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON array: {ex.Message}", ex);
            }
        }

        private static ArrayValue ParseJsonArray(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
                throw new ArgumentException("JSON element is not an array");

            var items = new List<IVariableValue>();
            foreach (var element in jsonElement.EnumerateArray())
            {
                items.Add(ParseJsonElement(element));
            }
            return new ArrayValue(items);
        }

        private static IVariableValue ParseJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => ParseJsonObject(element),
                JsonValueKind.Array => ParseJsonArray(element),
                JsonValueKind.String => new StringValue(element.GetString()),
                JsonValueKind.Number => element.TryGetInt32(out int intVal)
                    ? new IntValue(intVal)
                    : new DoubleValue(element.GetDouble()),
                JsonValueKind.True => new BoolValue(true),
                JsonValueKind.False => new BoolValue(false),
                JsonValueKind.Null => new NullValue(),
                _ => new StringValue(element.ToString())
            };
        }

        private static ObjectValue ParseJsonObject(JsonElement jsonObject)
        {
            var properties = new Dictionary<string, IVariableValue>();
            foreach (var property in jsonObject.EnumerateObject())
            {
                properties[property.Name] = ParseJsonElement(property.Value);
            }
            return new ObjectValue(properties);
        }

        private static IVariableValue ConvertToVariableValue(object value)
        {
            return value switch
            {
                null => new NullValue(),
                int i => new IntValue(i),
                double d => new DoubleValue(d),
                bool b => new BoolValue(b),
                string s => new StringValue(s),
                IVariableValue v => v,
                Dictionary<string, object> dict => ConvertDictionaryToObjectValue(dict),
                _ => new StringValue(value.ToString())
            };
        }

        private static ObjectValue ConvertDictionaryToObjectValue(Dictionary<string, object> dict)
        {
            var properties = new Dictionary<string, IVariableValue>();
            foreach (var kvp in dict)
            {
                properties[kvp.Key] = ConvertToVariableValue(kvp.Value);
            }
            return new ObjectValue(properties);
        }
    }
}
