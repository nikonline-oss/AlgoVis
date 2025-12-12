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
    public class SelectionSortAlgorithm : BaseAlgorithm<ArrayStructure, int[], SortingStep>
    {
        private int _currentIteration;
        private int _minIndex;

        public override string Name => "SelectionSort";

        protected override void ExecuteAlgorithm(AlgorithmConfig config, ArrayStructure structure)
        {
            var array = structure.GetState();
            bool detailed = config.Parameters?.ContainsKey("Detailed") == true &&
                           (bool)config.Parameters["Detailed"];

            bool dualSelection = config.Parameters?.ContainsKey("DualSelection") == true &&
                               (bool)config.Parameters["DualSelection"];

            // Сброс состояния
            _currentIteration = 0;

            // Начальный шаг
            CustomAddStep(array, description: "Начальное состояние массива");

            if (dualSelection && array.Length > 1)
            {
                DualSelectionSort(array, structure, detailed);
            }
            else
            {
                SelectionSortWithVisualization(array, structure, detailed);
            }

            // Финальный шаг
            CustomAddStep(array,
                sorted: Enumerable.Range(0, array.Length).ToArray(),
                description: "Сортировка выбором завершена");
        }

        private void SelectionSortWithVisualization(int[] array, ArrayStructure structure, bool detailed)
        {
            int n = array.Length;

            for (int i = 0; i < n - 1; i++)
            {
                _currentIteration++;
                _minIndex = i;

                // Начало итерации
                CustomAddStep(array,
                    comparing: new[] { i },
                    sorted: detailed ? Enumerable.Range(0, i).ToArray() : null,
                    description: $"Итерация {_currentIteration}: ищем минимальный элемент с позиции {i}");

                // Поиск минимального элемента
                FindMinimum(array, i, n, structure, detailed);

                // Обмен, если необходимо
                if (_minIndex != i)
                {
                    PerformSwap(array, i, _minIndex, structure, detailed);
                }
                else
                {
                    // Элемент уже на месте
                    CustomAddStep(array,
                        sorted: Enumerable.Range(0, i + 1).ToArray(),
                        description: $"Элемент [{i}] = {array[i]} уже минимальный в неотсортированной части");
                }
            }
        }

        private void FindMinimum(int[] array, int start, int end, ArrayStructure structure, bool detailed)
        {
            for (int j = start + 1; j < end; j++)
            {
                RecordComparison();

                // Визуализация сравнения
                CustomAddStep(array,
                    comparing: new[] { _minIndex, j },
                    sorted: detailed ? Enumerable.Range(0, start).ToArray() : null,
                    description: detailed ?
                        $"Сравниваем [{_minIndex}] = {array[_minIndex]} с [{j}] = {array[j]}" :
                        null);

                if (array[j] < array[_minIndex])
                {
                    _minIndex = j;

                    // Визуализация нового минимума
                    if (detailed)
                    {
                        CustomAddStep(array,
                            comparing: new[] { _minIndex },
                            sorted: Enumerable.Range(0, start).ToArray(),
                            description: $"Новый минимальный элемент: [{_minIndex}] = {array[_minIndex]}");
                    }
                }
            }
        }

        private void PerformSwap(int[] array, int i, int minIndex, ArrayStructure structure, bool detailed)
        {
            // Перед обменом
            CustomAddStep(array,
                comparing: new[] { i, minIndex },
                description: detailed ?
                    $"Меняем местами [{i}] = {array[i]} и [{minIndex}] = {array[minIndex]}" :
                    null);

            // Выполняем обмен
            Swap(array, i, minIndex, structure);

            // После обмена
            CustomAddStep(array,
                swapping: new[] { i, minIndex },
                sorted: Enumerable.Range(0, i + 1).ToArray(),
                description: detailed ?
                    $"Обмен завершен. Минимальный элемент {array[i]} на позиции [{i}]" :
                    null);
        }

        private void DualSelectionSort(int[] array, ArrayStructure structure, bool detailed)
        {
            int left = 0;
            int right = array.Length - 1;

            CustomAddStep(array, description: "Начинаем двустороннюю сортировку выбором");

            while (left < right)
            {
                _currentIteration++;
                int minIndex = left;
                int maxIndex = left;

                // Поиск минимального и максимального
                for (int i = left + 1; i <= right; i++)
                {
                    RecordComparison();

                    // Поиск минимума
                    if (array[i] < array[minIndex])
                    {
                        minIndex = i;
                    }

                    // Поиск максимума
                    if (array[i] > array[maxIndex])
                    {
                        maxIndex = i;
                    }
                }

                // Визуализация найденных элементов
                CustomAddStep(array,
                    comparing: new[] { minIndex, maxIndex },
                    description: $"Найдены: минимум [{minIndex}] = {array[minIndex]}, максимум [{maxIndex}] = {array[maxIndex]}");

                // Обмен минимума
                if (minIndex != left)
                {
                    Swap(array, left, minIndex, structure);
                    CustomAddStep(array,
                        swapping: new[] { left, minIndex },
                        description: $"Обмен минимума: [{left}] ↔ [{minIndex}]");

                    // Корректируем maxIndex если он был на left
                    if (maxIndex == left)
                        maxIndex = minIndex;
                }

                // Обмен максимума
                if (maxIndex != right)
                {
                    Swap(array, right, maxIndex, structure);
                    CustomAddStep(array,
                        swapping: new[] { right, maxIndex },
                        description: $"Обмен максимума: [{right}] ↔ [{maxIndex}]");
                }

                // Обновляем границы
                left++;
                right--;

                // Визуализация прогресса
                if (detailed)
                {
                    var sortedIndices = Enumerable.Range(0, left)
                        .Concat(Enumerable.Range(right + 1, array.Length - right - 1))
                        .ToArray();

                    CustomAddStep(array,
                        sorted: sortedIndices,
                        description: $"Прогресс: отсортировано {left} слева и {array.Length - right - 1} справа");
                }
            }
        }

        private void Swap(int[] array, int i, int j, ArrayStructure structure)
        {
            (array[i], array[j]) = (array[j], array[i]);
            RecordSwap();
            structure.ApplyState(array);
        }

        private void CustomAddStep(int[] array, int[] comparing = null, int[] swapping = null, int[] sorted = null, string description = "")
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

            if (sorted != null && sorted.Length > 0)
            {
                new_step.sorted = sorted;
            }

            RawAddStep(new_step);
        }

        private int[] CloneArray(int[] array)
        {
            return array.ToArray();
        }

        protected override Dictionary<string, object> GetOutputData(ArrayStructure structure)
        {
            var sortedArray = structure.GetState();

            return new Dictionary<string, object>
            {
                ["sorted_array"] = sortedArray,
                ["is_sorted"] = IsSorted(sortedArray),
                ["array_length"] = sortedArray.Length,
                ["iterations"] = _currentIteration,
                ["time_complexity"] = "O(n²)",
                ["space_complexity"] = "O(1)",
                ["stable"] = false
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
