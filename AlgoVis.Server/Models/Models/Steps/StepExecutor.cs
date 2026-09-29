using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.Steps.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;

namespace AlgoVis.Models.Models.Steps
{
    /// <summary>
    /// Реализация исполнителя шагов алгоритма с защитой от бесконечных циклов.
    /// </summary>
    /// <remarks>
    /// Выполняет шаги алгоритма, отслеживая глубину вызовов и количество выполнений
    /// для предотвращения зацикливания и переполнения стека.
    /// </remarks>
    public class StepExecutor : IStepExecutor
    {
        /// <summary>
        /// Максимальное количество выполнений одного шага.
        /// </summary>
        private const int MAX_STEPS = 10000;

        /// <summary>
        /// Максимальная глубина вызовов функций.
        /// </summary>
        private const int MAX_CALL_DEPTH = 100;

        /// <summary>
        /// Выполняет шаг алгоритма по его идентификатору с проверкой ограничений.
        /// </summary>
        /// <param name="stepId">Идентификатор шага для выполнения.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="stepId"/> равен null или пустой строке,
        /// или если <paramref name="context"/> равен null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается в следующих случаях:
        /// <list type="bullet">
        /// <item>Превышено максимальное количество выполнений шага (<see cref="MAX_STEPS"/>).</item>
        /// <item>Превышена максимальная глубина вызовов (<see cref="MAX_CALL_DEPTH"/>).</item>
        /// <item>Шаг с указанным идентификатором не найден.</item>
        /// <item>Произошла ошибка при выполнении шага.</item>
        /// </list>
        /// </exception>
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

        /// <summary>
        /// Выполняет отдельный шаг алгоритма.
        /// </summary>
        /// <param name="step">Шаг алгоритма для выполнения.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="step"/> или <paramref name="context"/> равны null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается при ошибке выполнения шага.
        /// </exception>
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

        /// <summary>
        /// Ищет шаг алгоритма по идентификатору.
        /// </summary>
        /// <param name="stepId">Идентификатор искомого шага.</param>
        /// <param name="request">Запрос алгоритма, содержащий шаги и функции.</param>
        /// <returns>
        /// Найденный шаг алгоритма или null, если шаг не найден.
        /// </returns>
        /// <remarks>
        /// Поиск выполняется сначала в основных шагах запроса, затем в шагах всех функций.
        /// </remarks>
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