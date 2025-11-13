using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.Operations.Interfaces;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;

namespace AlgoVis.Models.Models.Operations.Base
{
    public abstract class BaseOperationHandler : IOperationHandler
    {
        public abstract void Execute(AlgorithmStep step, ExecutionContext context);

        protected AlgorithmStep FindStep(string stepId, CustomAlgorithmRequest request)
        {
            // Сначала ищем в основных шагах
            var step = request.steps.FirstOrDefault(s => s.id == stepId);
            if (step != null) return step;

            // Затем ищем в функциях
            if (request.functions != null)
            {
                foreach (var function in request.functions)
                {
                    step = function.steps.FirstOrDefault(s => s.id == stepId);
                    if (step != null) return step;
                }
            }

            throw new InvalidOperationException($"Шаг '{stepId}' не найден");
        }

        protected void ExecuteNextStep(AlgorithmStep step, ExecutionContext context)
        {
            if (!string.IsNullOrEmpty(step.nextStep))
            {
                var nextStep = FindStep(step.nextStep, context.Request);
                context.OperationExecutor.Execute(nextStep, context);
            }
        }

        protected object EvaluateExpression(string expression, ExecutionContext context)
        {
            IVariableScope currentScope = context.FunctionStack.Current?.Variables ?? context.Variables;

            var result = context.ExpressionEvaluator.Evaluate(expression, currentScope);
            return ExtractValue(result);
        }

        protected bool EvaluateCondition(string condition, ExecutionContext context)
        {
            IVariableScope currentScope = context.FunctionStack.Current?.Variables ?? context.Variables;

            return context.ExpressionEvaluator.EvaluateCondition(condition, currentScope);
        }

        protected int ConvertToIndex(object value)
        {
            var extractedValue = ExtractValue(value);

            return extractedValue switch
            {
                int i => i,
                double d => (int)d,
                _ => Convert.ToInt32(extractedValue)
            };
        }

        protected object ExtractValue(object value)
        {
            Console.WriteLine($"🔍 ExtractValue: входное значение = {value}, тип = {value?.GetType()}");

            if (value is VariableValue variableValue)
            {
                Console.WriteLine($"🔍 ExtractValue: из VariableValue типа {variableValue.Type}");

                // Для объектов возвращаем сам VariableValue, чтобы сохранить возможность доступа к свойствам
                if (variableValue.Type == VariableType.Object)
                {
                    return variableValue;
                }

                var result = variableValue.Value;
                Console.WriteLine($"🔍 ExtractValue: извлечено значение = {result}");
                return result;
            }
            else if (value is Dictionary<string, VariableValue> dict)
            {
                Console.WriteLine($"🔍 ExtractValue: получен словарь, преобразуем в VariableValue");
                return new VariableValue(dict);
            }

            Console.WriteLine($"🔍 ExtractValue: возвращаем как есть = {value}");
            return value;
        }

        protected void AddVisualizationStep(AlgorithmStep step, ExecutionContext context,
        string operation, string description,
        List<HighlightedElement> highlights = null,
        Dictionary<string, object> metadata = null)
        {
            context.AddVisualizationStep(step, operation, description, highlights, metadata);
        }

    }
}
