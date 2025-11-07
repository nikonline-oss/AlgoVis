using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Functions;
using AlgoVis.Models.Models.Operations.Interfaces;
using AlgoVis.Models.Models.Steps;
using AlgoVis.Models.Models.Visualization;

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
        public IExpressionEvaluator ExpressionEvaluator { get; set; }
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

            var visualizationStep = new VisualizationStep
            {
                stepNumber = VisualizationSteps.Count + 1,
                operation = operation,
                description = description,
                visualizationData = Structure.ToVisualizationData(),
                metadata = metadata ?? new Dictionary<string, object>()
            };

            if (highlights != null)
            {
                visualizationStep.visualizationData.highlights.AddRange(highlights);
            }

            visualizationStep.metadata["visualization_type"] = step.visualizationType;

            VisualizationSteps.Add(visualizationStep);
            Statistics.Steps++;
        }

    }
}
