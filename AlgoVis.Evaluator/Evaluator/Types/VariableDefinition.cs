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
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = "int";
        public object initialValue { get; set; } = "0";
        public string describtion {  get; set; } = string.Empty;
    }
}
