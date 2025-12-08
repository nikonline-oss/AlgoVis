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
    public class InsertionSortAlgorithm : BaseAlgorithm<ArrayStructure, int[], SortingStep>
    {
        public override string Name => "InsertionSort";

        protected override void ExecuteAlgorithm(AlgorithmConfig config, ArrayStructure structure)
        {
            var array = structure.GetState();
            bool detailed = config.Parameters?.ContainsKey("Detailed") == true &&
                           (bool)config.Parameters["Detailed"];

            // Начальный шаг - показываем исходный массив
            CustomAddStep(array, description: "Начальное состояние массива");

            InsertionSortWithVisualization(array, structure, detailed);

            // Финальный шаг - показываем отсортированный массив
            CustomAddStep(array,
                sorted: Enumerable.Range(0, array.Length).ToArray(),
                description: "Массив полностью отсортирован");
        }

        private void InsertionSortWithVisualization(int[] array, ArrayStructure structure, bool detailed)
        {
            // Первый элемент считается отсортированным
            if (detailed && array.Length > 0)
            {
                CustomAddStep(array,
                    sorted: new[] { 0 },
                    description: "Первый элемент считается отсортированным");
            }

            for (int currentIndex = 1; currentIndex < array.Length; currentIndex++)
            {
                int currentValue = array[currentIndex];
                int compareIndex = currentIndex - 1;

                // Показываем текущий элемент для вставки
                CustomAddStep(array,
                    comparing: new[] { currentIndex },
                    sorted: detailed ? Enumerable.Range(0, currentIndex).ToArray() : null,
                    description: $"Обрабатываем элемент [{currentIndex}] = {currentValue}");

                // Ищем правильную позицию для вставки
                while (compareIndex >= 0 && array[compareIndex] > currentValue)
                {
                    RecordComparison();

                    // Показываем сравнение
                    CustomAddStep(array,
                        comparing: new[] { compareIndex, currentIndex },
                        description: $"Сравниваем [{compareIndex}] = {array[compareIndex]} с {currentValue}");

                    // Сдвигаем элемент вправо
                    array[compareIndex + 1] = array[compareIndex];
                    structure.ApplyState(array);

                    // Показываем сдвиг
                    CustomAddStep(array,
                        swapping: new[] { compareIndex, compareIndex + 1 },
                        description: $"Сдвигаем [{compareIndex}] → [{compareIndex + 1}]");

                    compareIndex--;
                }

                // Вставляем элемент на найденную позицию
                int insertPosition = compareIndex + 1;

                if (insertPosition != currentIndex)
                {
                    array[insertPosition] = currentValue;
                    structure.ApplyState(array);

                    // Показываем вставку
                    CustomAddStep(array,
                        comparing: new[] { insertPosition },
                        swapping: detailed ? new[] { currentIndex, insertPosition } : null,
                        sorted: Enumerable.Range(0, currentIndex + 1).ToArray(),
                        description: $"Вставляем {currentValue} на позицию [{insertPosition}]");
                }
                else
                {
                    // Элемент уже на правильной позиции
                    CustomAddStep(array,
                        sorted: Enumerable.Range(0, currentIndex + 1).ToArray(),
                        description: $"Элемент [{currentIndex}] = {currentValue} уже на правильной позиции");
                }

                RecordSwap();
            }
        }

        // Оптимизированная версия с бинарным поиском для вставки
        public void InsertionSortWithBinarySearch(int[] array, ArrayStructure structure, bool detailed)
        {
            for (int i = 1; i < array.Length; i++)
            {
                int key = array[i];

                CustomAddStep(array,
                    comparing: new[] { i },
                    description: $"Ищем позицию для элемента [{i}] = {key}");

                // Бинарный поиск позиции для вставки
                int position = BinarySearch(array, 0, i - 1, key, structure, detailed);

                // Сдвигаем элементы
                for (int j = i - 1; j >= position; j--)
                {
                    array[j + 1] = array[j];
                    structure.ApplyState(array);

                    CustomAddStep(array,
                        swapping: new[] { j, j + 1 },
                        description: $"Сдвигаем [{j}] → [{j + 1}]");
                }

                // Вставляем элемент
                array[position] = key;
                structure.ApplyState(array);

                CustomAddStep(array,
                    comparing: new[] { position },
                    sorted: Enumerable.Range(0, i + 1).ToArray(),
                    description: $"Вставляем {key} на позицию [{position}]");

                RecordSwap();
            }
        }

        private int BinarySearch(int[] array, int left, int right, int key, ArrayStructure structure, bool detailed)
        {
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                RecordComparison();

                if (detailed)
                {
                    CustomAddStep(array,
                        comparing: new[] { mid },
                        description: $"Бинарный поиск: сравниваем с [{mid}] = {array[mid]}");
                }

                if (array[mid] == key)
                    return mid + 1;
                else if (array[mid] < key)
                    left = mid + 1;
                else
                    right = mid - 1;
            }
            return left;
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
                ["algorithm_type"] = "insertion",
                ["time_complexity_best"] = "O(n)",
                ["time_complexity_avg"] = "O(n²)",
                ["time_complexity_worst"] = "O(n²)",
                ["space_complexity"] = "O(1)",
                ["stable"] = true
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
