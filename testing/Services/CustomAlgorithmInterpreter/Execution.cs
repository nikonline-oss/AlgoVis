using testing.Models.Custom;
using testing.Models.Visualization;

namespace testing.Services
{
    public partial class CustomAlgorithmInterpreter
    {
        /// <summary>
        /// Выполняет шаг алгоритма по идентификатору.
        /// </summary>
        private void ExecuteStep(string stepId)
        {
            if (string.IsNullOrEmpty(stepId)) return;

            if (_currentCallDepth > MAX_CALL_DEPTH)
            {
                throw new InvalidOperationException($"Превышена максимальная глубина рекурсии: {MAX_CALL_DEPTH}");
            }

            // Определяем, выполняем ли мы шаг из основной программы или из функции
            var (step, function) = GetStepAndFunction(stepId);


            if (step == null)
            {
                Console.WriteLine($"❌ Шаг '{stepId}' не найден");
                return;
            }

            Console.WriteLine($" шаг №{step.id}, тип :{step.type}, описание: {step.description}");


            switch (step.type.ToLower())
            {
                case "compare":
                    ExecuteCompare(step);
                    break;
                case "swap":
                    ExecuteSwap(step);
                    break;
                case "assign":
                    ExecuteAssign(step);
                    break;
                case "condition":
                    ExecuteCondition(step, function);
                    break;
                case "call_function":
                    ExecuteFunctionCall(step);
                    break;
                case "return":
                    ExecuteReturn(step);
                    break;
                default:
                    ExecuteGenericStep(step);
                    break;
            }

            if (step.type != "condition" &&
                step.type != "call_function" &&
                step.type != "return")
            {
                string nextStepId = step.nextStep;

                // Если nextStep не указан явно, пытаемся получить следующий шаг по порядку
                if (string.IsNullOrEmpty(nextStepId))
                {
                    nextStepId = GetNextStep(stepId, function);
                }

                if (!string.IsNullOrEmpty(nextStepId))
                {
                    ExecuteStep(nextStepId);
                }
            }
            else if (!string.IsNullOrEmpty(step.nextStep))
            {
                ExecuteStep(step.nextStep);
            }
        }

        // Методы обработки различных типов шагов:
        // ExecuteCompare, ExecuteSwap, ExecuteAssign, ExecuteCondition, ExecuteGenericStep
        private void ExecuteCompare(AlgorithmStep step)
        {
            _statistics.Comparisons++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Compare step requires 2 parameters");

            var index1 = EvaluateExpression(step.parameters[0]);
            var index2 = EvaluateExpression(step.parameters[1]);

            var array = GetArrayState();
            var value1 = array[ConvertToInt(index1)];
            var value2 = array[ConvertToInt(index2)];

            var comparisonResult = value1.CompareTo(value2);
            var description = step.description ?? $"Сравнение [{index1}]={value1} и [{index2}]={value2}";

            AddVisualizationStep("compare", description, new List<HighlightedElement>
            {
                new() { ElementId = index1.ToString(), HighlightType = "comparing", Color = "yellow" },
                new() { ElementId = index2.ToString(), HighlightType = "comparing", Color = "yellow" }
            }, new Dictionary<string, object>
            {
                ["comparison_result"] = comparisonResult,
                ["value1"] = value1,
                ["value2"] = value2
            });

            // Безопасно устанавливаем переменные сравнения
            SetVariableValue("last_comparison", comparisonResult);
            if (!string.IsNullOrEmpty(step.id))
            {
                SetVariableValue($"compare_{step.id}", comparisonResult);
            }
        }
        private void ExecuteSwap(AlgorithmStep step)
        {
            _statistics.Swaps++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Swap step requires 2 parameters");

            var index1 = EvaluateExpression(step.parameters[0]);
            var index2 = EvaluateExpression(step.parameters[1]);

            var array = GetArrayState();
            (array[ConvertToInt(index2)], array[ConvertToInt(index1)]) = (array[ConvertToInt(index1)], array[ConvertToInt(index2)]);

            UpdateArrayState(array);

            var description = step.description ?? $"Обмен элементов [{index1}] и [{index2}]";

            AddVisualizationStep("swap", description, new List<HighlightedElement>
            {
                new() { ElementId = index1.ToString(), HighlightType = "swapping", Color = "red" },
                new() { ElementId = index2.ToString(), HighlightType = "swapping", Color = "red" }
            });
        }
        private void ExecuteAssign(AlgorithmStep step)
        {
            if (step.parameters.Count < 2)
                throw new ArgumentException("Assign step requires 2 parameters (variable, value)");

            var variableName = step.parameters[0];
            var value = EvaluateExpression(step.parameters[1]);

            SetVariableValue(variableName, value);

            var description = step.description ?? $"Присвоение {variableName} = {value}";

            AddVisualizationStep("assign", description, metadata: new Dictionary<string, object>
            {
                ["variable"] = variableName,
                ["value"] = value
            });
        }
        private void ExecuteGenericStep(AlgorithmStep step)
        {
            AddVisualizationStep(step.operation, step.description, metadata: step.metadata);
        }

    }
}