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
        public List<ConditionDefinition> conditions { get; set; } = new();

        public List<FunctionGroup> functions { get; set; } = new(); // Новое!
    }
    //группа шагов
    public class FunctionGroup
    {
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public List<string> parameters { get; set; } = new(); // Параметры функции
        public List<AlgorithmStep> steps { get; set; } = new();
        public string entryPoint { get; set; } = "start"; // Шаг возврата
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
        // Новые поля для вызова функций
        public string functionName { get; set; } = string.Empty;
        public Dictionary<string, string> functionParameters { get; set; } = new();
        public string returnToStep { get; set; } = string.Empty;
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

    // Новая модель контекста вызова
    public class FunctionContext
    {
        public string callerStepId { get; set; } = string.Empty;
        public Dictionary<string, object> variables { get; set; } = new Dictionary<string, object>();
        public int depth { get; set; }
        public string functionName { get; set; } = string.Empty;

    }
}
