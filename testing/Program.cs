using MyMenuSystem;
using System.Text;
using System.Text.Json;
using testing.Models.Core;
using testing.Models.Custom;
using testing.Models.DataStructures;
using testing.Models.Evaluator;
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

            // Упрощенные пункты меню
            _menuManager.AddMenuItem("📊 Настроить тестовые данные", SetupTestData);
            _menuManager.AddMenuItem("🔄 Тестировать BubbleSort", TestBubbleSort);
            _menuManager.AddMenuItem("⚡ Тестировать QuickSort", TestQuickSort);
            _menuManager.AddMenuItem("🎯 Загрузить кастомный алгоритм", LoadCustomAlgorithm);
            _menuManager.AddMenuItem("💾 Сохранить результаты", SaveResultsMenu);
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

            setupMenu.AddMenuItem("🎲 Сгенерировать случайный массив", GenerateRandomArray);
            setupMenu.AddMenuItem("⌨️ Ввести массив вручную", InputArrayManually);
            setupMenu.AddMenuItem("📋 Использовать тестовые примеры", UseTestCases);
            setupMenu.AddMenuItem("👁️ Показать текущие данные", ShowCurrentData);

            setupMenu.Run();
        }

        private void LoadCustomAlgorithm()
        {
            Console.WriteLine("\n=== Загрузка кастомного алгоритма ===");

            // Предопределенные пути для тестирования
            var filePath = "C:\\2025\\Project\\Программная инженерия\\AlgoVis\\testing\\quickSort.json";
            //var filePath = "C:\\2025\\Project\\Программная инженерия\\AlgoVis\\testing\\InstructJson.json";

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
                    ["Detailed"] = false,
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

            var config = new AlgorithmConfig
            {
                Name = "QuickSort",
                SessionId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, object>
                {
                    ["Detailed"] = true,
                    ["PivotStrategy"] = "last"
                }
            };

            RunAlgorithm("QuickSort", config, arrayStructure, array);
        }

        private void RunAlgorithm(string algorithmName, AlgorithmConfig config, IDataStructure structure, int[] originalArray)
        {
            try
            {
                Console.WriteLine($"\n=== Запуск {algorithmName} ===");
                Console.WriteLine($"Массив: [{string.Join(", ", originalArray)}]");
                Console.WriteLine("Выполнение...\n");

                var result = _algorithmManager.ExecuteAlgorithm(config, structure);

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

            if (result.OutputData.ContainsKey("final_structure") && result.OutputData["final_structure"] is int[] sortedArray)
            {
                Console.WriteLine("📋 РЕЗУЛЬТАТ:");
                Console.WriteLine($"   Исходный: [{string.Join(", ", originalArray)}]");
                Console.WriteLine($"   Отсортированный: [{string.Join(", ", sortedArray)}]");
                Console.WriteLine($"   Корректность: {(IsSorted(sortedArray) ? "✓ Успешно" : "✗ Ошибка")}");
            }

            Console.WriteLine($"   Шагов визуализации: {result.Steps.Count}");
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

            saveMenu.AddMenuItem("💾 Сохранить последний результат", SaveLastResult);
            saveMenu.AddMenuItem("📊 Сохранить статистику", SaveStatistics);
            saveMenu.AddMenuItem("🔄 Сохранить шаги алгоритма", SaveAlgorithmSteps);
            saveMenu.AddMenuItem("🌐 Экспорт в HTML", ExportToHtml);
            saveMenu.AddMenuItem("📋 Показать историю сохранений", ShowSaveHistory);

            saveMenu.Run();
        }

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
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{result.AlgorithmName}_{timestamp}";

            SaveResultAsJson(result, fileName);
            SaveResultAsTxt(result, fileName);

            Console.WriteLine($"✅ Результаты сохранены:");
            Console.WriteLine($"   📄 {fileName}.json");
            Console.WriteLine($"   📄 {fileName}.txt");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void SaveStatistics()
        {
            if (!_testData.ContainsKey("last_result"))
            {
                Console.WriteLine("❌ Нет результатов для сохранения.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = _testData["last_result"] as AlgorithmResult;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"Statistics_{timestamp}.txt";

            try
            {
                using var writer = new StreamWriter(Path.Combine(_resultsDirectory, fileName), false, Encoding.UTF8);

                writer.WriteLine("=== СТАТИСТИКА АЛГОРИТМА ===");
                writer.WriteLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                writer.WriteLine($"Алгоритм: {result.AlgorithmName}");
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
                writer.WriteLine($"Шагов визуализации: {result.Steps.Count}");

                Console.WriteLine($"✅ Статистика сохранена в: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void SaveAlgorithmSteps()
        {
            if (!_testData.ContainsKey("last_result"))
            {
                Console.WriteLine("❌ Нет результатов для сохранения.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = _testData["last_result"] as AlgorithmResult;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{result.AlgorithmName}_Steps_{timestamp}.json";

            try
            {
                var stepsData = new
                {
                    AlgorithmName = result.AlgorithmName,
                    Timestamp = DateTime.Now,
                    TotalSteps = result.Steps.Count,
                    Steps = result.Steps.Select(step => new
                    {
                        step.stepNumber,
                        step.operation,
                        step.description,
                        ArrayState = GetArrayFromVisualizationData(step.visualizationData),
                        Highlights = step.visualizationData.highlights.Select(h => new
                        {
                            h.ElementId,
                            h.HighlightType,
                            h.Color,
                            h.Label
                        }).ToList(),
                        step.metadata
                    }).ToList()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(stepsData, options);
                File.WriteAllText(Path.Combine(_resultsDirectory, fileName), json);

                Console.WriteLine($"✅ Шаги алгоритма сохранены в: {fileName}");
                Console.WriteLine($"📊 Сохранено шагов: {result.Steps.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения шагов: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ExportToHtml()
        {
            if (!_testData.ContainsKey("last_result"))
            {
                Console.WriteLine("❌ Нет результатов для экспорта.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = _testData["last_result"] as AlgorithmResult;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{result.AlgorithmName}_Visualization_{timestamp}.html";

            try
            {
                var htmlContent = GenerateInteractiveHtml(result);
                File.WriteAllText(Path.Combine(_resultsDirectory, fileName), htmlContent);

                Console.WriteLine($"✅ Интерактивная визуализация сохранена в: {fileName}");
                Console.WriteLine($"📂 Файл: {Path.Combine(_resultsDirectory, fileName)}");
                Console.WriteLine("🌐 Откройте файл в браузере для просмотра анимации");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка экспорта в HTML: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private string GenerateInteractiveHtml(AlgorithmResult result)
        {
            var stepsJson = JsonSerializer.Serialize(result.Steps.Select(step => new
            {
                step.stepNumber,
                step.operation,
                step.description,
                Array = GetArrayFromVisualizationData(step.visualizationData),
                Highlights = step.visualizationData.highlights,
                Metadata = step.metadata
            }).ToList(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return $@"
<!DOCTYPE html>
<html lang='ru'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Визуализация {result.AlgorithmName}</title>
    <style>
        body {{ 
            font-family: 'Segoe UI', Arial, sans-serif; 
            margin: 0; 
            padding: 20px; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
        }}
        .container {{ 
            max-width: 1200px; 
            margin: 0 auto; 
            background: white; 
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.3);
            overflow: hidden;
        }}
        .header {{ 
            background: linear-gradient(135deg, #2c3e50, #34495e);
            color: white; 
            padding: 30px; 
            text-align: center;
        }}
        .header h1 {{ 
            margin: 0; 
            font-size: 2.5em;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
        }}
        .stats {{ 
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            padding: 25px;
            background: #f8f9fa;
            border-bottom: 1px solid #dee2e6;
        }}
        .stat-card {{
            background: white;
            padding: 20px;
            border-radius: 10px;
            text-align: center;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            border-left: 4px solid #667eea;
        }}
        .stat-value {{
            font-size: 2em;
            font-weight: bold;
            color: #2c3e50;
        }}
        .stat-label {{
            color: #7f8c8d;
            font-size: 0.9em;
            margin-top: 5px;
        }}
        .visualization {{ 
            padding: 30px; 
            min-height: 400px;
        }}
        .array-container {{
            display: flex;
            justify-content: center;
            align-items: end;
            gap: 8px;
            margin: 30px 0;
            padding: 20px;
            background: #f8f9fa;
            border-radius: 10px;
            min-height: 200px;
        }}
        .array-element {{
            width: 50px;
            background: #3498db;
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: bold;
            border-radius: 5px 5px 0 0;
            transition: all 0.3s ease;
            position: relative;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
        }}
        .array-element.comparing {{
            background: #f39c12;
            transform: scale(1.1);
            box-shadow: 0 4px 15px rgba(243, 156, 18, 0.4);
        }}
        .array-element.swapping {{
            background: #e74c3c;
            transform: scale(1.1);
            box-shadow: 0 4px 15px rgba(231, 76, 60, 0.4);
        }}
        .array-element.sorted {{
            background: #27ae60;
        }}
        .array-element.pivot {{
            background: #9b59b6;
        }}
        .element-value {{
            position: absolute;
            top: -25px;
            font-size: 0.9em;
            color: #2c3e50;
        }}
        .element-index {{
            position: absolute;
            bottom: -20px;
            font-size: 0.8em;
            color: #7f8c8d;
        }}
        .controls {{
            display: flex;
            justify-content: center;
            gap: 15px;
            padding: 20px;
            background: #34495e;
        }}
        .control-btn {{
            padding: 12px 25px;
            border: none;
            border-radius: 25px;
            background: linear-gradient(135deg, #667eea, #764ba2);
            color: white;
            font-weight: bold;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
        }}
        .control-btn:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0,0,0,0.3);
        }}
        .control-btn:disabled {{
            background: #bdc3c7;
            cursor: not-allowed;
            transform: none;
            box-shadow: none;
        }}
        .step-info {{
            text-align: center;
            padding: 20px;
            background: #2c3e50;
            color: white;
            font-size: 1.2em;
        }}
        .progress-container {{
            width: 100%;
            background: #ecf0f1;
            border-radius: 10px;
            margin: 20px 0;
            overflow: hidden;
        }}
        .progress-bar {{
            height: 10px;
            background: linear-gradient(135deg, #667eea, #764ba2);
            width: 0%;
            transition: width 0.3s ease;
        }}
        .highlight-legend {{
            display: flex;
            justify-content: center;
            gap: 20px;
            margin: 20px 0;
            flex-wrap: wrap;
        }}
        .legend-item {{
            display: flex;
            align-items: center;
            gap: 8px;
        }}
        .legend-color {{
            width: 20px;
            height: 20px;
            border-radius: 4px;
        }}
        .comparing-legend {{ background: #f39c12; }}
        .swapping-legend {{ background: #e74c3c; }}
        .sorted-legend {{ background: #27ae60; }}
        .pivot-legend {{ background: #9b59b6; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎯 {result.AlgorithmName}</h1>
            <p>Интерактивная визуализация алгоритма сортировки</p>
        </div>
        
        <div class='stats'>
            <div class='stat-card'>
                <div class='stat-value'>{result.Statistics.Comparisons}</div>
                <div class='stat-label'>Сравнений</div>
            </div>
            <div class='stat-card'>
                <div class='stat-value'>{result.Statistics.Swaps}</div>
                <div class='stat-label'>Обменов</div>
            </div>
            <div class='stat-card'>
                <div class='stat-value'>{result.Statistics.Steps}</div>
                <div class='stat-label'>Шагов</div>
            </div>
            <div class='stat-card'>
                <div class='stat-value'>{result.ExecutionTime.TotalMilliseconds:F2}мс</div>
                <div class='stat-label'>Время выполнения</div>
            </div>
        </div>

        <div class='highlight-legend'>
            <div class='legend-item'>
                <div class='legend-color comparing-legend'></div>
                <span>Сравнение</span>
            </div>
            <div class='legend-item'>
                <div class='legend-color swapping-legend'></div>
                <span>Обмен</span>
            </div>
            <div class='legend-item'>
                <div class='legend-color sorted-legend'></div>
                <span>Отсортирован</span>
            </div>
            <div class='legend-item'>
                <div class='legend-color pivot-legend'></div>
                <span>Опорный элемент</span>
            </div>
        </div>

        <div class='step-info'>
            <div id='stepDescription'>Готов к запуску...</div>
            <div class='progress-container'>
                <div class='progress-bar' id='progressBar'></div>
            </div>
            <div>Шаг <span id='currentStep'>0</span> из <span id='totalSteps'>{result.Steps.Count}</span></div>
        </div>

        <div class='visualization'>
            <div class='array-container' id='arrayContainer'>
                <!-- Массив будет отрисован здесь -->
            </div>
        </div>

        <div class='controls'>
            <button class='control-btn' onclick='previousStep()' id='prevBtn'>⏮️ Предыдущий</button>
            <button class='control-btn' onclick='playPause()' id='playBtn'>▶️ Воспроизвести</button>
            <button class='control-btn' onclick='nextStep()' id='nextBtn'>Следующий ⏭️</button>
            <button class='control-btn' onclick='resetAnimation()'>🔄 Сбросить</button>
            <button class='control-btn' onclick='changeSpeed(0.5)'>🐢 Медленно</button>
            <button class='control-btn' onclick='changeSpeed(1)'>🚀 Нормально</button>
            <button class='control-btn' onclick='changeSpeed(2)'>⚡ Быстро</button>
        </div>
    </div>

    <script>
        const steps = {stepsJson};
        let currentStepIndex = 0;
        let isPlaying = false;
        let playInterval;
        let speed = 1000;

        function renderArray(step) {{
            const container = document.getElementById('arrayContainer');
            container.innerHTML = '';
            
            if (!step || !step.array) return;
            
            const maxValue = Math.max(...step.array);
            const containerHeight = 200;
            
            step.array.forEach((value, index) => {{
                const element = document.createElement('div');
                element.className = 'array-element';
                element.style.height = `auto`;
                
                const valueSpan = document.createElement('div');
                valueSpan.className = 'element-value';
                valueSpan.textContent = value;
                
                const indexSpan = document.createElement('div');
                indexSpan.className = 'element-index';
                indexSpan.textContent = index;
                
                element.appendChild(valueSpan);
                element.appendChild(indexSpan);
                
                // Применяем подсветку
                if (step.highlights) {{
                    step.highlights.forEach(highlight => {{
                        if (highlight.elementId === index.toString()) {{
                            element.classList.add(highlight.highlightType.toLowerCase());
                        }}
                    }});
                }}
                
                container.appendChild(element);
            }});
        }}

        function updateStepInfo() {{
            const step = steps[currentStepIndex];
            document.getElementById('stepDescription').textContent = 
                step ? `Шаг ${result.Steps[0].stepNumber}: ${result.Steps[0].description}` : 'Завершено';
            document.getElementById('currentStep').textContent = currentStepIndex + 1;
            document.getElementById('totalSteps').textContent = steps.length;
            document.getElementById('progressBar').style.width = 
                `100%`;
            
            // Обновляем состояние кнопок
            document.getElementById('prevBtn').disabled = currentStepIndex === 0;
            document.getElementById('nextBtn').disabled = currentStepIndex === steps.length - 1;
        }}

        function nextStep() {{
            if (currentStepIndex < steps.length - 1) {{
                currentStepIndex++;
                renderArray(steps[currentStepIndex]);
                updateStepInfo();
            }}
        }}

        function previousStep() {{
            if (currentStepIndex > 0) {{
                currentStepIndex--;
                renderArray(steps[currentStepIndex]);
                updateStepInfo();
            }}
        }}

        function playPause() {{
            isPlaying = !isPlaying;
            const playBtn = document.getElementById('playBtn');
            
            if (isPlaying) {{
                playBtn.textContent = '⏸️ Пауза';
                playInterval = setInterval(() => {{
                    if (currentStepIndex < steps.length - 1) {{
                        nextStep();
                    }} else {{
                        playPause();
                    }}
                }}, speed);
            }} else {{
                playBtn.textContent = '▶️ Воспроизвести';
                clearInterval(playInterval);
            }}
        }}

        function resetAnimation() {{
            isPlaying = false;
            clearInterval(playInterval);
            document.getElementById('playBtn').textContent = '▶️ Воспроизвести';
            currentStepIndex = 0;
            renderArray(steps[currentStepIndex]);
            updateStepInfo();
        }}

        function changeSpeed(newSpeed) {{
            speed = 1000 / newSpeed;
            if (isPlaying) {{
                playPause();
                playPause();
            }}
        }}

        // Инициализация
        document.addEventListener('DOMContentLoaded', () => {{
            renderArray(steps[0]);
            updateStepInfo();
        }});

        // Горячие клавиши
        document.addEventListener('keydown', (e) => {{
            switch(e.key) {{
                case 'ArrowLeft': previousStep(); break;
                case 'ArrowRight': nextStep(); break;
                case ' ': playPause(); break;
                case 'Home': resetAnimation(); break;
            }}
        }});
    </script>
</body>
</html>";
        }

        private void ShowSaveHistory()
        {
            Console.WriteLine("\n=== ИСТОРИЯ СОХРАНЕНИЙ ===");

            try
            {
                var files = Directory.GetFiles(_resultsDirectory)
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(10)
                    .ToList();

                if (files.Count == 0)
                {
                    Console.WriteLine("📁 Файлы результатов не найдены.");
                }
                else
                {
                    Console.WriteLine($"Последние {files.Count} файлов:");
                    Console.WriteLine(new string('─', 80));

                    foreach (var file in files)
                    {
                        var icon = file.Extension.ToLower() switch
                        {
                            ".html" => "🌐",
                            ".json" => "📊",
                            ".txt" => "📄",
                            _ => "📁"
                        };

                        Console.WriteLine($"{icon} {file.Name,-35} {FormatFileSize(file.Length),-8} {file.CreationTime:dd.MM.yy HH:mm}");
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

        // Вспомогательные методы
        private int[] GetArrayFromVisualizationData(VisualizationData data)
        {
            if (!data.elements.Any()) return Array.Empty<int>();

            var array = new List<int>();
            for (int i = 0; data.elements.ContainsKey(i.ToString()); i++)
            {
                if (data.elements[i.ToString()] is JsonElement element &&
                    element.TryGetProperty("value", out var valueElement) &&
                    valueElement.ValueKind == JsonValueKind.Number)
                {
                    if (valueElement.TryGetInt32(out int value))
                    {
                        array.Add(value);
                    }
                }
            }
            return array.ToArray();
        }

        private void SaveResultAsJson(AlgorithmResult result, string baseFileName)
        {
            var filePath = Path.Combine(_resultsDirectory, baseFileName + ".json");

            var resultData = new
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
                    result.Statistics.RecursiveCalls
                },
                result.OutputData,
                StepsCount = result.Steps.Count,
                result.Steps
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(resultData, options);
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
            writer.WriteLine($"Шагов визуализации: {result.Steps.Count}");
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