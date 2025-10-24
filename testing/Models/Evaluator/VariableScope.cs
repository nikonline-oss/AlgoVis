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

        public object Get(string name)
        {
            // Сначала ищем в текущей области
            if (_variables.ContainsKey(name))
            {
                return _variables[name].Value;
            }

            // Затем в родительской области
            if (_parent != null && _parent.Contains(name))
            {
                return _parent.Get(name);
            }

            // Если переменная не найдена, возвращаем значение по умолчанию
            return GetDefaultValue(name);
        }

        public void Set(string name, object value)
        {

            var result = value as VariableValue;
            // Если переменная уже существует в текущей области, обновляем ее
            if (_variables.ContainsKey(name))
            {
                _variables[name].Value = result is null ? value : result;
                return;
            }

            // Если переменная существует в родительской области, обновляем там
            if (_parent != null && _parent.Contains(name))
            {
                _parent.Set(name, result is null ? value : result);
                return;
            }

            // Иначе создаем новую переменную в текущей области
            _variables[name] = new VariableValue(result is null ? DetectType(value) : result.Type, result is null ? value : result.Value);
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

            throw new ArgumentException($"Массив '{arrayName}' не найден");
        }

        public bool Contains(string name) => _variables.ContainsKey(name) || _parent?.Contains(name) == true;

        public Dictionary<string, object> GetAllVariables()
        {
            var result = new Dictionary<string, object>();

            // Сначала добавляем переменные из родительской области
            if (_parent != null)
            {
                foreach (var variable in _parent.GetAllVariables())
                {
                    result[variable.Key] = variable.Value;
                }
            }

            // Затем перезаписываем переменными из текущей области
            foreach (var variable in _variables)
            {
                result[variable.Key] = variable.Value;
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
