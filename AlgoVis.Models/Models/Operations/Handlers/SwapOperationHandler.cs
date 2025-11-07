using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Operations.Base;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;


namespace AlgoVis.Models.Models.Operations.Handlers
{
    // Операция обмена
    public class SwapOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            context.Statistics.Swaps++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Swap operation requires 2 parameters");

            var index1 = ConvertToIndex(EvaluateExpression(step.parameters[0], context));
            var index2 = ConvertToIndex(EvaluateExpression(step.parameters[1], context));

            var array = GetArrayState(context.Structure);
            (array[index2], array[index1]) = (array[index1], array[index2]);

            context.Structure.ApplyState(array);

            AddVisualizationStep(step, context, "swap",
                step.description ?? $"Обмен элементов [{index1}] и [{index2}]",
                new List<HighlightedElement>
                {
                new() { ElementId = index1.ToString(), HighlightType = "swapping", Color = "red" },
                new() { ElementId = index2.ToString(), HighlightType = "swapping", Color = "red" }
                },
                new Dictionary<string, object>
                {
                    ["index1"] = index1,
                    ["index2"] = index2
                });

            ExecuteNextStep(step, context);
        }

        private int[] GetArrayState(IDataStructure structure)
        {
            var state = structure.GetState();
            return state as int[] ?? throw new InvalidOperationException("Операция обмена поддерживает только массивы");
        }

    }
}
