using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using testing.models;

namespace BubbleSortTester
{
    class Program
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        static void Main(string[] args)
        {
            Console.Title = "BubbleSort Algorithm Tester";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== 🚀 BubbleSort Algorithm Tester ===");
            Console.ResetColor();

            while (true)
            {
                Console.WriteLine("\n" + new string('═', 60));
                Console.WriteLine("Выберите опцию:");
                Console.WriteLine("1 - Тестирование с случайными массивами");
                Console.WriteLine("2 - Тестирование с пользовательским массивом");
                Console.WriteLine("3 - Серийное тестирование (разные размеры)");
                Console.WriteLine("4 - Загрузка конфигурации из JSON");
                Console.WriteLine("5 - Выход");
                Console.Write("Ваш выбор: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        TestWithRandomArray();
                        break;
                    case "2":
                        TestWithCustomArray();
                        break;
                    case "3":
                        RunSerialTests();
                        break;
                    case "4":
                        LoadFromJson();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }

        static void TestWithRandomArray()
        {
            Console.WriteLine("\n" + new string('─', 40));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🔧 НАСТРОЙКИ ТЕСТИРОВАНИЯ");
            Console.ResetColor();

            Console.Write("Размер массива (по умолчанию 10): ");
            if (!int.TryParse(Console.ReadLine(), out int size) || size <= 0)
                size = 10;

            Console.Write("Минимальное значение (по умолчанию 1): ");
            if (!int.TryParse(Console.ReadLine(), out int min)) min = 1;

            Console.Write("Максимальное значение (по умолчанию 100): ");
            if (!int.TryParse(Console.ReadLine(), out int max)) max = 100;

            Console.Write("Детализированное логирование (y/n, по умолчанию y): ");
            bool detailed = !Console.ReadLine().ToLower().StartsWith('n');

            var array = GenerateRandomArray(size, min, max);
            RunBubbleSortTest(array, detailed, "Случайный массив");
        }

        static void TestWithCustomArray()
        {
            Console.WriteLine("\n" + new string('─', 40));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📝 ВВОД МАССИВА");
            Console.ResetColor();

            Console.WriteLine("Введите числа через пробел (например: 5 3 8 1 9):");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Используется массив по умолчанию: [5, 3, 8, 1, 9]");
                input = "5 3 8 1 9";
            }

            try
            {
                var array = input.Split(' ')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(int.Parse)
                    .ToArray();

                Console.Write("Детализированное логирование (y/n, по умолчанию y): ");
                bool detailed = !Console.ReadLine().ToLower().StartsWith('n');

                RunBubbleSortTest(array, detailed, "Пользовательский массив");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка ввода: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void RunSerialTests()
        {
            Console.WriteLine("\n" + new string('─', 40));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📊 СЕРИЙНОЕ ТЕСТИРОВАНИЕ");
            Console.ResetColor();

            Console.Write("Максимальный размер массива (по умолчанию 20): ");
            if (!int.TryParse(Console.ReadLine(), out int maxSize) || maxSize <= 0)
                maxSize = 20;

            var results = new List<TestResult>();

            for (int size = 5; size <= maxSize; size += size < 20 ? 5 : 10)
            {
                Console.WriteLine($"\nТестирование с размером массива: {size}");
                var array = GenerateRandomArray(size, 1, 100);
                var result = RunBubbleSortTest(array, false, $"Размер {size}", false);
                results.Add(result);
            }

            // Вывод сводной статистики
            Console.WriteLine("\n" + new string('═', 80));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("📈 СВОДНАЯ СТАТИСТИКА");
            Console.ResetColor();

            Console.WriteLine($"{"Размер",-10} {"Время (мс)",-12} {"Сравнения",-12} {"Обмены",-12} {"Шаги",-12} {"O(n^2)",-12}");
            Console.WriteLine(new string('─', 80));

            foreach (var result in results)
            {
                double expectedComplexity = Math.Pow(result.ArraySize, 2);
                double actualPerExpected = result.Comparisons / expectedComplexity;

                Console.WriteLine($"{result.ArraySize,-10} {result.ExecutionTimeMs,-12:F4} {result.Comparisons,-12} {result.Swaps,-12} {result.TotalSteps,-12} {expectedComplexity,-12:F2}");
            }

            // Сохранение результатов
            SaveSerialResults(results);
        }

        static void LoadFromJson()
        {
            Console.WriteLine("\n" + new string('─', 40));
            Console.Write("Введите путь к JSON файлу: ");
            var filePath = Console.ReadLine();

            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var config = JsonSerializer.Deserialize<TestConfig>(json, JsonOptions);

                    if (config != null)
                    {
                        Console.WriteLine($"Конфигурация загружена: {config.Description}");

                        foreach (var testCase in config.TestCases)
                        {
                            var array = testCase.InputArray ?? GenerateRandomArray(testCase.ArraySize, 1, 100);
                            RunBubbleSortTest(array, config.DetailedLogging, testCase.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка загрузки файла: {ex.Message}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Файл не найден.");
                Console.ResetColor();
            }
        }

        static TestResult RunBubbleSortTest(int[] array, bool detailed, string description, bool showResults = true)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = BubbleSortVisualizer.Sort(array, detailed);
            stopwatch.Stop();

            result.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            result.Description = description;
            result.OriginalArray = (int[])array.Clone();

            if (showResults)
            {
                DisplayResults(result);
            }

            SaveToFile(result);
            return result;
        }

        static void DisplayResults(TestResult result)
        {
            Console.WriteLine("\n" + new string('═', 80));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"📊 РЕЗУЛЬТАТЫ: {result.Description}");
            Console.ResetColor();

            Console.WriteLine($"Исходный массив:  [{string.Join(", ", result.OriginalArray)}]");
            Console.WriteLine($"Отсортированный:  [{string.Join(", ", result.SortedArray)}]");
            Console.WriteLine($"Размер массива:   {result.ArraySize}");
            Console.WriteLine($"Время выполнения: {result.ExecutionTimeMs:F4} мс");
            Console.WriteLine();

            // Статистика
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("📈 СТАТИСТИКА:");
            Console.ResetColor();

            Console.WriteLine($"Количество сравнений: {result.Comparisons}");
            Console.WriteLine($"Количество обменов:   {result.Swaps}");
            Console.WriteLine($"Всего шагов:          {result.TotalSteps}");
            Console.WriteLine($"Эффективность:        {result.Efficiency:P2}");

            // Теоретическая сложность
            double expectedComparisons = Math.Pow(result.ArraySize, 2);
            double actualVsExpected = result.Comparisons / expectedComparisons;

            Console.WriteLine($"O(n²) ожидалось:      {expectedComparisons:F2}");
            Console.WriteLine($"Факт/Ожидание:        {actualVsExpected:P2}");

            if (result.Steps.Count <= 50)
            {
                Console.WriteLine("\n" + new string('─', 40));
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("👣 ДЕТАЛИ ШАГОВ:");
                Console.ResetColor();

                for (int i = 0; i < result.Steps.Count; i++)
                {
                    var step = result.Steps[i];
                    Console.Write($"Шаг {step.StepNumber,3}: [{string.Join(", ", step.ArrayStep)}]");

                    if (step.Comparing != null && step.Comparing.Length == 2)
                    {
                        Console.Write($" | Сравнение [{step.Comparing[0]}]={step.ArrayStep[step.Comparing[0]]} и [{step.Comparing[1]}]={step.ArrayStep[step.Comparing[1]]}");
                    }

                    if (step.Swapping != null && step.Swapping.Length == 2)
                    {
                        Console.Write($" | 🔄 Обмен [{step.Swapping[0]}] и [{step.Swapping[1]}]");
                    }

                    if (!string.IsNullOrEmpty(step.Description))
                    {
                        Console.Write($" | {step.Description}");
                    }

                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine($"\nПоказаны первые и последние 10 шагов из {result.Steps.Count}:");

                // Первые 10 шагов
                for (int i = 0; i < Math.Min(10, result.Steps.Count); i++)
                {
                    DisplayStep(result.Steps[i]);
                }

                Console.WriteLine("...");

                // Последние 10 шагов
                for (int i = Math.Max(0, result.Steps.Count - 10); i < result.Steps.Count; i++)
                {
                    DisplayStep(result.Steps[i]);
                }
            }

            // Визуализация прогресса
            if (result.ArraySize <= 20)
            {
                Console.WriteLine("\n" + new string('─', 40));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("🎯 ВИЗУАЛИЗАЦИЯ ПРОГРЕССА:");
                Console.ResetColor();

                var keySteps = result.Steps
                    .Where(s => s.Swapping != null || s.Description?.Contains("итерация") == true)
                    .Take(10)
                    .ToList();

                foreach (var step in keySteps)
                {
                    VisualizeArray(step.ArrayStep, step.Comparing, step.Swapping);
                    Console.WriteLine($"  {step.Description}");
                    Console.WriteLine();
                }
            }
        }

        static void DisplayStep(SortingStep step)
        {
            Console.Write($"Шаг {step.StepNumber,3}: ");

            if (step.Comparing != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Сравнение ");
                Console.ResetColor();
            }

            if (step.Swapping != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Обмен ");
                Console.ResetColor();
            }

            Console.WriteLine($"{step.Description}");
        }

        static void VisualizeArray(int[] array, int[] comparing = null, int[] swapping = null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (swapping != null && swapping.Contains(i))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"[{array[i],2}] ");
                    Console.ResetColor();
                }
                else if (comparing != null && comparing.Contains(i))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[{array[i],2}] ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write($"[{array[i],2}] ");
                }
            }
        }

