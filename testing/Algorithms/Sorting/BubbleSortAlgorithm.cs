using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Algorithms.Core;
using testing.Models.Core;
using testing.Models.DataStructures;
using testing.Models.Visualization;

namespace testing.Algorithms.Sorting
{
    public class BubbleSortAlgorithm : BaseAlgorithm<ArrayStructure, int[]>
    {
        public override string Name => "BubbleSort";

        protected override void ExecuteAlgorithm(AlgorithmConfig config, ArrayStructure structure)
        {
            var array = structure.GetState();
            bool detailed = config.Parameters?.ContainsKey("Detailed") == true &&
                           (bool)config.Parameters["Detailed"];

            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - i - 1; j++)
                {
                    RecordComparison();

                    AddStep("compare", $"Сравнение элементов [{j}] и [{j + 1}]", structure,
                        highlights: new List<HighlightedElement>
                        {
                            new() { ElementId = j.ToString(), HighlightType = "comparing", Color = "yellow" },
                            new() { ElementId = (j + 1).ToString(), HighlightType = "comparing", Color = "yellow" }
                        });

                    if (array[j] > array[j + 1])
                    {
                        (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        RecordSwap();

                        structure.ApplyState(array);

                        AddStep("swap", $"Обмен элементов [{j}] и [{j + 1}]", structure,
                            highlights: new List<HighlightedElement>
                            {
                                new() { ElementId = j.ToString(), HighlightType = "swapping", Color = "red" },
                                new() { ElementId = (j + 1).ToString(), HighlightType = "swapping", Color = "red" }
                            });
                    }
                }

                if (detailed)
                {
                    var sortedIndices = Enumerable.Range(array.Length - i - 1, i + 1).ToArray();
                    AddStep("mark_sorted", $"Завершена итерация {i + 1}", structure,
                        highlights: sortedIndices.Select(idx =>
                            new HighlightedElement
                            {
                                ElementId = idx.ToString(),
                                HighlightType = "sorted",
                                Color = "green"
                            }).ToList());
                }
            }

            structure.ApplyState(array);
        }

        protected override Dictionary<string, object> GetOutputData(ArrayStructure structure)
        {
            return new Dictionary<string, object>
            {
                ["sorted_array"] = structure.GetState(),
                ["is_sorted"] = true
            };
        }
    }
}
