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
    /// <summary>
    /// Представляет массив значений.
    /// </summary>
    /// <remarks>
    /// Этот класс реализует динамический массив со свойствами и методами, аналогичными JavaScript.
    /// Поддерживает индексацию, добавление/удаление элементов и различные операции с массивами.
    /// Также предоставляет статические методы для создания массивов из различных источников.
    /// </remarks>
    public class ArrayValue : VariableValue
    {
        private readonly List<IVariableValue> _items;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ArrayValue"/>.
        /// </summary>
        /// <param name="items">
        /// Начальные элементы массива. Если <c>null</c>, создается пустой список.
        /// </param>
        public ArrayValue(IEnumerable<IVariableValue> items = null)
        {
            _items = items?.ToList() ?? new List<IVariableValue>();
        }

        /// <summary>
        /// Получает тип значения - <see cref="VariableType.Array"/>.
        /// </summary>
        public override VariableType Type => VariableType.Array;

        /// <summary>
        /// Получает необработанное значение в виде списка элементов.
        /// </summary>
        public override object RawValue => _items;

        /// <summary>
        /// Индексатор для доступа к элементам массива.
        /// </summary>
        /// <param name="index">Индекс элемента (начинается с 0).</param>
        /// <returns>
        /// Значение элемента или <see cref="NullValue"/>, если индекс вне диапазона.
        /// </returns>
        /// <remarks>
        /// При установке значения, если индекс превышает текущий размер массива,
        /// массив автоматически расширяется с добавлением <see cref="NullValue"/>.
        /// </remarks>
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

        /// <summary>
        /// Получает количество элементов в массиве.
        /// </summary>
        public int Length => _items.Count;

        /// <summary>
        /// Определяет, поддерживает ли массив указанное свойство.
        /// </summary>
        /// <param name="name">Имя свойства для проверки.</param>
        /// <returns>
        /// <c>true</c>, если свойство существует в <see cref="_arrayProperties"/>; в противном случае <c>false</c>.
        /// </returns>
        public override bool HasProperty(string name) => _arrayProperties.ContainsKey(name);

        /// <summary>
        /// Определяет, поддерживает ли массив указанный метод.
        /// </summary>
        /// <param name="name">Имя метода для проверки.</param>
        /// <returns>
        /// <c>true</c>, если метод существует в <see cref="_arrayMethods"/>; в противном случае <c>false</c>.
        /// </returns>
        public override bool HasMethod(string name) => _arrayMethods.ContainsKey(name);

        /// <summary>
        /// Получает значение свойства массива.
        /// </summary>
        /// <param name="name">Имя свойства для получения.</param>
        /// <returns>
        /// Значение свойства или результат базовой реализации, если свойство не поддерживается.
        /// </returns>
        public override IVariableValue GetProperty(string name)
        {
            if (_arrayProperties.TryGetValue(name, out var property))
                return property(this);

            return base.GetProperty(name);
        }

        /// <summary>
        /// Вызывает метод массива.
        /// </summary>
        /// <param name="methodName">Имя вызываемого метода.</param>
        /// <param name="arguments">Аргументы метода.</param>
        /// <returns>
        /// Результат выполнения метода или исключение, если метод не поддерживается.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, когда метод не найден в словаре <see cref="_arrayMethods"/>.
        /// </exception>
        public override IVariableValue CallMethod(string methodName, IVariableValue[] arguments)
        {
            if (_arrayMethods.TryGetValue(methodName, out var method))
                return method(this, arguments);

            return base.CallMethod(methodName, arguments);
        }

        /// <summary>
        /// Словарь свойств массива.
        /// </summary>
        /// <remarks>
        /// Ключ - имя свойства, значение - делегат для получения значения свойства.
        /// </remarks>
        private static readonly Dictionary<string, Func<ArrayValue, IVariableValue>> _arrayProperties = new()
        {
            ["length"] = array => new IntValue(array._items.Count),
            ["len"] = array => new IntValue(array._items.Count),
            ["first"] = array => array._items.Count > 0 ? array._items[0] : new NullValue(),
            ["last"] = array => array._items.Count > 0 ? array._items[^1] : new NullValue(),
            ["isEmpty"] = array => new BoolValue(array._items.Count == 0)
        };

        /// <summary>
        /// Словарь методов массива.
        /// </summary>
        /// <remarks>
        /// Ключ - имя метода, значение - делегат для выполнения метода.
        /// </remarks>
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

        /// <summary>
        /// Сравнивает два значения переменных на равенство.
        /// </summary>
        /// <param name="a">Первое значение для сравнения.</param>
        /// <param name="b">Второе значение для сравнения.</param>
        /// <returns>
        /// <c>true</c>, если значения равны; в противном случае <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Для числовых типов учитывается точность сравнения чисел с плавающей запятой.
        /// </remarks>
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

        /// <summary>
        /// Преобразует массив в целое число (количество элементов).
        /// </summary>
        public override int ToInt() => _items.Count;

        /// <summary>
        /// Преобразует массив в число с плавающей запятой (количество элементов).
        /// </summary>
        public override double ToDouble() => _items.Count;

        /// <summary>
        /// Преобразует массив в логическое значение.
        /// </summary>
        /// <returns>
        /// <c>true</c>, если массив содержит хотя бы один элемент; в противном случае <c>false</c>.
        /// </returns>
        public override bool ToBool() => _items.Count > 0;

        /// <summary>
        /// Возвращает строковое представление массива в формате "[element1, element2, ...]".
        /// </summary>
        public override string ToValueString() => $"[{string.Join(", ", _items.Select(item => item.ToValueString()))}]";

        // Методы для удобной работы из кода

        /// <summary>
        /// Добавляет элемент в конец массива.
        /// </summary>
        /// <param name="value">Значение для добавления.</param>
        public void Add(IVariableValue value) => _items.Add(value);

        /// <summary>
        /// Вставляет элемент в указанную позицию массива.
        /// </summary>
        /// <param name="index">Позиция для вставки (начинается с 0).</param>
        /// <param name="value">Значение для вставки.</param>
        public void Insert(int index, IVariableValue value) => _items.Insert(index, value);

        /// <summary>
        /// Удаляет элемент по указанному индексу.
        /// </summary>
        /// <param name="index">Индекс элемента для удаления.</param>
        public void RemoveAt(int index) => _items.RemoveAt(index);

        /// <summary>
        /// Определяет, содержит ли массив указанное значение.
        /// </summary>
        /// <param name="value">Значение для поиска.</param>
        /// <returns>
        /// <c>true</c>, если значение найдено; в противном случае <c>false</c>.
        /// </returns>
        public bool Contains(IVariableValue value) => _items.Any(item => ValuesEqual(item, value));

        /// <summary>
        /// Находит индекс первого вхождения указанного значения.
        /// </summary>
        /// <param name="value">Значение для поиска.</param>
        /// <returns>
        /// Индекс значения или -1, если значение не найдено.
        /// </returns>
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

        /// <summary>
        /// Создает массив целых чисел.
        /// </summary>
        /// <param name="values">Целочисленные значения для массива.</param>
        /// <returns>Новый массив с указанными целочисленными значениями.</returns>
        public static ArrayValue CreateIntArray(params int[] values)
        {
            return new ArrayValue(values.Select(v => new IntValue(v) as IVariableValue));
        }

        /// <summary>
        /// Создает массив чисел с плавающей запятой.
        /// </summary>
        /// <param name="values">Числовые значения для массива.</param>
        /// <returns>Новый массив с указанными числовыми значениями.</returns>
        public static ArrayValue CreateDoubleArray(params double[] values)
        {
            return new ArrayValue(values.Select(v => new DoubleValue(v) as IVariableValue));
        }

        /// <summary>
        /// Создает массив строк.
        /// </summary>
        /// <param name="values">Строковые значения для массива.</param>
        /// <returns>Новый массив с указанными строковыми значениями.</returns>
        public static ArrayValue CreateStringArray(params string[] values)
        {
            return new ArrayValue(values.Select(v => new StringValue(v) as IVariableValue));
        }

        /// <summary>
        /// Создает массив логических значений.
        /// </summary>
        /// <param name="values">Логические значения для массива.</param>
        /// <returns>Новый массив с указанными логическими значениями.</returns>
        public static ArrayValue CreateBoolArray(params bool[] values)
        {
            return new ArrayValue(values.Select(v => new BoolValue(v) as IVariableValue));
        }

        /// <summary>
        /// Создает массив из произвольных объектов.
        /// </summary>
        /// <param name="values">Объекты для преобразования в значения переменных.</param>
        /// <returns>Новый массив с преобразованными значениями.</returns>
        /// <remarks>
        /// Поддерживает преобразование типов: null, int, double, bool, string, IVariableValue.
        /// Остальные типы преобразуются в строки.
        /// </remarks>
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

        /// <summary>
        /// Преобразует массив в массив указанного типа с помощью функции преобразования.
        /// </summary>
        /// <typeparam name="T">Целевой тип элементов.</typeparam>
        /// <param name="converter">Функция преобразования значения переменной в тип T.</param>
        /// <returns>Массив элементов типа T.</returns>
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

        /// <summary>
        /// Создает массив объектов из словарей свойств.
        /// </summary>
        /// <param name="objects">Словари свойств для создания объектов.</param>
        /// <returns>Новый массив объектов.</returns>
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

        /// <summary>
        /// Создает массив из JSON-строки.
        /// </summary>
        /// <param name="jsonArray">JSON-строка, представляющая массив.</param>
        /// <returns>Новый массив, созданный из JSON.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда JSON некорректен или не является массивом.
        /// </exception>
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

        /// <summary>
        /// Парсит JSON-массив в объект ArrayValue.
        /// </summary>
        /// <param name="jsonElement">JSON-элемент, представляющий массив.</param>
        /// <returns>Объект ArrayValue с элементами из JSON.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда элемент не является массивом.
        /// </exception>
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

        /// <summary>
        /// Парсит JSON-элемент в значение переменной.
        /// </summary>
        /// <param name="element">JSON-элемент для парсинга.</param>
        /// <returns>Значение переменной соответствующего типа.</returns>
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

        /// <summary>
        /// Парсит JSON-объект в объект ObjectValue.
        /// </summary>
        /// <param name="jsonObject">JSON-элемент, представляющий объект.</param>
        /// <returns>Объект ObjectValue со свойствами из JSON.</returns>
        private static ObjectValue ParseJsonObject(JsonElement jsonObject)
        {
            var properties = new Dictionary<string, IVariableValue>();
            foreach (var property in jsonObject.EnumerateObject())
            {
                properties[property.Name] = ParseJsonElement(property.Value);
            }
            return new ObjectValue(properties);
        }

        /// <summary>
        /// Преобразует объект в значение переменной.
        /// </summary>
        /// <param name="value">Объект для преобразования.</param>
        /// <returns>Значение переменной соответствующего типа.</returns>
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

        /// <summary>
        /// Преобразует словарь в объект ObjectValue.
        /// </summary>
        /// <param name="dict">Словарь свойств.</param>
        /// <returns>Объект ObjectValue со свойствами из словаря.</returns>
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