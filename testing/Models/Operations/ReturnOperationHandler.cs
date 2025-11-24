using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;


namespace testing.Models.Operations
{
    // Обработчик возврата из функции
    public class ReturnOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            if (context.FunctionStack.Current == null)
            {
                // Нет активных функций - завершаем выполнение
                return;
            }

            var functionContext = context.FunctionStack.Pop();

            AddVisualizationStep(step, context, "return",
                step.description ?? $"Возврат из функции: {functionContext.FunctionName}",
                metadata: new Dictionary<string, object>
                {
                    ["function_name"] = functionContext.FunctionName,
                    ["call_depth"] = context.FunctionStack.CurrentDepth
                });

            // Возвращаемся к шагу после вызова функции
            if (!string.IsNullOrEmpty(functionContext.ReturnStepId))
            {
                context.OperationExecutor.Execute(FindStep(functionContext.ReturnStepId, context.Request), context);
            }
        }

    }
}
