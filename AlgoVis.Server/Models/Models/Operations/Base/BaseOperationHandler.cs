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
    /// <summary>
    /// Абстрактный базовый класс обработчиков операций алгоритма.
    /// Предоставляет общую функциональность для всех типов операций.
    /// </summary>
    /// <remarks>
    /// Этот класс реализует интерфейс <see cref="IOperationHandler"/> и содержит
    /// вспомогательные методы для поиска шагов, выполнения выражений и управления визуализацией.
    /// </remarks>
    public abstract class BaseOperationHandler : IOperationHandler
    {
        /// <summary>
        /// Выполняет указанный шаг алгоритма в заданном контексте выполнения.
        /// </summary>
        /// <param name="step">Шаг алгоритма для выполнения.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <remarks>
        /// Абстрактный метод, должен быть реализован в производных классах.
        /// </remarks>
        public abstract void Execute(AlgorithmStep step, ExecutionContext context);

        /// <summary>
        /// Находит шаг алгоритма по его идентификатору в запросе.
        /// </summary>
        /// <param name="stepId">Идентификатор искомого шага.</param>
        /// <param name="request">Запрос алгоритма, содержащий шаги и функции.</param>
        /// <returns>Найденный шаг алгоритма.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если шаг с указанным идентификатором не найден.
        /// </exception>
        /// <remarks>
        /// Сначала ищет в основных шагах запроса, затем в шагах функций.
        /// </remarks>
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

        /// <summary>
        /// Выполняет следующий шаг алгоритма, если он указан.
        /// </summary>
        /// <param name="step">Текущий шаг алгоритма.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <remarks>
        /// Если у текущего шага указан следующий шаг (step.nextStep),
        /// находит его и выполняет через OperationExecutor.
        /// </remarks>
        protected void ExecuteNextStep(AlgorithmStep step, ExecutionContext context)
        {
            if (!string.IsNullOrEmpty(step.nextStep))
            {
                var nextStep = FindStep(step.nextStep, context.Request);
                context.OperationExecutor.Execute(nextStep, context);
            }
        }

        /// <summary>
        /// Вычисляет выражение и возвращает его значение.
        /// </summary>
        /// <param name="expression">Строка с выражением для вычисления.</param>
        /// <param name="context">Контекст выполнения с парсером выражений и переменными.</param>
        /// <returns>Значение выражения в виде <see cref="IVariableValue"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается при ошибках парсинга или вычисления выражения.
        /// </exception>
        /// <remarks>
        /// Если выражение пустое, возвращает IntValue(0).
        /// Использует текущую область видимости переменных из стека функций или глобальную.
        /// </remarks>
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

        /// <summary>
        /// Вычисляет логическое условие.
        /// </summary>
        /// <param name="condition">Строка с условием для вычисления.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <returns>true, если условие истинно; иначе false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается при ошибках парсинга или вычисления условия.
        /// </exception>
        /// <remarks>
        /// Если условие пустое, возвращает false.
        /// Корректно обрабатывает область видимости с учетом стека вызовов функций.
        /// </remarks>
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

        /// <summary>
        /// Добавляет шаг визуализации в контекст выполнения.
        /// </summary>
        /// <param name="step">Текущий шаг алгоритма.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <param name="operation">Тип операции для визуализации.</param>
        /// <param name="description">Описание шага визуализации.</param>
        /// <param name="highlights">Список подсвечиваемых элементов (опционально).</param>
        /// <param name="metadata">Дополнительные метаданные для визуализации (опционально).</param>
        /// <remarks>
        /// Метод делегирует вызов контексту выполнения для добавления шага визуализации.
        /// </remarks>
        protected void AddVisualizationStep(AlgorithmStep step, ExecutionContext context,
        string operation, string description,
        List<HighlightedElement> highlights = null,
        Dictionary<string, object> metadata = null)
        {
            context.AddVisualizationStep(step, operation, description, highlights, metadata);
        }

    }
}