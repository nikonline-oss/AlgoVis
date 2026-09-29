using AlgoVis.Evaluator.Evaluator.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Parsing;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Functions;
using AlgoVis.Models.Models.Operations.Interfaces;
using AlgoVis.Models.Models.Steps;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.DataStructures
{
    /// <summary>
    /// Представляет контекст выполнения алгоритма для визуализации.
    /// Содержит все необходимые данные и состояния для работы алгоритма.
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>Запрос на выполнение пользовательского алгоритма</summary>
        public CustomAlgorithmRequest Request { get; set; }

        /// <summary>Текущая структура данных</summary>
        public IDataStructure Structure { get; set; }

        /// <summary>Статистика выполнения алгоритма</summary>
        public AlgorithmStatistics Statistics { get; set; }

        /// <summary>Список шагов визуализации</summary>
        public List<VisualizationStep> VisualizationSteps { get; set; }

        /// <summary>Область видимости переменных</summary>
        public IVariableScope Variables { get; set; }

        /// <summary>Стек вызовов функций</summary>
        public FunctionStack FunctionStack { get; set; }

        /// <summary>История выполнения шагов</summary>
        public StepExecutionHistory StepHistory { get; set; }

        /// <summary>Парсер выражений</summary>
        public IParser ExpressionParser { get; set; }

        /// <summary>Исполнитель операций</summary>
        public IOperationExecutor OperationExecutor { get; set; }

        /// <summary>
        /// Добавляет шаг визуализации на основе выполненного шага алгоритма.
        /// </summary>
        /// <param name="step">Шаг алгоритма, для которого создается визуализация.</param>
        /// <param name="operation">Тип операции, выполненной на этом шаге.</param>
        /// <param name="description">Описание шага для отображения в визуализации.</param>
        /// <param name="highlights">Список подсвечиваемых элементов (опционально).</param>
        /// <param name="metadata">Метаданные шага (опционально).</param>
        /// <remarks>
        /// Метод проверяет флаг step.visualize перед созданием шага визуализации.
        /// Если параметр highlights не указан, создается пустой список.
        /// </remarks>
        public void AddVisualizationStep(AlgorithmStep step, string operation, string description,
            List<HighlightedElement> highlights = null, Dictionary<string, object> metadata = null)
        {
            // Проверяем, нужно ли визуализировать этот шаг
            if (!step.visualize)
                return;

            var stepHighlights = highlights ?? new List<HighlightedElement>();

            // Добавляем подсветку из настроек шага
            if (step.highlightElements?.Count > 0)
            {
                foreach (var elementId in step.highlightElements)
                {
                    stepHighlights.Add(new HighlightedElement
                    {
                        ElementId = elementId,
                        HighlightType = step.visualizationType ?? "custom",
                        Color = step.highlightColor ?? "yellow"
                    });
                }
            }

            // Обрабатываем метаданные - парсим выражения в значениях
            var processedMetadata = ProcessMetadata(metadata ?? new Dictionary<string, object>());

            var visualizationStep = new VisualizationStep
            {
                stepNumber = VisualizationSteps.Count + 1,
                operation = operation,
                description = description,
                //visualizationData = Structure.ToVisualizationData(),
                metadata = processedMetadata,
                variables = Variables.GetSnapshot() // Используем снимок вместо прямой ссылки
            };

            if (highlights != null)
            {
                //visualizationStep.visualizationData.highlights.AddRange(highlights);
            }

            visualizationStep.metadata["visualization_type"] = step.visualizationType;

            VisualizationSteps.Add(visualizationStep);
            Statistics.Steps++;
        }

        /// <summary>
        /// Символ, обозначающий что значение является выражением (=)
        /// </summary>
        private const string ExpressionPrefix = "=";

        /// <summary>
        /// Обрабатывает метаданные, парся выражения помеченные специальным символом.
        /// </summary>
        /// <param name="metadata">Исходные метаданные.</param>
        /// <returns>Обработанные метаданные с вычисленными выражениями.</returns>
        /// <remarks>
        /// Если значение строки начинается с '=', метод пытается вычислить это выражение.
        /// В случае ошибки возвращается оригинальное значение.
        /// </remarks>
        private Dictionary<string, object> ProcessMetadata(Dictionary<string, object> metadata)
        {
            var processedMetadata = new Dictionary<string, object>();

            foreach (var kvp in metadata)
            {
                try
                {
                    // Если значение - строка, начинающаяся с символа выражения, пытаемся его распарсить
                    if (kvp.Value is string stringValue && IsMarkedAsExpression(stringValue))
                    {
                        var expression = ExtractExpression(stringValue);
                        var parsedValue = TryParseExpression(expression);
                        processedMetadata[kvp.Key] = parsedValue ?? stringValue;
                    }
                    else
                    {
                        // Оставляем оригинальное значение
                        processedMetadata[kvp.Key] = kvp.Value;
                    }
                }
                catch (Exception ex)
                {
                    // В случае ошибки парсинга оставляем оригинальное значение
                    processedMetadata[kvp.Key] = kvp.Value;
                    // Можно добавить логгирование ошибки
                    System.Diagnostics.Debug.WriteLine($"Ошибка парсинга выражения в метаданных: {ex.Message}");
                }
            }

            return processedMetadata;
        }

        /// <summary>
        /// Проверяет, помечена ли строка как выражение.
        /// </summary>
        /// <param name="value">Проверяемая строка.</param>
        /// <returns>true, если строка начинается с символа '=', иначе false.</returns>
        private bool IsMarkedAsExpression(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Trim().StartsWith(ExpressionPrefix);
        }

        /// <summary>
        /// Извлекает выражение из строки (убирает символ выражения).
        /// </summary>
        /// <param name="markedValue">Строка с символом выражения в начале.</param>
        /// <returns>Выражение без символа '=' в начале.</returns>
        private string ExtractExpression(string markedValue)
        {
            return markedValue.Trim().Substring(ExpressionPrefix.Length).Trim();
        }

        /// <summary>
        /// Пытается распарсить и вычислить выражение.
        /// </summary>
        /// <param name="expression">Выражение для вычисления.</param>
        /// <returns>Результат вычисления выражения или оригинальное выражение с символом '=' в случае ошибки.</returns>
        /// <remarks>
        /// Если парсер или исполнитель операций не инициализированы, возвращает выражение с символом '='.
        /// </remarks>
        private object TryParseExpression(string expression)
        {
            if (ExpressionParser == null || OperationExecutor == null)
            {
                // Если нет парсера или исполнителя, возвращаем оригинальное выражение
                return $"{ExpressionPrefix}{expression}";
            }

            try
            {
                // Парсим выражение
                var parsedExpression = ExpressionParser.Parse(expression);

                // Вычисляем выражение с использованием текущего контекста переменных
                var result = parsedExpression.Evaluate(Variables);
                return result;
            }
            catch (Exception ex)
            {
                // Если не удалось распарсить, возвращаем оригинальное выражение с символом
                System.Diagnostics.Debug.WriteLine($"Не удалось вычислить выражение '{expression}': {ex.Message}");
                return $"{ExpressionPrefix}{expression}";
            }
        }
    }
}