        static int[] GenerateRandomArray(int size, int min, int max)
        {
            var random = new Random();
            return Enumerable.Range(0, size)
                .Select(_ => random.Next(min, max + 1))
                .ToArray();
        }

        static void SaveToFile(TestResult result)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"bubblesort_result_{timestamp}.json";

                var output = new
                {
                    result.Description,
                    result.OriginalArray,
                    result.SortedArray,
                    result.ArraySize,
                    result.ExecutionTimeMs,
                    Statistics = new
                    {
                        result.Comparisons,
                        result.Swaps,
                        result.TotalSteps,
                        result.Efficiency
                    },
                    Steps = result.Steps.Take(100).ToList() // Ограничиваем для файла
                };

                var json = JsonSerializer.Serialize(output, JsonOptions);
                File.WriteAllText(fileName, json);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"💾 Результаты сохранены в: {fileName}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void SaveSerialResults(List<TestResult> results)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"bubblesort_serial_{timestamp}.json";

                var summary = results.Select(r => new
                {
                    r.Description,
                    r.ArraySize,
                    r.ExecutionTimeMs,
                    r.Comparisons,
                    r.Swaps,
                    r.TotalSteps,
                    r.Efficiency,
                    ExpectedComplexity = Math.Pow(r.ArraySize, 2)
                }).ToList();

                var json = JsonSerializer.Serialize(summary, JsonOptions);
                File.WriteAllText(fileName, json);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"💾 Серийные результаты сохранены в: {fileName}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}