using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.Operations.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;


namespace AlgoVis.Models.Models.Operations.Handlers
{
    /// <summary>
    /// Обработчик операций условий (ветвлений).
    /// </summary>
    /// <remarks>
    /// Выполняет проверку условия и переходит к соответствующему следующему шагу
    /// в зависимости от результата (true или false).
    /// </remarks>
    public class ConditionOperationHandler : BaseOperationHandler
    {
        /// <summary>
        /// Выполняет операцию проверки условия.
        /// </summary>
        /// <param name="step">Шаг алгоритма с параметрами операции.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если отсутствует параметр с условием.
        /// </exception>
        /// <remarks>
        /// Параметры шага:
        /// - parameters[0]: Условие для проверки (логическое выражение)
        /// - conditionCases: Список случаев с переходом на следующий шаг
        /// </remarks>
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            if (step.parameters.Count == 0)
                throw new ArgumentException("Condition operation requires a condition parameter");

            var condition = step.parameters[0];
            var conditionResult = EvaluateCondition(condition, context);

            AddVisualizationStep(step,context,"condition",
                step.description ?? $"Проверка условия: {condition}",
                metadata: new Dictionary<string, object>
                {
                    ["condition"] = condition,
                    ["result"] = conditionResult
                });

            var nextStep = GetNextStepFromCondition(step, conditionResult);
            if (!string.IsNullOrEmpty(nextStep))
            {
                context.OperationExecutor.Execute(FindStep(nextStep, context.Request), context);
            }
        }
        
        /// <summary>
        /// Определяет следующий шаг на основе результата условия.
        /// </summary>
        /// <param name="step">Шаг алгоритма с определенными случаями перехода.</param>
        /// <param name="conditionResult">Результат вычисления условия (true/false).</param>
        /// <returns>
        /// Идентификатор следующего шага или null, если переход не определен.
        /// </returns>
        private string? GetNextStepFromCondition(AlgorithmStep step, bool conditionResult)
        {
            var targetCondition = conditionResult ? "true" : "false";
            return step.conditionCases?
                .FirstOrDefault(c => c.condition == targetCondition)?.nextStep;
        }

    }
}