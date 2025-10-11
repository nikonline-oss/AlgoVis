using MyMenuSystem;
using System.Text;
using System.Text.Json;
using testing.Models.Core;
using testing.Models.Custom;
using testing.Models.DataStructures;
using testing.Models.Visualization;
using testing.Services.Core;
using testing.Support;

namespace AlgorithmVisualization
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Algorithm Testing Console";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== 🚀 Algorithm Testing Console ===");
            Console.ResetColor();

            IMenu mainMenu = new AlgorithmTestingMenu();
            mainMenu.Run();
        }
    }

    public class AlgorithmTestingMenu : IMenu
    {
        private MenuManager _menuManager;
        private AlgorithmManager _algorithmManager;
        private Dictionary<string, object> _testData;
        private string _resultsDirectory;

        public AlgorithmTestingMenu()
        {
            _menuManager = new MenuManager(this);
            _algorithmManager = new AlgorithmManager();
            _testData = new Dictionary<string, object>();
            _resultsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Results");

            Directory.CreateDirectory(_resultsDirectory);
            SetupMainMenu();
        }

        private void SetupMainMenu()
        {
            _menuManager.LastTextShowMenu = "Выйти из программы";
            _menuManager.ActionExite = () => Environment.Exit(0);

            // Основные пункты меню
            _menuManager.AddMenuItem("Настроить тестовые данные", SetupTestData);
            _menuManager.AddMenuItem("Тестировать BubbleSort", TestBubbleSort);
            _menuManager.AddMenuItem("Тестировать QuickSort", TestQuickSort);
            _menuManager.AddMenuItem("Загрузить кастомный алгоритм", LoadCustomAlgorithm);
            _menuManager.AddMenuItem("Создать кастомный алгоритм", CreateCustomAlgorithm);
            _menuManager.AddMenuItem("Сравнить производительность", CompareAlgorithms);
            _menuManager.AddMenuItem("Показать статистику", ShowStatistics);
            _menuManager.AddMenuItem("Сохранить результаты", SaveResultsMenu);
            _menuManager.AddMenuItem("Очистить данные", ClearTestData);
        }

        public void ShowMenu() => _menuManager.ShowMenu();
        public void HandleChoice(int choice) => _menuManager.HandleChoice(choice);
        public void Exit() => _menuManager.Exit();
        public void BackToMainMenu() => _menuManager.BackToMainMenu();
        public void Run() => _menuManager.Run();

        private void SetupTestData()
        {
            var setupMenu = new MenuManager(this);
            setupMenu.SubMenuBool = true;
            setupMenu.LastTextShowMenu = "Назад в главное меню";

            setupMenu.AddMenuItem("Сгенерировать случайный массив", GenerateRandomArray);
            setupMenu.AddMenuItem("Ввести массив вручную", InputArrayManually);
            setupMenu.AddMenuItem("Использовать тестовые примеры", UseTestCases);
            setupMenu.AddMenuItem("Показать текущие данные", ShowCurrentData);

            setupMenu.Run();
        }

        private void LoadCustomAlgorithm()
        {
            Console.WriteLine("\n=== Загрузка кастомного алгоритма ===");
            Console.Write("Введите путь к JSON файлу с алгоритмом: ");

            var filePath = Console.ReadLine();

            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var customAlgorithm = JsonSerializer.Deserialize<CustomAlgorithmRequest>(json);

                    if (customAlgorithm != null)
                    {
                        ExecuteCustomAlgorithm(customAlgorithm);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка загрузки алгоритма: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("❌ Файл не найден.");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void CreateCustomAlgorithm()
        {
            Console.WriteLine("\n=== Создание кастомного алгоритма ===");
            Console.WriteLine("Эта функция в разработке...");
            Console.WriteLine("Сейчас вы можете создать JSON файл по шаблону и загрузить его.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ExecuteCustomAlgorithm(CustomAlgorithmRequest customAlgorithm)
        {
            if (!ValidateTestData()) return;

            var array = (int[])_testData["current_array"];
            var arrayStructure = StructureFactory.CreateStructure("array", array);

            try
            {
                Console.WriteLine($"\n=== Выполнение кастомного алгоритма: {customAlgorithm.name} ===");
                Console.WriteLine($"Описание: {customAlgorithm.description}");

                var result = _algorithmManager.ExecuteCustomAlgorithm(customAlgorithm, arrayStructure);

                if (result.success)
                {
                    Console.WriteLine("✅ Алгоритм выполнен успешно!");
                    DisplayAlgorithmResult(result.result, array);

                    // Сохраняем результат
                    _testData["last_result"] = result.result;
                    _testData["custom_algorithm"] = customAlgorithm;
                }
                else
                {
                    Console.WriteLine($"❌ Ошибка: {result.message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка выполнения: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void GenerateRandomArray()
        {
            Console.WriteLine("\n=== Генерация случайного массива ===");

            Console.Write("Размер массива (по умолчанию 10): ");
            if (!int.TryParse(Console.ReadLine(), out int size) || size <= 0) size = 10;

            Console.Write("Минимальное значение (по умолчанию 1): ");
            if (!int.TryParse(Console.ReadLine(), out int min)) min = 1;

            Console.Write("Максимальное значение (по умолчанию 100): ");
            if (!int.TryParse(Console.ReadLine(), out int max)) max = 100;

            var random = new Random();
            var array = new int[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = random.Next(min, max + 1);
            }

            _testData["current_array"] = array;
            _testData["array_name"] = "Случайный массив";

            Console.WriteLine($"\n✅ Сгенерирован массив: [{string.Join(", ", array)}]");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void InputArrayManually()
        {
            Console.WriteLine("\n=== Ввод массива вручную ===");
            Console.WriteLine("Введите числа через пробел (например: 5 3 8 1 9):");

            try
            {
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Используется массив по умолчанию: [5, 3, 8, 1, 9]");
                    input = "5 3 8 1 9";
                }

                var array = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToArray();

                _testData["current_array"] = array;
                _testData["array_name"] = "Пользовательский массив";

                Console.WriteLine($"\n✅ Сохранен массив: [{string.Join(", ", array)}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка ввода: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void UseTestCases()
        {
            var testMenu = new MenuManager(this);
            testMenu.SubMenuBool = true;
            testMenu.LastTextShowMenu = "Назад";

            // Предопределенные тестовые случаи
            var testCases = new Dictionary<string, int[]>
            {
                ["Уже отсортированный"] = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                ["Обратно отсортированный"] = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                ["Случайный небольшой"] = new[] { 64, 34, 25, 12, 22, 11, 90 },
                ["С повторяющимися элементами"] = new[] { 5, 2, 5, 1, 2, 3, 5, 2, 1, 4 },
                ["Большой массив (20 элементов)"] = GenerateLargeArray(20),
                ["Почти отсортированный"] = new[] { 1, 2, 3, 5, 4, 6, 7, 9, 8, 10 }
            };

            foreach (var testCase in testCases)
            {
                testMenu.AddMenuItem(testCase.Key, () => UseTestCase(testCase.Key, testCase.Value));
            }

            testMenu.Run();
        }

        private void UseTestCase(string name, int[] array)
        {
            _testData["current_array"] = array;
            _testData["array_name"] = name;
            Console.WriteLine($"\n✅ Используется тестовый случай: {name}");
            Console.WriteLine($"Массив: [{string.Join(", ", array)}]");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ShowCurrentData()
        {
            Console.WriteLine("\n=== Текущие тестовые данные ===");

            if (_testData.ContainsKey("current_array") && _testData["current_array"] is int[] array)
            {
                Console.WriteLine($"Название: {_testData["array_name"]}");
                Console.WriteLine($"Размер: {array.Length} элементов");
                Console.WriteLine($"Массив: [{string.Join(", ", array)}]");

                // Проверяем отсортирован ли массив
                bool isSorted = IsSorted(array);
                Console.WriteLine($"Статус: {(isSorted ? "✓ Отсортирован" : "✗ Не отсортирован")}");
            }
            else
            {
                Console.WriteLine("❌ Тестовые данные не настроены. Сначала настройте массив.");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void TestBubbleSort()
        {
            if (!ValidateTestData()) return;

            var array = (int[])_testData["current_array"];
            var arrayStructure = StructureFactory.CreateStructure("array", array);

            var config = new AlgorithmConfig
            {
                Name = "BubbleSort",
                SessionId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, object>
                {
                    ["Detailed"] = true,
                    ["TrackSwaps"] = true
                }
            };

            RunAlgorithm("BubbleSort", config, arrayStructure, array);
        }

        private void TestQuickSort()
        {
            if (!ValidateTestData()) return;

            var array = (int[])_testData["current_array"];
            var arrayStructure = StructureFactory.CreateStructure("array", array);

            // Выбор стратегии опорного элемента
            Console.WriteLine("\n=== Выбор стратегии для QuickSort ===");
            Console.WriteLine("1 - Последний элемент (по умолчанию)");
            Console.WriteLine("2 - Первый элемент");
            Console.WriteLine("3 - Средний элемент");
            Console.WriteLine("4 - Случайный элемент");
            Console.WriteLine("5 - Медиана трех");
            Console.Write("Ваш выбор: ");

            var strategyChoice = Console.ReadLine();
            var strategy = strategyChoice switch
            {
                "1" => "last",
                "2" => "first",
                "3" => "middle",
                "4" => "random",
                "5" => "median",
                _ => "last"
            };

            var config = new AlgorithmConfig
            {
                Name = "QuickSort",
                SessionId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, object>
                {
                    ["Detailed"] = true,
                    ["PivotStrategy"] = strategy
                }
            };

            RunAlgorithm("QuickSort", config, arrayStructure, array);
        }

        private void CompareAlgorithms()
        {
            if (!ValidateTestData()) return;

            var array = (int[])_testData["current_array"];
            var algorithms = new[] { "BubbleSort", "QuickSort" };
            var results = new List<AlgorithmResult>();

            Console.WriteLine("\n=== Сравнение производительности ===");
            Console.WriteLine($"Массив: {_testData["array_name"]}");
            Console.WriteLine($"Размер: {array.Length} элементов\n");

            foreach (var algorithmName in algorithms)
            {
                var arrayStructure = StructureFactory.CreateStructure("array", (int[])array.Clone());
                var config = new AlgorithmConfig
                {
                    Name = algorithmName,
                    SessionId = Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, object> { ["Detailed"] = false }
                };

                var result = _algorithmManager.ExecuteAlgorithm(config, arrayStructure);
                results.Add(result);

                Console.WriteLine($"📊 {algorithmName}:");
                Console.WriteLine($"   Время: {result.ExecutionTime.TotalMilliseconds:F4} мс");
                Console.WriteLine($"   Сравнений: {result.Statistics.Comparisons}");
                Console.WriteLine($"   Обменов: {result.Statistics.Swaps}");
                Console.WriteLine($"   Шагов: {result.Statistics.Steps}");
                Console.WriteLine();
            }

            // Сохранение результатов для отображения в статистике
            _testData["comparison_results"] = results;

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ShowStatistics()
        {
            Console.WriteLine("\n=== Статистика тестирования ===");

            if (_testData.ContainsKey("comparison_results") &&
                _testData["comparison_results"] is List<AlgorithmResult> results)
            {
                Console.WriteLine("📈 Сравнительная статистика:\n");

                foreach (var result in results)
                {
                    Console.WriteLine($"🔹 {result.AlgorithmName}:");
                    Console.WriteLine($"   ⏱️  Время: {result.ExecutionTime.TotalMilliseconds:F4} мс");
                    Console.WriteLine($"   🔄 Сравнений: {result.Statistics.Comparisons}");
                    Console.WriteLine($"   🔁 Обменов: {result.Statistics.Swaps}");
                    Console.WriteLine($"   📝 Шагов: {result.Statistics.Steps}");
                    Console.WriteLine($"   📊 Эффективность: {CalculateEfficiency(result):F2} сравнений/обмен");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("❌ Нет данных для сравнения. Сначала выполните сравнение алгоритмов.");
            }

            if (_testData.ContainsKey("current_array") && _testData["current_array"] is int[] array)
            {
                Console.WriteLine("📋 Информация о массиве:");
                Console.WriteLine($"   Размер: {array.Length} элементов");
                Console.WriteLine($"   Диапазон значений: {array.Min()} - {array.Max()}");
                Console.WriteLine($"   Уникальных значений: {array.Distinct().Count()}");
                Console.WriteLine($"   Отсортирован: {(IsSorted(array) ? "✓ Да" : "✗ Нет")}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ClearTestData()
        {
            _testData.Clear();
            Console.WriteLine("✅ Все тестовые данные очищены.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void RunAlgorithm(string algorithmName, AlgorithmConfig config, IDataStructure structure, int[] originalArray)
        {
            try
            {
                Console.WriteLine($"\n=== Запуск {algorithmName} ===");
                Console.WriteLine($"Массив: [{string.Join(", ", originalArray)}]");
                Console.WriteLine("Выполнение...\n");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = _algorithmManager.ExecuteAlgorithm(config, structure);
                stopwatch.Stop();

                DisplayAlgorithmResult(result, originalArray);

                // Сохранение последнего результата
                _testData["last_result"] = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка выполнения алгоритма: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void DisplayAlgorithmResult(AlgorithmResult result, int[] originalArray)
        {
            Console.WriteLine("🎯 РЕЗУЛЬТАТЫ АЛГОРИТМА:");
            Console.WriteLine(new string('═', 50));

            Console.WriteLine($"Алгоритм: {result.AlgorithmName}");
            Console.WriteLine($"Размер массива: {originalArray.Length}");
            Console.WriteLine($"Время выполнения: {result.ExecutionTime.TotalMilliseconds:F4} мс");
            Console.WriteLine();

            Console.WriteLine("📊 СТАТИСТИКА:");
            Console.WriteLine($"   Сравнений: {result.Statistics.Comparisons}");
            Console.WriteLine($"   Обменов: {result.Statistics.Swaps}");
            Console.WriteLine($"   Шагов: {result.Statistics.Steps}");

            if (result.Statistics.RecursiveCalls > 0)
                Console.WriteLine($"   Рекурсивных вызовов: {result.Statistics.RecursiveCalls}");

            Console.WriteLine($"   Эффективность: {CalculateEfficiency(result):F2} сравнений/обмен");
            Console.WriteLine();

            // Теоретическая сложность
            double expectedComplexity = result.AlgorithmName == "BubbleSort"
                ? Math.Pow(originalArray.Length, 2)
                : originalArray.Length * Math.Log(originalArray.Length);

            double actualVsExpected = result.Statistics.Comparisons / expectedComplexity;

            Console.WriteLine("📈 АНАЛИЗ СЛОЖНОСТИ:");
            Console.WriteLine($"   O({(result.AlgorithmName == "BubbleSort" ? "n²" : "n log n")}) ожидалось: {expectedComplexity:F2}");
            Console.WriteLine($"   Факт/Ожидание: {actualVsExpected:P2}");

            if (result.OutputData.ContainsKey("sorted_array") && result.OutputData["sorted_array"] is int[] sortedArray)
            {
                Console.WriteLine();
                Console.WriteLine("📋 РЕЗУЛЬТАТ:");
                Console.WriteLine($"   Исходный: [{string.Join(", ", originalArray)}]");
                Console.WriteLine($"   Отсортированный: [{string.Join(", ", sortedArray)}]");
                Console.WriteLine($"   Корректность: {(IsSorted(sortedArray) ? "✓ Успешно" : "✗ Ошибка")}");
            }
        }


        private bool ValidateTestData()
        {
            if (!_testData.ContainsKey("current_array"))
            {
                Console.WriteLine("❌ Сначала настройте тестовые данные в разделе 'Настроить тестовые данные'");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return false;
            }
            return true;
        }

        private static int[] GenerateLargeArray(int size)
        {
            var random = new Random();
            var array = new int[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = random.Next(1, 1000);
            }
            return array;
        }

        private static bool IsSorted(int[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                if (array[i] > array[i + 1])
                    return false;
            }
            return true;
        }

        private static double CalculateEfficiency(AlgorithmResult result)
        {
            return result.Statistics.Swaps > 0 ? (double)result.Statistics.Comparisons / result.Statistics.Swaps : result.Statistics.Comparisons;
        }


        private void SaveResultsMenu()
        {
            var saveMenu = new MenuManager(this);
            saveMenu.SubMenuBool = true;
            saveMenu.LastTextShowMenu = "Назад в главное меню";

            saveMenu.AddMenuItem("Сохранить последний результат", SaveLastResult);
            saveMenu.AddMenuItem("Сохранить сравнение алгоритмов", SaveComparisonResults);
            saveMenu.AddMenuItem("Сохранить все данные в JSON", SaveAllDataAsJson);
            saveMenu.AddMenuItem("Сохранить статистику в TXT", SaveStatisticsAsTxt);
            saveMenu.AddMenuItem("Сохранить в CSV формат", SaveAsCsv);
            saveMenu.AddMenuItem("Сохранить шаги алгоритма", SaveAlgorithmSteps);
            saveMenu.AddMenuItem("Показать историю сохранений", ShowSaveHistory);

            saveMenu.Run();
        }
        private void SaveComparisonResults()
        {
            if (!_testData.ContainsKey("comparison_results") ||
                _testData["comparison_results"] is not List<AlgorithmResult> results ||
                results.Count == 0)
            {
                Console.WriteLine("❌ Нет данных для сравнения. Сначала выполните сравнение алгоритмов.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"Comparison_{timestamp}";

            // Сохраняем в разных форматах
            SaveComparisonAsJson(results, fileName);
            SaveComparisonAsTxt(results, fileName);
            SaveComparisonAsCsv(results, fileName);

            Console.WriteLine($"✅ Результаты сравнения сохранены в файлы:");
            Console.WriteLine($"   📄 {fileName}.json");
            Console.WriteLine($"   📄 {fileName}.txt");
            Console.WriteLine($"   📊 {fileName}.csv");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void SaveAllDataAsJson()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"FullData_{timestamp}.json";
            var filePath = Path.Combine(_resultsDirectory, fileName);

            try
            {
                var saveData = new
                {
                    Timestamp = DateTime.Now,
                    TestData = _testData.ContainsKey("current_array") ? new
                    {
                        ArrayName = _testData["array_name"],
                        Array = _testData["current_array"],
                        ArraySize = _testData["current_array"] is int[] arr ? arr.Length : 0
                    } : null,
                    LastResult = _testData.ContainsKey("last_result") ?
                        SerializeAlgorithmResult(_testData["last_result"] as AlgorithmResult) : null,
                    ComparisonResults = _testData.ContainsKey("comparison_results") ?
                        (_testData["comparison_results"] as List<AlgorithmResult>)?.Select(SerializeAlgorithmResult)
                                                                                   .ToList() : null
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(saveData, options);
                File.WriteAllText(filePath, json);

                Console.WriteLine($"✅ Все данные сохранены в: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
        // Вспомогательные методы
        private object SerializeAlgorithmResult(AlgorithmResult result)
        {
            return new
            {
                result.AlgorithmName,
                result.SessionId,
                result.StructureType,
                result.ExecutionTime,
                Statistics = new
                {
                    result.Statistics.Comparisons,
                    result.Statistics.Swaps,
                    result.Statistics.Steps,
                    result.Statistics.RecursiveCalls,
                    result.Statistics.MemoryOperations,
                    Efficiency = CalculateEfficiency(result)
                },
                result.OutputData,
                StepsCount = result.Steps.Count
                // Не сохраняем сами шаги для экономии места
            };
        }

        private void SaveComparisonAsJson(List<AlgorithmResult> results, string baseFileName)
        {
            var filePath = Path.Combine(_resultsDirectory, baseFileName + ".json");
            var comparisonData = new
            {
                Timestamp = DateTime.Now,
                TestData = _testData.ContainsKey("current_array") ? new
                {
                    ArrayName = _testData["array_name"],
                    ArraySize = _testData["current_array"] is int[] arr ? arr.Length : 0
                } : null,
                Results = results.Select(SerializeAlgorithmResult).ToList()
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(comparisonData, options);
            File.WriteAllText(filePath, json);
        }

        private void SaveComparisonAsTxt(List<AlgorithmResult> results, string baseFileName)
        {
            var filePath = Path.Combine(_resultsDirectory, baseFileName + ".txt");

            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine("=== СРАВНЕНИЕ АЛГОРИТМОВ СОРТИРОВКИ ===");
            writer.WriteLine($"Дата сравнения: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            writer.WriteLine();

            if (_testData.ContainsKey("current_array") && _testData["current_array"] is int[] array)
            {
                writer.WriteLine($"Тестовые данные: {_testData["array_name"]}");
                writer.WriteLine($"Размер массива: {array.Length} элементов");
                writer.WriteLine();
            }

            writer.WriteLine(new string('═', 100));
            writer.WriteLine($"{"Алгоритм",-15} {"Время (мс)",-12} {"Сравнения",-12} {"Обмены",-12} {"Шаги",-12} {"Рекурсия",-10} {"Эффективность",-15}");
            writer.WriteLine(new string('═', 100));

            foreach (var result in results)
            {
                writer.WriteLine($"{result.AlgorithmName,-15} {result.ExecutionTime.TotalMilliseconds,-12:F4} " +
                               $"{result.Statistics.Comparisons,-12} {result.Statistics.Swaps,-12} " +
                               $"{result.Statistics.Steps,-12} {result.Statistics.RecursiveCalls,-10} " +
                               $"{CalculateEfficiency(result),-15:F2}");
            }

            // Определяем победителя
            var fastest = results.OrderBy(r => r.ExecutionTime).First();
            var mostEfficient = results.OrderByDescending(r => CalculateEfficiency(r)).First();

            writer.WriteLine();
            writer.WriteLine("АНАЛИЗ РЕЗУЛЬТАТОВ:");
            writer.WriteLine($"  🏆 Самый быстрый: {fastest.AlgorithmName} ({fastest.ExecutionTime.TotalMilliseconds:F4} мс)");
            writer.WriteLine($"  📊 Самый эффективный: {mostEfficient.AlgorithmName} ({CalculateEfficiency(mostEfficient):F2} сравнений/обмен)");
        }

        private void SaveComparisonAsCsv(List<AlgorithmResult> results, string baseFileName)
        {
            var filePath = Path.Combine(_resultsDirectory, baseFileName + ".csv");

            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            // Заголовок CSV
            writer.WriteLine("Algorithm;TimeMs;Comparisons;Swaps;Steps;RecursiveCalls;Efficiency;ArraySize;Timestamp");

            // Данные
            foreach (var result in results)
            {
                var arraySize = _testData.ContainsKey("current_array") && _testData["current_array"] is int[] arr
                    ? arr.Length : 0;

                writer.WriteLine(
                    $"{result.AlgorithmName};" +
                    $"{result.ExecutionTime.TotalMilliseconds:F4};" +
                    $"{result.Statistics.Comparisons};" +
                    $"{result.Statistics.Swaps};" +
                    $"{result.Statistics.Steps};" +
                    $"{result.Statistics.RecursiveCalls};" +
                    $"{CalculateEfficiency(result):F2};" +
                    $"{arraySize};" +
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                );
            }
        }

        private void SaveStatisticsAsTxt()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"Statistics_{timestamp}.txt";
            var filePath = Path.Combine(_resultsDirectory, fileName);

            try
            {
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

                writer.WriteLine("=== СТАТИСТИКА ТЕСТИРОВАНИЯ АЛГОРИТМОВ ===");
                writer.WriteLine($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                writer.WriteLine();

                // Информация о тестовых данных
                if (_testData.ContainsKey("current_array") && _testData["current_array"] is int[] array)
                {
                    writer.WriteLine("ТЕСТОВЫЕ ДАННЫЕ:");
                    writer.WriteLine($"  Название: {_testData["array_name"]}");
                    writer.WriteLine($"  Размер массива: {array.Length}");
                    writer.WriteLine($"  Диапазон значений: {array.Min()} - {array.Max()}");
                    writer.WriteLine($"  Уникальных значений: {array.Distinct().Count()}");
                    writer.WriteLine($"  Отсортирован: {(IsSorted(array) ? "Да" : "Нет")}");
                    writer.WriteLine($"  Массив: [{string.Join(", ", array)}]");
                    writer.WriteLine();
                }

                // Результаты сравнения
                if (_testData.ContainsKey("comparison_results") &&
                    _testData["comparison_results"] is List<AlgorithmResult> results)
                {
                    writer.WriteLine("СРАВНИТЕЛЬНАЯ СТАТИСТИКА:");
                    writer.WriteLine(new string('─', 80));
                    writer.WriteLine($"{"Алгоритм",-15} {"Время (мс)",-12} {"Сравнения",-12} {"Обмены",-12} {"Шаги",-12} {"Эффективность",-15}");
                    writer.WriteLine(new string('─', 80));

                    foreach (var result in results)
                    {
                        writer.WriteLine($"{result.AlgorithmName,-15} {result.ExecutionTime.TotalMilliseconds,-12:F4} " +
                                       $"{result.Statistics.Comparisons,-12} {result.Statistics.Swaps,-12} " +
                                       $"{result.Statistics.Steps,-12} {CalculateEfficiency(result),-15:F2}");
                    }
                    writer.WriteLine();
                }

                // Последний результат
                if (_testData.ContainsKey("last_result") && _testData["last_result"] is AlgorithmResult lastResult)
                {
                    writer.WriteLine("ПОСЛЕДНИЙ РЕЗУЛЬТАТ:");
                    writer.WriteLine($"  Алгоритм: {lastResult.AlgorithmName}");
                    writer.WriteLine($"  Время выполнения: {lastResult.ExecutionTime.TotalMilliseconds:F4} мс");
                    writer.WriteLine($"  Сравнений: {lastResult.Statistics.Comparisons}");
                    writer.WriteLine($"  Обменов: {lastResult.Statistics.Swaps}");
                    writer.WriteLine($"  Шагов: {lastResult.Statistics.Steps}");
                    if (lastResult.Statistics.RecursiveCalls > 0)
                        writer.WriteLine($"  Рекурсивных вызовов: {lastResult.Statistics.RecursiveCalls}");
                    writer.WriteLine();
                }

                writer.WriteLine("Конец отчета");
                Console.WriteLine($"✅ Статистика сохранена в: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void SaveAsCsv()
        {
            if (!_testData.ContainsKey("comparison_results") ||
                _testData["comparison_results"] is not List<AlgorithmResult> results ||
                results.Count == 0)
            {
                Console.WriteLine("❌ Нет данных для сохранения в CSV. Сначала выполните сравнение алгоритмов.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"Comparison_{timestamp}.csv";
            var filePath = Path.Combine(_resultsDirectory, fileName);

            try
            {
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

                // Заголовок CSV
                writer.WriteLine("Algorithm,TimeMs,Comparisons,Swaps,Steps,RecursiveCalls,Efficiency,ArraySize,Timestamp");

                // Данные
                foreach (var result in results)
                {
                    var arraySize = _testData.ContainsKey("current_array") && _testData["current_array"] is int[] arr
                        ? arr.Length : 0;

                    writer.WriteLine(
                        $"{EscapeCsv(result.AlgorithmName)}," +
                        $"{result.ExecutionTime.TotalMilliseconds:F4}," +
                        $"{result.Statistics.Comparisons}," +
                        $"{result.Statistics.Swaps}," +
                        $"{result.Statistics.Steps}," +
                        $"{result.Statistics.RecursiveCalls}," +
                        $"{CalculateEfficiency(result):F2}," +
                        $"{arraySize}," +
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                    );
                }

                Console.WriteLine($"✅ Данные сохранены в CSV: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения CSV: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ShowSaveHistory()
        {
            Console.WriteLine("\n=== ИСТОРИЯ СОХРАНЕНИЙ ===");

            try
            {
                var files = Directory.GetFiles(_resultsDirectory)
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(10) // Показываем последние 10 файлов
                    .ToList();

                if (files.Count == 0)
                {
                    Console.WriteLine("📁 Файлы результатов не найдены.");
                }
                else
                {
                    Console.WriteLine($"Последние {files.Count} файлов в папке Results:");
                    Console.WriteLine(new string('─', 80));
                    Console.WriteLine($"{"Имя файла",-30} {"Размер",-10} {"Дата создания"}");
                    Console.WriteLine(new string('─', 80));

                    foreach (var file in files)
                    {
                        Console.WriteLine($"{file.Name,-30} {FormatFileSize(file.Length),-10} {file.CreationTime:dd.MM.yy HH:mm}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка чтения истории: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // Новый метод для сохранения шагов алгоритма
        private void SaveAlgorithmSteps()
        {
            if (!_testData.ContainsKey("last_result"))
            {
                Console.WriteLine("❌ Нет результатов для сохранения. Сначала выполните тестирование алгоритма.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = _testData["last_result"] as AlgorithmResult;
            if (result == null || result.Steps.Count == 0)
            {
                Console.WriteLine("❌ Нет шагов алгоритма для сохранения.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var stepsMenu = new MenuManager(this);
            stepsMenu.SubMenuBool = true;
            stepsMenu.LastTextShowMenu = "Назад";

            stepsMenu.AddMenuItem("Сохранить все шаги в JSON", () => SaveStepsAsJson(result, true));
            stepsMenu.AddMenuItem("Сохранить все шаги в TXT", () => SaveStepsAsTxt(result, true));
            stepsMenu.AddMenuItem("Сохранить ключевые шаги в JSON", () => SaveStepsAsJson(result, false));
            stepsMenu.AddMenuItem("Сохранить ключевые шаги в TXT", () => SaveStepsAsTxt(result, false));
            stepsMenu.AddMenuItem("Сохранить шаги для визуализации", SaveStepsForVisualization);

            stepsMenu.Run();
        }

        private void SaveStepsAsJson(AlgorithmResult result, bool allSteps)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = allSteps ?
                $"{result.AlgorithmName}_AllSteps_{timestamp}.json" :
                $"{result.AlgorithmName}_KeySteps_{timestamp}.json";

            var stepsToSave = allSteps ?
                result.Steps :
                result.Steps.Where(s => IsKeyStep(s)).ToList();

            var stepsData = new
            {
                AlgorithmName = result.AlgorithmName,
                Timestamp = DateTime.Now,
                TotalSteps = result.Steps.Count,
                SavedSteps = stepsToSave.Count,
                Steps = stepsToSave.Select(step => new
                {
                    step.StepNumber,
                    step.Operation,
                    step.Description,
                    ArrayState = step.VisualizationData.Elements.ContainsKey("0") ?
                        GetArrayFromVisualizationData(step.VisualizationData) : null,
                    Comparing = step.VisualizationData.Highlights
                        .Where(h => h.HighlightType == "comparing")
                        .Select(h => h.ElementId).ToArray(),
                    Swapping = step.VisualizationData.Highlights
                        .Where(h => h.HighlightType == "swapping")
                        .Select(h => h.ElementId).ToArray(),
                    Sorted = step.VisualizationData.Highlights
                        .Where(h => h.HighlightType == "sorted")
                        .Select(h => h.ElementId).ToArray(),
                    PivotIndex = step.VisualizationData.Highlights
                        .Where(h => h.HighlightType == "pivot")
                        .Select(h => h.ElementId).FirstOrDefault(),
                    step.Metadata
                }).ToList()
            };

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(stepsData, options);
                var filePath = Path.Combine(_resultsDirectory, fileName);
                File.WriteAllText(filePath, json);

                Console.WriteLine($"✅ Шаги алгоритма сохранены в: {fileName}");
                Console.WriteLine($"📊 Сохранено шагов: {stepsToSave.Count} из {result.Steps.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения шагов: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void SaveStepsAsTxt(AlgorithmResult result, bool allSteps)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = allSteps ?
                $"{result.AlgorithmName}_AllSteps_{timestamp}.txt" :
                $"{result.AlgorithmName}_KeySteps_{timestamp}.txt";

            var stepsToSave = allSteps ?
                result.Steps :
                result.Steps.Where(s => IsKeyStep(s)).ToList();

            var filePath = Path.Combine(_resultsDirectory, fileName);

            try
            {
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

                writer.WriteLine("=== ШАГИ АЛГОРИТМА ===");
                writer.WriteLine($"Алгоритм: {result.AlgorithmName}");
                writer.WriteLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                writer.WriteLine($"Всего шагов: {result.Steps.Count}");
                writer.WriteLine($"Сохранено шагов: {stepsToSave.Count}");
                writer.WriteLine();

                foreach (var step in stepsToSave)
                {
                    writer.WriteLine($"Шаг {step.StepNumber}: {step.Operation}");
                    writer.WriteLine($"  Описание: {step.Description}");

                    // Состояние массива
                    if (step.VisualizationData.Elements.ContainsKey("0"))
                    {
                        var array = GetArrayFromVisualizationData(step.VisualizationData);
                        writer.WriteLine($"  Массив: [{string.Join(", ", array)}]");
                    }

                    // Подсвеченные элементы
                    var highlights = step.VisualizationData.Highlights
                        .GroupBy(h => h.HighlightType)
                        .Select(g => $"{g.Key}: {string.Join(", ", g.Select(h => h.ElementId))}");

                    if (highlights.Any())
                    {
                        writer.WriteLine($"  Подсветка: {string.Join("; ", highlights)}");
                    }

                    // Метаданные
                    if (step.Metadata.Any())
                    {
                        writer.WriteLine($"  Метаданные: {string.Join(", ", step.Metadata.Select(kv => $"{kv.Key}={kv.Value}"))}");
                    }

                    writer.WriteLine();
                }

                Console.WriteLine($"✅ Шаги алгоритма сохранены в: {fileName}");
                Console.WriteLine($"📊 Сохранено шагов: {stepsToSave.Count} из {result.Steps.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения шагов: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void SaveStepsForVisualization()
        {
            if (!_testData.ContainsKey("last_result"))
            {
                Console.WriteLine("❌ Нет результатов для сохранения.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = _testData["last_result"] as AlgorithmResult;
            if (result == null) return;

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{result.AlgorithmName}_Visualization_{timestamp}.json";

            try
            {
                var visualizationData = new
                {
                    AlgorithmName = result.AlgorithmName,
                    OriginalArray = _testData.ContainsKey("current_array") ? _testData["current_array"] : null,
                    Statistics = new
                    {
                        result.Statistics.Comparisons,
                        result.Statistics.Swaps,
                        result.Statistics.Steps,
                        result.Statistics.RecursiveCalls,
                        ExecutionTime = result.ExecutionTime.TotalMilliseconds
                    },
                    Steps = result.Steps.Select(step => new
                    {
                        step.StepNumber,
                        step.Operation,
                        step.Description,
                        Array = GetArrayFromVisualizationData(step.VisualizationData),
                        Highlights = step.VisualizationData.Highlights.Select(h => new
                        {
                            h.ElementId,
                            h.HighlightType,
                            h.Color,
                            h.Label
                        }).ToList(),
                        Connections = step.VisualizationData.Connections.Select(c => new
                        {
                            c.FromId,
                            c.ToId,
                            c.Type,
                            c.Weight,
                            c.IsHighlighted
                        }).ToList(),
                        step.Metadata
                    }).ToList()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(visualizationData, options);
                var filePath = Path.Combine(_resultsDirectory, fileName);
                File.WriteAllText(filePath, json);

                Console.WriteLine($"✅ Данные для визуализации сохранены в: {fileName}");
                Console.WriteLine($"📊 Шагов: {result.Steps.Count}");

                // Сохраняем также HTML файл для простой визуализации
                SaveVisualizationHtml(result, fileName.Replace(".json", ".html"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения данных визуализации: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void SaveVisualizationHtml(AlgorithmResult result, string htmlFileName)
        {
            var filePath = Path.Combine(_resultsDirectory, htmlFileName);

            var htmlContent = $@"
            <!DOCTYPE html>
            <html lang='ru'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Визуализация {result.AlgorithmName}</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    .step {{ margin-bottom: 20px; padding: 10px; border: 1px solid #ccc; border-radius: 5px; }}
                    .array {{ display: flex; gap: 5px; margin: 10px 0; }}
                    .element {{ width: 40px; height: 40px; display: flex; align-items: center; justify-content: center; 
                               border: 1px solid #333; border-radius: 3px; font-weight: bold; }}
                    .comparing {{ background-color: yellow; }}
                    .swapping {{ background-color: red; color: white; }}
                    .sorted {{ background-color: green; color: white; }}
                    .pivot {{ background-color: orange; }}
                    .stats {{ background-color: #f0f0f0; padding: 10px; border-radius: 5px; margin: 10px 0; }}
                    .metadata {{ color: #666; font-size: 0.9em; }}
                </style>
            </head>
            <body>
                <h1>Визуализация алгоритма: {result.AlgorithmName}</h1>
                <div class='stats'>
                    <strong>Статистика:</strong><br>
                    Сравнений: {result.Statistics.Comparisons}<br>
                    Обменов: {result.Statistics.Swaps}<br>
                    Шагов: {result.Statistics.Steps}<br>
                    Время: {result.ExecutionTime.TotalMilliseconds:F2} мс
                </div>
                <div id='steps'>
                    <p>Данные для визуализации сохранены в JSON файле.</p>
                    <p>Для полной визуализации используйте React-приложение с загрузкой этого файла.</p>
                </div>
                <script>
                    // Здесь может быть код для загрузки и отображения шагов из JSON файла
                    console.log('Загрузите JSON файл для визуализации шагов алгоритма');
                </script>
            </body>
            </html>";

            File.WriteAllText(filePath, htmlContent);
            Console.WriteLine($"📄 HTML шаблон для визуализации сохранен в: {htmlFileName}");
        }

        // Обновим метод SaveLastResult для включения шагов
        private void SaveLastResult()
        {
            if (!_testData.ContainsKey("last_result"))
            {
                Console.WriteLine("❌ Нет результатов для сохранения. Сначала выполните тестирование алгоритма.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = _testData["last_result"] as AlgorithmResult;
            if (result == null) return;

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{result.AlgorithmName}_{timestamp}";

            // Сохраняем в разных форматах
            SaveResultAsJson(result, fileName);
            SaveResultAsTxt(result, fileName);
            SaveStepsAsJson(result, false); // Сохраняем ключевые шаги

            Console.WriteLine($"✅ Результаты сохранены в файлы:");
            Console.WriteLine($"   📄 {fileName}.json");
            Console.WriteLine($"   📄 {fileName}.txt");
            Console.WriteLine($"   📊 {result.AlgorithmName}_KeySteps_{timestamp}.json");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // Обновим метод SaveResultAsJson для включения шагов
        private void SaveResultAsJson(AlgorithmResult result, string baseFileName)
        {
            var filePath = Path.Combine(_resultsDirectory, baseFileName + ".json");
            var serializedResult = SerializeAlgorithmResult(result, true); // Включаем шаги

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(serializedResult, options);
            File.WriteAllText(filePath, json);
        }
        private void SaveResultAsTxt(AlgorithmResult result, string baseFileName)
        {
            var filePath = Path.Combine(_resultsDirectory, baseFileName + ".txt");

            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine($"=== РЕЗУЛЬТАТ АЛГОРИТМА: {result.AlgorithmName} ===");
            writer.WriteLine($"Дата выполнения: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            writer.WriteLine($"Время выполнения: {result.ExecutionTime.TotalMilliseconds:F4} мс");
            writer.WriteLine();
            writer.WriteLine("СТАТИСТИКА:");
            writer.WriteLine($"  Сравнений: {result.Statistics.Comparisons}");
            writer.WriteLine($"  Обменов: {result.Statistics.Swaps}");
            writer.WriteLine($"  Шагов: {result.Statistics.Steps}");
            if (result.Statistics.RecursiveCalls > 0)
                writer.WriteLine($"  Рекурсивных вызовов: {result.Statistics.RecursiveCalls}");
            writer.WriteLine($"  Эффективность: {CalculateEfficiency(result):F2} сравнений/обмен");
            writer.WriteLine();

            if (result.OutputData.ContainsKey("sorted_array") && result.OutputData["sorted_array"] is int[] sortedArray)
            {
                writer.WriteLine("РЕЗУЛЬТАТ СОРТИРОВКИ:");
                writer.WriteLine($"  Отсортированный массив: [{string.Join(", ", sortedArray)}]");
            }
        }

        // Обновим метод SerializeAlgorithmResult для поддержки шагов
        private object SerializeAlgorithmResult(AlgorithmResult result, bool includeSteps = false)
        {
            var serialized = new
            {
                result.AlgorithmName,
                result.SessionId,
                result.StructureType,
                result.ExecutionTime,
                Statistics = new
                {
                    result.Statistics.Comparisons,
                    result.Statistics.Swaps,
                    result.Statistics.Steps,
                    result.Statistics.RecursiveCalls,
                    result.Statistics.MemoryOperations,
                    Efficiency = CalculateEfficiency(result)
                },
                result.OutputData,
                StepsCount = result.Steps.Count,
                Steps = includeSteps ? result.Steps.Select(step => new
                {
                    step.StepNumber,
                    step.Operation,
                    step.Description,
                    Array = GetArrayFromVisualizationData(step.VisualizationData),
                    Highlights = step.VisualizationData.Highlights,
                    Connections = step.VisualizationData.Connections,
                    step.Metadata
                }).ToList() : null
            };
            return serialized;
        }

        // Вспомогательные методы
        private int[] GetArrayFromVisualizationData(VisualizationData data)
        {
            if (!data.Elements.Any()) return Array.Empty<int>();

            // Предполагаем, что элементы массива имеют ключи "0", "1", "2", ...
            var array = new List<int>();
            for (int i = 0; data.Elements.ContainsKey(i.ToString()); i++)
            {
                if (data.Elements[i.ToString()] is JsonElement element && element.TryGetProperty("value", out var valueElement))
                {
                    if (valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetInt32(out int value))
                    {
                        array.Add(value);
                    }
                }
            }
            return array.ToArray();
        }

        private bool IsKeyStep(VisualizationStep step)
        {
            // Определяем ключевые шаги для сохранения
            var keyOperations = new[] { "swap", "compare", "select_pivot", "partition_complete", "recursive_call", "complete" };
            return keyOperations.Contains(step.Operation) ||
                   step.VisualizationData.Highlights.Any(h =>
                       h.HighlightType == "swapping" || h.HighlightType == "comparing");
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }

        

    }


}