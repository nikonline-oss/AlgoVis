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
    /// Обработчик общих операций без специфической логики.
    /// </summary>
    /// <remarks>
    /// Используется для операций, которые не требуют специальной обработки,
    /// но нуждаются в визуализации и переходе к следующему шагу.
    /// </remarks>
    public class GenericOperationHandler : BaseOperationHandler
    {
        /// <summary>
        /// Выполняет общую операцию.
        /// </summary>
        /// <param name="step">Шаг алгоритма для выполнения.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <remarks>
        /// Устанавливает флаг visualize в true, добавляет шаг визуализации
        /// и переходит к следующему шагу, если он указан.
        /// </remarks>
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            step.visualize = true;

            AddVisualizationStep(step, context,
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