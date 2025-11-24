using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;


namespace testing.Models.Steps
{
    public class StepExecutor : IStepExecutor
    {
        private const int MAX_STEPS = 10000;
        private const int MAX_CALL_DEPTH = 100;

        public void Execute(string stepId, ExecutionContext context)
        {
            if (string.IsNullOrEmpty(stepId)) return;

            // Защита от бесконечных циклов
            if (context.StepHistory.GetStepCount(stepId) > MAX_STEPS)
            {
                throw new InvalidOperationException($"Превышено максимальное количество выполнений шага: {stepId}");
            }

            if (context.FunctionStack.CurrentDepth > MAX_CALL_DEPTH)
            {
                throw new InvalidOperationException($"Превышена максимальная глубина вызовов: {MAX_CALL_DEPTH}");
            }

            context.StepHistory.RecordStep(stepId);

            var step = FindStep(stepId, context.Request);
            if (step == null)
            {
                throw new InvalidOperationException($"Шаг '{stepId}' не найден");
            }

            ExecuteStep(step, context);
        }

        private void ExecuteStep(AlgorithmStep step, ExecutionContext context)
        {
            Console.WriteLine($"Выполнение шага: {step.id}, тип: {step.type}");

            try
            {
                context.OperationExecutor.Execute(step, context);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка выполнения шага '{step.id}': {ex.Message}", ex);
            }
        }

        private AlgorithmStep FindStep(string stepId, CustomAlgorithmRequest request)
        {
            // Поиск в основных шагах
            var step = request.steps.FirstOrDefault(s => s.id == stepId);
            if (step != null) return step;

            // Поиск в функциях
            if (request.functions != null)
            {
                foreach (var function in request.functions)
                {
                    step = function.steps.FirstOrDefault(s => s.id == stepId);
                    if (step != null) return step;
                }
            }

            return null;
        }
    }
}
