using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Core
{
    public class VariableScope : IVariableScope
    {
        private readonly Dictionary<string, VariableValue> _variables = new();
        private readonly IVariableScope _parent;

        public VariableScope(VariableScope parent = null)
        {
            _variables = new Dictionary<string, VariableValue>();
            _parent = parent;
        }

        public VariableScope(IVariableScope parent)
        {
            _parent = parent;
        }

        public object GetElement(string arrayName, int index)
        {
            if(arrayName.Contains("."))
            {
                var obj = Get(arrayName);

                // Обработка разных типов массивов
                switch (obj)
                {
                    case List<VariableValue> list:
                        return list[index];

                    case Array array: // Для любых массивов (int[], string[] и т.д.)
                        return new VariableValue(array.GetValue(index));

                    case IList collection: // Для других коллекций
                        return new VariableValue(collection[index]);

                    default:
                        throw new InvalidOperationException(
                            $"Object '{arrayName}' is not a collection or array"
                        );
                }
            }
            else if (_variables.ContainsKey(arrayName))
            {
                return _variables[arrayName].GetElement(index);
            }

            return _parent?.GetElement(arrayName, index);
        }

        public void SetProperty(string objectName, string propertyName, object value)
        {
            Console.WriteLine($"🔍 SetProperty: {objectName}.{propertyName} = {value}");

            // Получаем объект
            var obj = Get(objectName);

            if (obj is VariableValue variableValue)
            {
                // Если это VariableValue типа Object
                if (variableValue.Type == VariableType.Object)
                {
                    variableValue.SetProperty(propertyName, value);
                    return;
                }
                // Если это обычное значение, но устанавливаем свойство - преобразуем в объект
                else
                {
                    var newObj = new Dictionary<string, VariableValue>
                    {
                        ["value"] = new VariableValue(variableValue.Value),
                        [propertyName] = new VariableValue(value)
                    };
                    _variables[objectName] = new VariableValue(newObj);
                    return;
                }
            }
            else if (obj is Dictionary<string, VariableValue> dict)
            {
                // Прямая работа со словарем
                dict[propertyName] = new VariableValue(value);
                return;
            }

            // Если объект не найден, создаем новый
            var newObject = new Dictionary<string, VariableValue> { [propertyName] = new VariableValue(value) };
            _variables[objectName] = new VariableValue(newObject);
        }

        public object GetProperty(string objectName, string propertyName)
        {
            Console.WriteLine($"🔍 GetProperty: {objectName}.{propertyName}");

            // Получаем объект
            var obj = Get(objectName);

            if (obj is VariableValue variableValue)
            {
                return variableValue.GetProperty(propertyName);
            }
            else if (obj is Dictionary<string, VariableValue> dict)
            {
                if (dict.TryGetValue(propertyName, out var value))
                {
                    return value.Value;
                }
            }

            return 0;
        }
       
        // Новый метод для установки элемента массива
        public void SetElement(string arrayName, int index, object value)
        {
            var nameVarible = arrayName;

            if(arrayName.Contains("."))
            {
                var parts = arrayName.Split('.');
                nameVarible = parts[0];
            }

            if (_variables.ContainsKey(nameVarible))
            {
                _variables[nameVarible].SetElement(index, value);
                return;
            }

            if (_parent != null && _parent.Contains(nameVarible))
            {
                _parent.SetElement(nameVarible, index, value);
                return;
            }

            // Создаем новый массив, если не существует
            var newArray = new List<VariableValue>();
            _variables[nameVarible] = new VariableValue(newArray);
            _variables[nameVarible].SetElement(index, value);
        }

        public Dictionary<string, object> GetAllVariables()
        {
            var result = new Dictionary<string, object>();

            if (_parent != null)
            {
                foreach (var variable in _parent.GetAllVariables())
                {
                    result[variable.Key] = variable.Value;
                }
            }

            foreach (var variable in _variables)
            {
                result[variable.Key] = variable.Value.Value;
            }

            return result;
        }

        private object GetDefaultValue(string name)
        {
            // Для специальных переменных алгоритма возвращаем соответствующие значения по умолчанию
            var specialVariables = new Dictionary<string, object>
            {
                ["last_comparison"] = 0,
                ["array_length"] = 0,
                ["i"] = 0,
                ["j"] = 0,
                ["k"] = 0,
                ["n"] = 0,
                ["swapped"] = false
            };

            if (specialVariables.ContainsKey(name))
                return specialVariables[name];

            // Для переменных, начинающихся с compare_
            if (name.StartsWith("compare_"))
                return 0;

            // По умолчанию возвращаем 0
            return 0;
        }


        private VariableType DetectType(object value)
        {
            return value switch
            {
                int _ => VariableType.Int,
                double _ => VariableType.Double,
                bool _ => VariableType.Bool,
                string _ => VariableType.String,
                Array _ => VariableType.Array,
                _ => VariableType.String
            };
        }
        public object Get(string name)
        {
            Console.WriteLine($"🔍 VariableScope.Get: {name}");

            // Обработка вложенных свойств: obj.prop1.prop2.prop3
            if (name.Contains("."))
            {
                var parts = name.Split('.');
                return GetNestedProperty(parts);
            }

            // Обычная переменная
            return GetSimple(name);
        }

        private object GetNestedProperty(string[] path)
        {
            if (path.Length == 0) return 0;

            Console.WriteLine($"🔍 GetNestedProperty: путь = {string.Join(".", path)}");

            // Начинаем с корневой переменной
            var current = GetSimple(path[0]);

            if (current is not VariableValue rootValue)
            {
                Console.WriteLine($"⚠️ GetNestedProperty: корневая переменная '{path[0]}' не найдена или не VariableValue");
                return 0;
            }

            Console.WriteLine($"🔍 GetNestedProperty: корень найден, тип = {rootValue.Type}");

            // Если путь состоит из одной части, возвращаем значение
            if (path.Length == 1)
            {
                return ExtractValue(rootValue);
            }

            // Проходим по оставшимся частям пути
            VariableValue currentValue = rootValue;
            for (int i = 1; i < path.Length; i++)
            {
                string part = path[i];
                Console.WriteLine($"🔍 GetNestedProperty: часть[{i}] = {part}");

                if (currentValue.Type != VariableType.Object)
                {
                    Console.WriteLine($"⚠️ GetNestedProperty: нельзя получить свойство '{part}' из не-объекта (тип: {currentValue.Type})");
                    return 0;
                }

                var dict = currentValue.Value as Dictionary<string, VariableValue>;
                if (dict == null)
                {
                    Console.WriteLine($"⚠️ GetNestedProperty: внутренняя ошибка - объект не содержит словарь");
                    return 0;
                }

                if (!dict.TryGetValue(part, out currentValue))
                {
                    Console.WriteLine($"⚠️ GetNestedProperty: свойство '{part}' не найдено в объекте");
                    return 0;
                }

                Console.WriteLine($"🔍 GetNestedProperty: свойство '{part}' найдено, тип = {currentValue.Type}");
            }

            var result = ExtractValue(currentValue);
            Console.WriteLine($"🔍 GetNestedProperty: конечный результат = {result}");
            return result;
        }

        public void Set(string name, object value)
        {
            Console.WriteLine($"🔍 VariableScope.Set: {name} = {value}");

            // Обработка вложенных свойств
            if (name.Contains("."))
            {
                var parts = name.Split('.');
                SetNestedProperty(parts, value);
                return;
            }

            // Обычная переменная
            SetSimple(name, value);
        }

        private void SetNestedProperty(string[] path, object value)
        {
            if (path.Length == 0) return;

            Console.WriteLine($"🔍 SetNestedProperty: путь = {string.Join(".", path)}, значение = {value}");

            // Если путь состоит из одной части, просто устанавливаем
            if (path.Length == 1)
            {
                SetSimple(path[0], value);
                return;
            }

            // Получаем или создаем корневой объект
            var root = GetSimple(path[0]);
            VariableValue currentValue;

            if (root is VariableValue existingValue && existingValue.Type == VariableType.Object)
            {
                currentValue = existingValue;
            }
            else
            {
                // Создаем новый объект
                currentValue = new VariableValue(new Dictionary<string, VariableValue>());
                SetSimple(path[0], currentValue);
            }

            // Проходим по пути, создавая промежуточные объекты при необходимости
            for (int i = 1; i < path.Length - 1; i++)
            {
                string part = path[i];
                var dict = currentValue.Value as Dictionary<string, VariableValue>;

                if (!dict.TryGetValue(part, out var nextValue))
                {
                    // Создаем промежуточный объект
                    nextValue = new VariableValue(new Dictionary<string, VariableValue>());
                    dict[part] = nextValue;
                }

                currentValue = nextValue;

                // Убеждаемся, что текущее значение - объект
                if (currentValue.Type != VariableType.Object)
                {
                    // Преобразуем в объект
                    var newDict = new Dictionary<string, VariableValue> { ["value"] = new VariableValue(currentValue.Value) };
                    currentValue.Value = newDict;
                    currentValue.Type = VariableType.Object;
                }
            }

            // Устанавливаем конечное значение
            string finalPart = path[path.Length - 1];
            var finalDict = currentValue.Value as Dictionary<string, VariableValue>;
            finalDict[finalPart] = new VariableValue(value);

            Console.WriteLine($"✅ SetNestedProperty: значение установлено");
        }

        private object GetSimple(string name)
        {
            if (_variables.ContainsKey(name))
            {
                var result = _variables[name];
                Console.WriteLine($"🔍 GetSimple: {name} = {result.Value}, тип = {result.Type}");
                return result;
            }

            if (_parent != null && _parent.Contains(name))
            {
                return _parent.Get(name);
            }

            return GetDefaultValue(name);
        }

        private void SetSimple(string name, object value)
        {
            var variableValue = value as VariableValue ?? new VariableValue(value);
            _variables[name] = variableValue;
            Console.WriteLine($"🔍 SetSimple: {name} = {variableValue.Value}, тип = {variableValue.Type}");
        }

        private object ExtractValue(object value)
        {
            if (value is VariableValue variableValue)
            {
                Console.WriteLine($"🔍 ExtractValue: извлекаем {variableValue.Value} из VariableValue");
                return variableValue.Value;
            }
            Console.WriteLine($"🔍 ExtractValue: значение уже извлечено = {value}");
            return value;
        }

        public bool Contains(string name)
        {
            return _variables.ContainsKey(name) || (_parent?.Contains(name) == true);
        }

    }
}
