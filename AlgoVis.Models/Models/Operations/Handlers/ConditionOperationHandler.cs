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
    // Обработчик условий
    public class ConditionOperationHandler : BaseOperationHandler
    {
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
        private string? GetNextStepFromCondition(AlgorithmStep step, bool conditionResult)
        {
            var targetCondition = conditionResult ? "true" : "false";
            return step.conditionCases?
                .FirstOrDefault(c => c.condition == targetCondition)?.nextStep;
        }

    }
}
