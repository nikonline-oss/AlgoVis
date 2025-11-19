using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;

using testing.Models.Evaluator;
using testing.Models.Functions;

namespace testing.Models.Operations
{
    // Обработчик вызова функций
    public class FunctionCallOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            if (string.IsNullOrEmpty(step.functionName))
                throw new ArgumentException("Function call requires a function name");

            var function = context.Request.functions?.FirstOrDefault(f => f.name == step.functionName);
            if (function == null)
                throw new ArgumentException($"Функция '{step.functionName}' не найдена");

            // Создаем контекст функции
            var functionContext = new FunctionContext
            {
                FunctionName = step.functionName,
                ReturnStepId = step.returnToStep,
                Variables = new VariableScope(context.Variables),
                Depth = context.FunctionStack.CurrentDepth + 1
            };

            // Инициализируем параметры функции
            InitializeFunctionParameters(step, functionContext, context);

            // Добавляем в стек вызовов
            context.FunctionStack.Push(functionContext);
            context.Statistics.RecursiveCalls++;

            AddVisualizationStep(step, context, "call_function",
                step.description ?? $"Вызов функции: {step.functionName}",
                metadata: new Dictionary<string, object>
                {
                    ["function_name"] = step.functionName,
                    ["call_depth"] = context.FunctionStack.CurrentDepth,
                    ["parameters"] = step.functionParameters
                });

            // Переходим к точке входа функции
            context.OperationExecutor.Execute(FindStep(function.entryPoint, context.Request), context);
        }

        private void InitializeFunctionParameters(AlgorithmStep step, FunctionContext functionContext, ExecutionContext context)
        {
            foreach (var param in step.functionParameters)
            {
                var value = EvaluateExpression(param.Value, context);
                functionContext.Variables.Set(param.Key, value);
            }
        }

    }
}
