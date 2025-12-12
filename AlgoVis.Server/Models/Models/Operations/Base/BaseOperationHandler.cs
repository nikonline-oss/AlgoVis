using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Parsing;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
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

        protected IVariableValue EvaluateExpression(string expression, ExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return new IntValue(0);

            try
            {
                var node = context.ExpressionParser.Parse(expression);
                return node.Evaluate(context.FunctionStack.Current != null ? context.FunctionStack.Current.Variables : context.Variables);
            }
            catch (ParseException ex)
            {
                throw new InvalidOperationException($"Ошибка парсинга выражения '{expression}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка вычисления выражения '{expression}': {ex.Message}", ex);
            }
        }

        protected bool EvaluateCondition(string condition, ExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return false;

            try
            {
                // Получаем правильную область видимости с учетом стека вызовов
                IVariableScope currentScope = context.FunctionStack.Current?.Variables ?? context.Variables;

                var node = context.ExpressionParser.Parse(condition);
                var result = node.Evaluate(currentScope);

                return result.ToBool();
            }
            catch (ParseException ex)
            {
                throw new InvalidOperationException($"Ошибка парсинга условия '{condition}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка вычисления условия '{condition}': {ex.Message}", ex);
            }
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
