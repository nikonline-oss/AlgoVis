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
    public class QuickSortAlgorithm : BaseAlgorithm<ArrayStructure, int[], SortingStep>
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

            // Начальный шаг - показываем исходный массив
            CustomAddStep(array);

            QuickSort(array, 0, array.Length - 1, structure, pivotStrategy, detailed);

            // Финальный шаг - показываем отсортированный массив
            CustomAddStep(array, sorted: Enumerable.Range(0, array.Length).ToArray());
        }

        private void QuickSort(int[] array, int low, int high, ArrayStructure structure,
            string pivotStrategy, bool detailed)
        {
            if (low < high)
            {
                RecordRecursiveCall();

                if (detailed)
                {
                    // Показываем текущий диапазон
                    CustomAddStep(array, comparing: Enumerable.Range(low, high - low + 1).ToArray());
                }

                int pivotIndex = Partition(array, low, high, structure, pivotStrategy, detailed);

                if (detailed)
                {
                    // Показываем опорный элемент после разделения
                    CustomAddStep(array, pivotIndex: pivotIndex, sorted: new[] { pivotIndex });
                }

                QuickSort(array, low, pivotIndex - 1, structure, pivotStrategy, detailed);
                QuickSort(array, pivotIndex + 1, high, structure, pivotStrategy, detailed);
            }
            else if (low == high && detailed)
            {
                // Базовый случай - один элемент
                CustomAddStep(array, comparing: new[] { low }, sorted: new[] { low });
            }
        }

        private int Partition(int[] array, int low, int high, ArrayStructure structure,
            string pivotStrategy, bool detailed)
        {
            int pivotIndex = ChoosePivot(array, low, high, pivotStrategy);
            int pivotValue = array[pivotIndex];

            // Показываем выбранный опорный элемент
            CustomAddStep(array, comparing: new[] { pivotIndex }, pivotIndex: pivotIndex);

            // Если опорный элемент не последний, перемещаем его в конец
            if (pivotIndex != high)
            {
                Swap(array, pivotIndex, high, structure);
                pivotIndex = high;
                // Показываем перемещение опорного элемента
                CustomAddStep(array, swapping: new[] { pivotIndex, high }, pivotIndex: pivotIndex);
            }

            int i = low - 1;

            if (detailed)
            {
                // Показываем инициализацию
                CustomAddStep(array, comparing: new[] { low, high }, pivotIndex: pivotIndex);
            }

            for (int j = low; j < high; j++)
            {
                RecordComparison();

                // Показываем сравнение текущего элемента с опорным
                CustomAddStep(array, comparing: new[] { j, pivotIndex }, pivotIndex: pivotIndex);

                if (array[j] <= pivotValue)
                {
                    i++;

                    if (detailed)
                    {
                        // Показываем перемещение указателя i
                        CustomAddStep(array, comparing: new[] { i }, pivotIndex: pivotIndex);
                    }

                    if (i != j)
                    {
                        Swap(array, i, j, structure);
                        // Показываем обмен
                        CustomAddStep(array, swapping: new[] { i, j }, pivotIndex: pivotIndex);
                    }
                    else if (detailed)
                    {
                        // Показываем, что обмен не требуется
                        CustomAddStep(array, comparing: new[] { i }, pivotIndex: pivotIndex);
                    }
                }
                else if (detailed)
                {
                    // Показываем пропуск элемента
                    CustomAddStep(array, comparing: new[] { j }, pivotIndex: pivotIndex);
                }
            }

            // Помещаем опорный элемент на правильную позицию
            Swap(array, i + 1, high, structure);
            // Показываем финальное размещение опорного элемента
            CustomAddStep(array, swapping: new[] { i + 1, high }, pivotIndex: i + 1);

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

        private void Swap(int[] array, int i, int j, ArrayStructure structure)
        {
            (array[i], array[j]) = (array[j], array[i]);
            RecordSwap();
            structure.ApplyState(array);
        }

        private void CustomAddStep(int[] array, int[] comparing = null, int[] swapping = null, int pivotIndex = -1, int[] sorted = null)
        {
            var new_step = new SortingStep
            {
                array = CloneArray(array)
            };

            if (comparing != null && comparing.Length > 0)
            {
                new_step.comparing = comparing;
            }

            if (swapping != null && swapping.Length > 0)
            {
                new_step.swapping = swapping;
            }

            if (pivotIndex != -1)
            {
                new_step.pivotIndex = pivotIndex;
            }

            if (sorted != null && sorted.Length > 0)
            {
                new_step.sorted = sorted;
            }

            RawAddStep(new_step);
        }

        private int[] CloneArray(int[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            return array.ToArray();
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
