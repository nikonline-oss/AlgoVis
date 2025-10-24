using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator
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
                _ => VariableType.Int
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
                VariableType.Array => CreateArray(type, arraySize),
                _ => 0
            };
        }

        private static Array CreateArray(VariableType elementType, int size)
        {
            if (size <= 0) size = 10;

            return elementType switch
            {
                VariableType.Int => new int[size],
                VariableType.Double => new double[size],
                VariableType.Bool => new bool[size],
                VariableType.String => new string[size],
                _ => new object[size]
            };
        }
    }
}
