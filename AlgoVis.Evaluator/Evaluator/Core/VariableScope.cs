using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
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
        private readonly Dictionary<string, IVariableValue> _variables = new();
        private readonly IVariableScope _parent;

        public VariableScope(IVariableScope parent = null)
        {
            _parent = parent;
        }

        public IVariableValue Get(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name];

            return _parent?.Get(name) ?? new IntValue(0);
        }

        public void Set(string name, IVariableValue value)
        {
            _variables[name] = value ?? new IntValue(0);
        }

        public bool Contains(string name) => _variables.ContainsKey(name);

        public IVariableValue GetNested(string path)
        {
            var parts = path.Split('.');
            IVariableValue current = Get(parts[0]);

            for (int i = 1; i < parts.Length; i++)
            {
                current = current.GetProperty(parts[i]);
            }

            return current;
        }

        public void SetNested(string path, IVariableValue value)
        {
            var parts = path.Split('.');

            if (parts.Length == 1)
            {
                Set(parts[0], value);
                return;
            }

            // Получаем или создаем корневой объект
            var rootName = parts[0];
            var root = Get(rootName);

            if (root is not ObjectValue rootObj)
            {
                rootObj = new ObjectValue();
                Set(rootName, rootObj);
            }

            // Устанавливаем вложенное свойство
            var remainingPath = string.Join(".", parts.Skip(1));
            rootObj.SetNestedProperty(remainingPath, value);
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
                result[variable.Key] = variable.Value.RawValue;
            }

            return result;
        }
    }
}
