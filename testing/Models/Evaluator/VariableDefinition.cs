using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator
{
    public enum VariableType
    {
        Int,
        Double,
        Bool,
        String,
        Array
    }

    public class VariableDefinition
    {
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = "int";
        public object initialValue { get; set; } = "0";
        public int arraySize { get; set; } = 0; // Для массивов
    }
}
