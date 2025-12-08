using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.Algorithms.Sorting
{
    public class BubbleSortAlgorithm : BaseAlgorithm<ArrayStructure, int[], SortingStep>
    {
        public override string Name => "BubbleSort";

        protected override void ExecuteAlgorithm(AlgorithmConfig config, ArrayStructure structure)
        {
            var array = structure.GetState(); // Создаем копию для работы
            var n = array.Length;

            // Начальный шаг - показываем исходный массив
            CustomAddStep(array.ToArray());

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    RecordComparison();

                    // Шаг сравнения - показываем сравниваемые элементы
                    CustomAddStep(
                        array: array,
                        comparing: new[] { j, j + 1 }
                    );

                    if (array[j] > array[j + 1])
                    {
                        // Обмен элементов
                        (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        RecordSwap();

                        // Обновляем состояние структуры
                        structure.ApplyState(array);

                        // Шаг обмена - показываем обмениваемые элементы
                        CustomAddStep(
                            array: array,
                            swapping: new[] { j, j + 1 }
                        );
                    }
                }

                // После каждого прохода внешнего цикла отмечаем отсортированные элементы
                // В пузырьковой сортировке после i-ой итерации последние i+1 элементов отсортированы
                var sortedIndices = Enumerable.Range(n - i - 1, i + 1).ToArray();
                CustomAddStep(
                    array: array,
                    sorted: sortedIndices
                );
            }

            // Финальный шаг - весь массив отсортирован
            CustomAddStep(
                array: array,
                sorted: Enumerable.Range(0, n).ToArray()
            );

            // Применяем финальное состояние
            structure.ApplyState(array);
        }

        protected override Dictionary<string, object> GetOutputData(ArrayStructure structure)
        {
            return new Dictionary<string, object>
            {
                ["origin_array"] = structure.GetOriginState(),
                ["sorted_array"] = structure.GetState(),
                ["is_sorted"] = true
            };
        }

        private void CustomAddStep(int[] array, int[] comparing = null, int[] swapping = null, int pivotIndex = -1, int[] sorted = null)
        {
            var new_step = new SortingStep
            {
                array = CloneArray(array)
            };
            if (comparing != null)
            {
                new_step.comparing = comparing;
            }
            if (swapping != null)
            {
                new_step.swapping = swapping;
            }
            if (pivotIndex != -1)
            {
                new_step.pivotIndex = pivotIndex;
            }
            if (sorted != null)
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
    }
}
