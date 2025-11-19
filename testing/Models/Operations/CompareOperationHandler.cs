using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using testing.Models.DataStructures;
using testing.Models.Visualization;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;

namespace testing.Models.Operations
{
    // Операция сравнения
    public class CompareOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            context.Statistics.Comparisons++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Compare operation requires 2 parameters");

            var index1 = ConvertToIndex(EvaluateExpression(step.parameters[0], context));
            var index2 = ConvertToIndex(EvaluateExpression(step.parameters[1], context));

            var array = GetArrayState(context.Structure);
            var value1 = array[index1];
            var value2 = array[index2];
            var comparisonResult = value1.CompareTo(value2);

            context.Variables.Set("last_comparison", comparisonResult);

            AddVisualizationStep(step, context, "compare",
                step.description ?? $"Сравнение [{index1}]={value1} и [{index2}]={value2}",
                new List<HighlightedElement>
                {
                new() { ElementId = index1.ToString(), HighlightType = "comparing", Color = "yellow" },
                new() { ElementId = index2.ToString(), HighlightType = "comparing", Color = "yellow" }
                },
                new Dictionary<string, object>
                {
                    ["index1"] = index1,
                    ["index2"] = index2,
                    ["value1"] = value1,
                    ["value2"] = value2,
                    ["comparison_result"] = comparisonResult
                });

            ExecuteNextStep(step, context);
        }

        private int[] GetArrayState(IDataStructure structure)
        {
            var state = structure.GetState();
            return state as int[] ?? throw new InvalidOperationException("Операция сравнения поддерживает только массивы");
        }
    }
}
