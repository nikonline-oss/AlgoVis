using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Types
{
    //public enum VariableType
    //{
    //    Int,
    //    Double,
    //    Bool,
    //    String,
    //    Array,
    //    Object
    //}

    public class VariableDefinition
    {
        internal int arraySize;

        public string name { get; set; } = string.Empty;
        public string type { get; set; } = "int";
        public object initialValue { get; set; } = "0";

        public Dictionary<string, object> ObjectProperties { get; set; } = new();

        // Новое свойство для динамической инициализации
        public string initialization { get; set; } = string.Empty;
    }
}
