using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.Algorithms.Sorting
{
    public class QuickSortAlgorithm : BaseAlgorithm<ArrayStructure, int[]>
    {
        public override string Name => "QuickSort";

        protected override void ExecuteAlgorithm(AlgorithmConfig config, ArrayStructure structure)
        {
            var array = structure.GetState();
            bool detailed = config.Parameters?.ContainsKey("Detailed") == true &&
                           (bool)config.Parameters["Detailed"];

            string pivotStrategy = config.Parameters?.ContainsKey("PivotStrategy") == true
                ? (string)config.Parameters["PivotStrategy"]
                : "last";

            AddStep("start", "Начало быстрой сортировки", structure);

            QuickSort(array, 0, array.Length - 1, structure, pivotStrategy, detailed);

            AddStep("complete", "Быстрая сортировка завершена", structure,
                highlights: Enumerable.Range(0, array.Length)
                    .Select(i => new HighlightedElement
                    {
                        ElementId = i.ToString(),
                        HighlightType = "sorted",
                        Color = "green"
                    }).ToList());
        }

        private void QuickSort(int[] array, int low, int high, ArrayStructure structure,
            string pivotStrategy, bool detailed)
        {
            if (low < high)
            {
                RecordRecursiveCall();

                if (detailed)
                {
                    AddStep("recursive_call",
                        $"Рекурсивный вызов для диапазона [{low}, {high}]", structure,
                        highlights: Enumerable.Range(low, high - low + 1)
                            .Select(i => new HighlightedElement
                            {
                                ElementId = i.ToString(),
                                HighlightType = "current_range",
                                Color = "lightblue"
                            }).ToList());
                }

                int pivotIndex = Partition(array, low, high, structure, pivotStrategy, detailed);

                if (detailed)
                {
                    AddStep("partition_complete",
                        $"Разделение завершено. Опорный элемент [{pivotIndex}] = {array[pivotIndex]}", structure,
                        highlights: new List<HighlightedElement>
                        {
                            new() { ElementId = pivotIndex.ToString(), HighlightType = "pivot_final", Color = "purple" }
                        });
                }

                QuickSort(array, low, pivotIndex - 1, structure, pivotStrategy, detailed);
                QuickSort(array, pivotIndex + 1, high, structure, pivotStrategy, detailed);
            }
            else if (low == high && detailed)
            {
                AddStep("base_case",
                    $"Базовый случай - один элемент [{low}] = {array[low]}", structure,
                    highlights: new List<HighlightedElement>
                    {
                        new() { ElementId = low.ToString(), HighlightType = "base_case", Color = "gray" }
                    });
            }
        }

        private int Partition(int[] array, int low, int high, ArrayStructure structure,
            string pivotStrategy, bool detailed)
        {
            int pivotIndex = ChoosePivot(array, low, high, pivotStrategy);
            int pivotValue = array[pivotIndex];

            // Если опорный элемент не последний, перемещаем его в конец
            if (pivotIndex != high)
            {
                Swap(array, pivotIndex, high, structure, "move_pivot",
                    $"Перемещение опорного элемента [{pivotIndex}] в конец [{high}]");
                pivotIndex = high;
            }

            AddStep("select_pivot",
                $"Выбор опорного элемента [{pivotIndex}] = {pivotValue}", structure,
                highlights: new List<HighlightedElement>
                {
                    new() { ElementId = pivotIndex.ToString(), HighlightType = "pivot", Color = "orange" }
                });

            int i = low - 1;

            if (detailed)
            {
                AddStep("initialize",
                    $"Инициализация: i = {i}, диапазон [{low}, {high}]", structure,
                    highlights: new List<HighlightedElement>
                    {
                        new() { ElementId = pivotIndex.ToString(), HighlightType = "pivot", Color = "orange" }
                    });
            }

            for (int j = low; j < high; j++)
            {
                RecordComparison();

                AddStep("compare",
                    $"Сравнение [{j}] = {array[j]} с опорным {pivotValue}", structure,
                    highlights: new List<HighlightedElement>
                    {
                        new() { ElementId = j.ToString(), HighlightType = "comparing", Color = "yellow" },
                        new() { ElementId = pivotIndex.ToString(), HighlightType = "pivot", Color = "orange" }
                    });

                if (array[j] <= pivotValue)
                {
                    i++;

                    if (detailed)
                    {
                        AddStep("move_pointer",
                            $"Увеличиваем i до {i}", structure,
                            highlights: new List<HighlightedElement>
                            {
                                new() { ElementId = i.ToString(), HighlightType = "pointer", Color = "cyan" },
                                new() { ElementId = pivotIndex.ToString(), HighlightType = "pivot", Color = "orange" }
                            });
                    }

                    if (i != j)
                    {
                        Swap(array, i, j, structure, "swap",
                            $"Обмен [{i}] и [{j}] так как {array[j]} <= {pivotValue}");
                    }
                    else if (detailed)
                    {
                        AddStep("no_swap",
                            $"Обмен не требуется: i = j = {i}", structure,
                            highlights: new List<HighlightedElement>
                            {
                                new() { ElementId = i.ToString(), HighlightType = "pointer", Color = "cyan" },
                                new() { ElementId = pivotIndex.ToString(), HighlightType = "pivot", Color = "orange" }
                            });
                    }
                }
                else if (detailed)
                {
                    AddStep("skip",
                        $"Пропускаем [{j}] = {array[j]} (больше опорного)", structure,
                        highlights: new List<HighlightedElement>
                        {
                            new() { ElementId = j.ToString(), HighlightType = "skipped", Color = "lightgray" },
                            new() { ElementId = pivotIndex.ToString(), HighlightType = "pivot", Color = "orange" }
                        });
                }
            }

            // Помещаем опорный элемент на правильную позицию
            Swap(array, i + 1, high, structure, "place_pivot",
                $"Размещение опорного элемента на позицию {i + 1}");

            return i + 1;
        }

        private int ChoosePivot(int[] array, int low, int high, string strategy)
        {
            return strategy.ToLower() switch
            {
                "first" => low,
                "last" => high,
                "middle" => low + (high - low) / 2,
                "random" => new Random().Next(low, high + 1),
                "median" => MedianOfThree(array, low, high),
                _ => high // по умолчанию последний элемент
            };
        }

        private int MedianOfThree(int[] array, int low, int high)
        {
            int mid = low + (high - low) / 2;

            // Находим медиану из трех элементов
            if (array[low] > array[mid])
                (low, mid) = (mid, low);
            if (array[low] > array[high])
                (low, high) = (high, low);
            if (array[mid] > array[high])
                (mid, high) = (high, mid);

            return mid;
        }

        private void Swap(int[] array, int i, int j, ArrayStructure structure, string operation, string description)
        {
            (array[i], array[j]) = (array[j], array[i]);
            RecordSwap();

            structure.ApplyState(array);

            AddStep(operation, description, structure,
                highlights: new List<HighlightedElement>
                {
                    new() { ElementId = i.ToString(), HighlightType = "swapping", Color = "red" },
                    new() { ElementId = j.ToString(), HighlightType = "swapping", Color = "red" }
                });
        }

        protected override Dictionary<string, object> GetOutputData(ArrayStructure structure)
        {
            var sortedArray = structure.GetState();
            bool isSorted = IsSorted(sortedArray);

            return new Dictionary<string, object>
            {
                ["sorted_array"] = sortedArray,
                ["is_sorted"] = isSorted,
                ["array_length"] = sortedArray.Length,
                ["algorithm_type"] = "divide_and_conquer"
            };
        }

        private bool IsSorted(int[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                if (array[i] > array[i + 1])
                    return false;
            }
            return true;
        }
    }
}
