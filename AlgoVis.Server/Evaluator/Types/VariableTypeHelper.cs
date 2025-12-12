using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Types
{
    public static class VariableTypeHelper
    {
        public static VariableType ParseType(string typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
                return VariableType.Int;

            return typeString.ToLower() switch
            {
                "int" => VariableType.Int,
                "double" => VariableType.Double,
                "bool" => VariableType.Bool,
                "string" => VariableType.String,
                "array" => VariableType.Array,
                "object" => VariableType.Object,
                _ => VariableType.Object // По умолчанию объект для гибкости
            };
        }

        public static object CreateDefaultValue(VariableType type, int arraySize = 0)
        {
            return type switch
            {
                VariableType.Int => 0,
                VariableType.Double => 0.0,
                VariableType.Bool => false,
                VariableType.String => string.Empty,
                VariableType.Array => CreateDynamicArray(),
                VariableType.Object => CreateDynamicObject(),
                _ => CreateDynamicObject()
            };
        }

        private static List<VariableValue> CreateDynamicArray()
        {
            return new List<VariableValue>();
        }

        private static Dictionary<string, VariableValue> CreateDynamicObject()
        {
            return new Dictionary<string, VariableValue>();
        }
    }
}
