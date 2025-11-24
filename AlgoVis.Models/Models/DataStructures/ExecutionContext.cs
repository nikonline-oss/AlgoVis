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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.DataStructures
{
    public class ExecutionContext
    {
        public CustomAlgorithmRequest Request { get; set; }
        public IDataStructure Structure { get; set; }
        public AlgorithmStatistics Statistics { get; set; }
        public List<VisualizationStep> VisualizationSteps { get; set; }
        public IVariableScope Variables { get; set; }
        public FunctionStack FunctionStack { get; set; }
        public StepExecutionHistory StepHistory { get; set; }
        public IParser ExpressionParser { get; set; }
        public IOperationExecutor OperationExecutor { get; set; }


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
                visualizationData = Structure.ToVisualizationData(),
                metadata = processedMetadata
            };

            if (highlights != null)
            {
                visualizationStep.visualizationData.highlights.AddRange(highlights);
            }

            visualizationStep.metadata["visualization_type"] = step.visualizationType;

            VisualizationSteps.Add(visualizationStep);
            Statistics.Steps++;
        }

        /// <summary>
        /// Символ, обозначающий что значение является выражением
        /// </summary>
        private const string ExpressionPrefix = "=";

        /// <summary>
        /// Обрабатывает метаданные, парся выражения помеченные специальным символом
        /// </summary>
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
        /// Проверяет, помечена ли строка как выражение
        /// </summary>
        private bool IsMarkedAsExpression(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Trim().StartsWith(ExpressionPrefix);
        }

        /// <summary>
        /// Извлекает выражение из строки (убирает символ выражения)
        /// </summary>
        private string ExtractExpression(string markedValue)
        {
            return markedValue.Trim().Substring(ExpressionPrefix.Length).Trim();
        }

        /// <summary>
        /// Пытается распарсить и вычислить выражение
        /// </summary>
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
