using AlgoVis.Core.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Operations.Base;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;


namespace AlgoVis.Models.Models.Operations.Handlers
{
    // Операция обмена
    public class SwapOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            context.Statistics.Swaps++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Swap operation requires 2 parameters");

            // Получаем имя массива и индексы
            string arrayName = step.parameters.Count > 2 ? step.parameters[0] : "struct";
            var index1Value = EvaluateExpression(step.parameters[step.parameters.Count > 2 ? 1 : 0].ToLower(), context);
            var index2Value = EvaluateExpression(step.parameters[step.parameters.Count > 2 ? 2 : 1].ToLower(), context);

            int index1 = index1Value.ToInt();
            int index2 = index2Value.ToInt();

            // Получаем массив из переменных
            var arrayValue = context.Variables.Get(arrayName) as ArrayValue;

            if (context.Variables.Get(arrayName).HasProperty("values"))
                arrayValue = context.Variables.Get(arrayName).GetProperty("values") as ArrayValue;

            if (arrayValue == null)
                throw new InvalidOperationException($"Массив '{arrayName}' не найден или не является массивом");

            // Проверяем индексы
            if (index1 < 0 || index1 >= arrayValue.Length)
                throw new IndexOutOfRangeException($"Index {index1} is out of range for array of length {arrayValue.Length}");

            if (index2 < 0 || index2 >= arrayValue.Length)
                throw new IndexOutOfRangeException($"Index {index2} is out of range for array of length {arrayValue.Length}");

            // Выполняем обмен в ArrayValue
            var temp = arrayValue[index1];
            arrayValue[index1] = arrayValue[index2];
            arrayValue[index2] = temp;

            // Обновляем состояние структуры, если нужно
            context.Variables.Set(arrayName, arrayValue);

            if (arrayName == "struct")
                FromArrayValue(arrayValue, context);


            AddVisualizationStep(step, context, "swap",
                step.description ?? $"Обмен элементов {arrayName}[{index1}] и {arrayName}[{index2}]",
                new List<HighlightedElement>
                {
                    new() {
                        ElementId = $"{arrayName}[{index1}]",
                        HighlightType = "swapping",
                        Color = "red"
                    },
                    new() {
                        ElementId = $"{arrayName}[{index2}]",
                        HighlightType = "swapping",
                        Color = "red"
                    }
                },
                new Dictionary<string, object>
                {
                    ["array_name"] = arrayName,
                    ["index1"] = index1,
                    ["index2"] = index2,
                    ["value1"] = arrayValue[index1].RawValue,
                    ["value2"] = arrayValue[index2].RawValue
                });

            ExecuteNextStep(step, context);
        }
        /// <summary>
        /// Обновляет состояние ArrayStructure из ArrayValue через конвертер
        /// </summary>
        public void FromArrayValue(ArrayValue arrayValue, ExecutionContext context)
        {
            if (arrayValue == null)
                throw new ArgumentNullException(nameof(arrayValue));

            // Создаем ObjectValue с данными массива
            var obj = new ObjectValue(new Dictionary<string, IVariableValue>
            {
                ["values"] = arrayValue
            });

            var converter = new UniversalStructureConverter();
            var newStructure = converter.ConvertFromVariableValue(obj, "array");

            if (newStructure is ArrayStructure newArrayStructure)
            {
                // Копируем состояние из нового ArrayStructure
                context.Structure.ApplyState(newArrayStructure.GetState());
            }
        }

    }
}
