using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator
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
            if (_variables.ContainsKey(arrayName))
            {
                return _variables[arrayName].GetElement(index);
            }

            return _parent?.GetElement(arrayName, index);
        }

        public object GetProperty(string objectName, string propertyName)
        {
            if (_variables.ContainsKey(objectName))
            {
                return _variables[objectName].GetProperty(propertyName);
            }

            return _parent?.GetProperty(objectName, propertyName);
        }

        public object Get(string name)
        {
            // Проверяем доступ к свойствам объекта: obj.property
            if (name.Contains("."))
            {
                var parts = name.Split('.');
                var objName = parts[0];
                var propName = parts[1];

                if (_variables.ContainsKey(objName))
                {
                    return _variables[objName].GetProperty(propName);
                }

                return _parent?.GetProperty(objName, propName);
            }

            // Обычная переменная
            if (_variables.ContainsKey(name))
            {
                return _variables[name].Value;
            }

            if (_parent != null && _parent.Contains(name))
            {
                return _parent.Get(name);
            }

            return GetDefaultValue(name);
        }

        public void Set(string name, object value)
        {
            var variableValue = value as VariableValue ?? new VariableValue(value);

            // Проверяем установку свойства объекта: obj.property
            if (name.Contains("."))
            {
                var parts = name.Split('.');
                var objName = parts[0];
                var propName = parts[1];

                if (_variables.ContainsKey(objName))
                {
                    _variables[objName].SetProperty(propName, variableValue.Value);
                    return;
                }

                if (_parent != null && _parent.Contains(objName))
                {
                    // Для родительской области нужно специальное решение
                    throw new InvalidOperationException($"Нельзя устанавливать свойства объектов в родительской области: {objName}");
                }

                // Создаем новый объект
                var newObj = new Dictionary<string, VariableValue> { [propName] = variableValue };
                _variables[objName] = new VariableValue(newObj);
                return;
            }

            // Обычная переменная
            if (_variables.ContainsKey(name))
            {
                _variables[name] = variableValue;
                return;
            }

            if (_parent != null && _parent.Contains(name))
            {
                _parent.Set(name, variableValue.Value);
                return;
            }

            _variables[name] = variableValue;
        }

        // Новый метод для установки элемента массива
        public void SetElement(string arrayName, int index, object value)
        {
            if (_variables.ContainsKey(arrayName))
            {
                _variables[arrayName].SetElement(index, value);
                return;
            }

            if (_parent != null && _parent.Contains(arrayName))
            {
                _parent.SetElement(arrayName, index, value);
                return;
            }

            // Создаем новый массив, если не существует
            var newArray = new List<VariableValue>();
            _variables[arrayName] = new VariableValue(newArray);
            _variables[arrayName].SetElement(index, value);
        }

        public void SetProperty(string objectName, string propertyName, object value)
        {
            if (_variables.ContainsKey(objectName))
            {
                _variables[objectName].SetProperty(propertyName, value);
                return;
            }

            if (_parent != null && _parent.Contains(objectName))
            {
                _parent.SetProperty(objectName, propertyName, value);
                return;
            }

            // Создаем новый объект
            var newObj = new Dictionary<string, VariableValue> { [propertyName] = new VariableValue(value) };
            _variables[objectName] = new VariableValue(newObj);
        }

        public bool Contains(string name)
        {
            if (name.Contains("."))
            {
                var parts = name.Split('.');
                var objName = parts[0];
                return _variables.ContainsKey(objName) || _parent?.Contains(objName) == true;
            }

            return _variables.ContainsKey(name) || _parent?.Contains(name) == true;
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
    }
}
