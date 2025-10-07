using System;
using System.Collections.Generic;
using System.Linq;
using testing.models;

namespace BubbleSortTester
{
    public static class BubbleSortVisualizer
    {
        public static TestResult Sort(int[] array, bool detailed = true)
        {
            var steps = new List<SortingStep>();
            var workingArray = (int[])array.Clone();
            int comparisons = 0;
            int swaps = 0;

            // Начальное состояние
            steps.Add(new SortingStep
            {
                StepNumber = 1,
                ArrayStep = (int[])workingArray.Clone(),
                Description = "Начальное состояние массива"
            });

            bool swapped;
            int iteration = 0;

            do
            {
                swapped = false;
                iteration++;

                for (int i = 0; i < workingArray.Length - 1; i++)
                {
                    comparisons++;

                    // Шаг сравнения
                    steps.Add(new SortingStep
                    {
                        StepNumber = steps.Count + 1,
                        ArrayStep = (int[])workingArray.Clone(),
                        Comparing = new[] { i, i + 1 },
                        Description = $"Итерация {iteration}: Сравнение [{i}]={workingArray[i]} и [{i + 1}]={workingArray[i + 1]}"
                    });

                    if (workingArray[i] > workingArray[i + 1])
                    {
                        // Обмен элементов
                        (workingArray[i], workingArray[i + 1]) = (workingArray[i + 1], workingArray[i]);
                        swaps++;
                        swapped = true;

                        // Шаг обмена
                        steps.Add(new SortingStep
                        {
                            StepNumber = steps.Count + 1,
                            ArrayStep = (int[])workingArray.Clone(),
                            Swapping = new[] { i, i + 1 },
                            Description = $"Итерация {iteration}: Обмен [{i}] и [{i + 1}]"
                        });
                    }
                }

                // Отметка отсортированных элементов в конце
                if (detailed)
                {
                    var sortedIndices = Enumerable.Range(workingArray.Length - iteration, iteration)
                        .Where(i => i >= 0 && i < workingArray.Length)
                        .ToArray();

                    steps.Add(new SortingStep
                    {
                        StepNumber = steps.Count + 1,
                        ArrayStep = (int[])workingArray.Clone(),
                        Description = $"Завершена итерация {iteration}. Отсортировано элементов: {iteration}"
                    });
                }

            } while (swapped);

            // Финальное состояние
            steps.Add(new SortingStep
            {
                StepNumber = steps.Count + 1,
                ArrayStep = (int[])workingArray.Clone(),
                Description = "Сортировка завершена. Массив полностью отсортирован."
            });

            return new TestResult
            {
                OriginalArray = array,
                SortedArray = workingArray,
                Steps = steps,
                Comparisons = comparisons,
                Swaps = swaps,
                TotalSteps = steps.Count,
                ArraySize = array.Length,
                Efficiency = swaps == 0 ? 1.0 : (double)comparisons / swaps
            };
        }

        public static void AnalyzePerformance(int[] array)
        {
            Console.WriteLine("\n" + new string('─', 50));
            Console.WriteLine("📊 АНАЛИЗ ПРОИЗВОДИТЕЛЬНОСТИ");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // "Тихая" сортировка без записи шагов
            int comparisons = 0;
            int swaps = 0;
            var tempArray = (int[])array.Clone();

            bool swapped;
            do
            {
                swapped = false;
                for (int i = 0; i < tempArray.Length - 1; i++)
                {
                    comparisons++;
                    if (tempArray[i] > tempArray[i + 1])
                    {
                        (tempArray[i], tempArray[i + 1]) = (tempArray[i + 1], tempArray[i]);
                        swaps++;
                        swapped = true;
                    }
                }
            } while (swapped);

            stopwatch.Stop();

            double expectedComparisons = Math.Pow(array.Length, 2);
            double actualVsExpected = comparisons / expectedComparisons;
            double swapsPerComparison = (double)swaps / comparisons;

            Console.WriteLine($"Время выполнения: {stopwatch.Elapsed.TotalMilliseconds:F4} мс");
            Console.WriteLine($"Сравнения: {comparisons} (ожидалось: {expectedComparisons:F2})");
            Console.WriteLine($"Соотношение: {actualVsExpected:P2}");
            Console.WriteLine($"Обмены: {swaps}");
            Console.WriteLine($"Отношение обменов к сравнениям: {swapsPerComparison:P2}");

            if (actualVsExpected < 0.5)
                Console.WriteLine("✅ Алгоритм показал лучшую производительность чем O(n²)");
            else if (actualVsExpected < 0.8)
                Console.WriteLine("⚠️  Алгоритм близок к ожидаемой производительности");
            else
                Console.WriteLine("📊 Алгоритм соответствует ожидаемой сложности O(n²)");
        }
    }
}