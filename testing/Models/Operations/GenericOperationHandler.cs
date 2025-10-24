using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;


namespace testing.Models.Operations
{
    // Обработчик общих операций
    public class GenericOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            step.visualize = true;

            AddVisualizationStep(step,context,
                step.operation ?? step.type,
                step.description ?? "Выполнение операции",
                metadata: step.metadata ?? new Dictionary<string, object>()
            );

            if (!string.IsNullOrEmpty(step.nextStep))
            {
                context.OperationExecutor.Execute(FindStep(step.nextStep, context.Request), context);
            }
        }

    }
}
