using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Core;

namespace testing.Models.Custom
{
    public class CustomAlgorithmRequest
    {
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string structureType { get; set; } = "array";
        public List<AlgorithmStep> steps { get; set; } = new();
        public Dictionary<string, object> parametrs{ get; set; } = new();
        public List<VariableDefinition> variables { get; set; } = new();
        public List<LoopDefinition> loops { get; set; } = new();
        public List<ConditionDefinition> conditions { get; set; } = new();
    }
    //Шаг алгоритма
    public class AlgorithmStep
    {
        public string id { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty; // compare, swap, assign, loop, condition
        public string operation { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public List<string> parameters { get; set; } = new();
        public Dictionary<string, object> metadata { get; set; } = new();
        public string nextStep { get; set; } = string.Empty;
        public List<ConditionCase> conditionCases { get; set; } = new();
    }
    //случай условие
    public class ConditionCase
    {
        public string condition {  get; set; } = string.Empty;
        public string nextStep {  get; set; } = string.Empty;
    }
    //определение переменной
    public class VariableDefinition
    {
        public string name { get; set; } = string.Empty ;
        public string type { get; set; } = "int";
        public object initialValue { get; set; } = 0;
    }
    //определение цикла
    public class LoopDefinition
    {
        public string id { get; set; } = string.Empty ;
        public string type { get; set; } = "for"; // for, while
        public string variable { get; set; } = string.Empty;
        public string from { get; set; } = "0";
        public string to { get; set; } = string.Empty;
        public string condition { get; set; } = string.Empty;
        public List<string> steps { get; set; } = new();
    }
    // Определение условия
    public class ConditionDefinition
    {
        public string id { get; set; } = string.Empty;
        public string condition { get; set; } = string.Empty;
        public List<string> trueSteps { get; set; } = new();
        public List<string> falseSteps { get; set; } = new();
    }

    // Результат выполнения кастомного алгоритма
    public class CustomAlgorithmResult
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public AlgorithmResult result { get; set; } = new();
        public Dictionary<string, object> executionState { get; set; } = new();
    }
}
