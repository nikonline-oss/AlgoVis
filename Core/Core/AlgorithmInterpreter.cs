using AlgoVis.Core.Core.Interfaces;
using AlgoVis.Evaluator.Evaluator.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures.initializers;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Functions;
using AlgoVis.Models.Models.Functions.Interfaces;
using AlgoVis.Models.Models.Operations;
using AlgoVis.Models.Models.Operations.Interfaces;
using AlgoVis.Models.Models.Steps;
using AlgoVis.Models.Models.Steps.Interfaces;
using AlgoVis.Models.Models.Suport;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;


namespace AlgoVis.Core.Core
{
    public class AlgorithmInterpreter : ICustomAlgorithmInterpreter
    {
        private readonly IOperationExecutor _operationExecutor;
        private readonly IExpressionEvaluator _expressionEvaluator;
        private readonly IVariableManager _variableManager;
        private readonly IFunctionManager _functionManager;
        private readonly IStepExecutor _stepExecutor;

        public AlgorithmInterpreter(
            IOperationExecutor operationExecutor = null,
            IExpressionEvaluator expressionEvaluator = null,
            IVariableManager variableManager = null,
            IFunctionManager functionManager = null,
            IStepExecutor stepExecutor = null)
        {
            _operationExecutor = operationExecutor ?? new OperationExecutor();
            _expressionEvaluator = expressionEvaluator ?? new ExpressionEvaluator();
            _variableManager = variableManager ?? new VariableManager();
            _functionManager = functionManager ?? new FunctionManager();
            _stepExecutor = stepExecutor ?? new StepExecutor();
        }

        private List<IStructureInitializer> GetStructureInitializers()
        {
            return new List<IStructureInitializer>
            {
                new ArrayStructureInitializer(),
                new BinaryTreeStructureInitializer()
            };
        }

        public CustomAlgorithmResult Execute(CustomAlgorithmRequest request, IDataStructure structure)
        {
            // Валидация входных параметров
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Запрос не может быть null");
            }

            if (structure == null)
            {
                throw new ArgumentNullException(nameof(structure), "Структура данных не может быть null");
            }

            var stopwatch = Stopwatch.StartNew();
            ExecutionContext context = null;

            try
            {
                context = CreateExecutionContext(request, structure);
                InitializeExecution(context);
                ExecuteAlgorithm(context);

                stopwatch.Stop();
                return CreateSuccessResult(context, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // Если контекст не был создан, создаем минимальный для возврата ошибки
                if (context == null)
                {
                    try
                    {
                        context = CreateExecutionContext(request, structure);
                    }
                    catch
                    {
                        // Если даже создание контекста не удалось, возвращаем минимальный результат
                        return new CustomAlgorithmResult
                        {
                            success = false,
                            message = $"Критическая ошибка: {ex.Message}",
                            result = new AlgorithmResult
                            {
                                AlgorithmName = request?.name ?? "Unknown",
                                ExecutionTime = stopwatch.Elapsed
                            }
                        };
                    }
                }
                return CreateErrorResult(ex, stopwatch.Elapsed, context);
            }
        }

        private ExecutionContext CreateExecutionContext(CustomAlgorithmRequest request, IDataStructure structure)
        {
            return new ExecutionContext
            {
                Request = request,
                Structure = structure,
                Statistics = new AlgorithmStatistics(),
                VisualizationSteps = new List<VisualizationStep>(),
                Variables = _variableManager.CreateScope(),
                FunctionStack = new FunctionStack(),
                StepHistory = new StepExecutionHistory(),
                ExpressionEvaluator = _expressionEvaluator,
                OperationExecutor = _operationExecutor
            };
        }

        private void InitializeExecution(ExecutionContext context)
        {
            // Проверка на null
            if (context.Request.variables == null)
            {
                context.Request.variables = new List<VariableDefinition>();
            }

            foreach (var variableDef in context.Request.variables)
            {
                // Валидация имени переменной
                if (string.IsNullOrWhiteSpace(variableDef.name))
                {
                    throw new ArgumentException("Имя переменной не может быть пустым");
                }

                var variableType = VariableTypeHelper.ParseType(variableDef.type);
                VariableValue variableValue;

                if (variableType == VariableType.Array)
                {
                    // Инициализация динамического массива
                    variableValue = InitializeDynamicArray(variableDef, context);
                }
                else if (variableType == VariableType.Object)
                {
                    // Инициализация объекта
                    variableValue = InitializeObject(variableDef, context);
                }
                else
                {
                    // Обычные переменные
                    var value = ParseVariableValue(variableType, variableDef.initialValue?.ToString(), context);
                    variableValue = new VariableValue(variableType, value);
                }

                // Создаем VariableValue и устанавливаем переменную
                context.Variables.Set(variableDef.name, variableValue);
            }


            // Инициализация стандартных переменных алгоритма
            InitializeStandardVariables(context);
        }
        private VariableValue InitializeDynamicArray(VariableDefinition variableDef, ExecutionContext context)
        {
            var array = new List<VariableValue>();

            // Поддержка инициализации массива из initialValue
            if (variableDef.initialValue is string initStr && !string.IsNullOrWhiteSpace(initStr))
            {
                try
                {
                    // Попробуем распарсить как JSON массив: [1, 2, 3]
                    if (initStr.Trim().StartsWith("[") && initStr.Trim().EndsWith("]"))
                    {
                        // Упрощенный парсинг для демонстрации
                        var elements = initStr.Trim().Trim('[', ']').Split(',');
                        foreach (var element in elements)
                        {
                            if (!string.IsNullOrWhiteSpace(element))
                            {
                                var elementValue = ParseVariableValue(VariableType.Int, element.Trim(), context);
                                array.Add(new VariableValue(elementValue));
                            }
                        }
                    }
                    else
                    {
                        // Инициализация массива одним значением
                        var elementValue = ParseVariableValue(VariableType.Int, initStr, context);
                        // Создаем массив с одним элементом
                        array.Add(new VariableValue(elementValue));
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но создаем пустой массив для продолжения работы
                    Console.WriteLine($"⚠️ Ошибка инициализации массива '{variableDef.name}': {ex.Message}");
                    // Создаем пустой массив в случае ошибки
                    // В production лучше бы выбросить исключение или вернуть ошибку
                }
            }


            return new VariableValue(array);
        }

        private VariableValue InitializeObject(VariableDefinition variableDef, ExecutionContext context)
        {
            // Всегда используем System.Text.Json для надежности
            return InitializeObjectWithSystemJson(variableDef, context);
        }

        private VariableValue InitializeObjectWithSystemJson(VariableDefinition variableDef, ExecutionContext context)
        {
            if (variableDef.initialValue?.ToString() is string initStr && !string.IsNullOrWhiteSpace(initStr))
            {
                try
                {
                    initStr = initStr.Trim().Trim('"');
                    Console.WriteLine($"🔍 Инициализация объекта '{variableDef.name}': {initStr}");

                    using var jsonDocument = JsonDocument.Parse(initStr);
                    var root = jsonDocument.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        var obj = ParseJsonElementToDictionary(root);
                        Console.WriteLine($"✅ Объект '{variableDef.name}' инициализирован: {obj.Count} свойств");

                        // Логируем структуру объекта
                        LogObjectStructure(obj, variableDef.name, 0);

                        return new VariableValue(obj);
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"⚠️ Ошибка парсинга JSON для объекта '{variableDef.name}': {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Общая ошибка инициализации объекта '{variableDef.name}': {ex.Message}");
                }
            }

            // Fallback: создаем пустой объект
            var fallbackObj = new Dictionary<string, VariableValue> { ["value"] = new VariableValue(0) };
            Console.WriteLine($"⚠️ Создан fallback объект для '{variableDef.name}'");
            return new VariableValue(fallbackObj);
        }

        private void LogObjectStructure(Dictionary<string, VariableValue> obj, string name, int depth)
        {
            string indent = new string(' ', depth * 2);
            foreach (var prop in obj)
            {
                Console.WriteLine($"{indent}📁 {name}.{prop.Key}: {prop.Value.Value} (тип: {prop.Value.Type})");

                if (prop.Value.Type == VariableType.Object && prop.Value.Value is Dictionary<string, VariableValue> nestedObj)
                {
                    LogObjectStructure(nestedObj, $"{name}.{prop.Key}", depth + 1);
                }
            }
        }


        private Dictionary<string, VariableValue> ParseJsonElementToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, VariableValue>();

            foreach (var property in element.EnumerateObject())
            {
                var value = ParseJsonElementValue(property.Value);
                dict[property.Name] = value;
                Console.WriteLine($"🔍 JSON свойство: {property.Name} = {value.Value} (тип значения: {value.Value?.GetType()})");
            }

            return dict;
        }

        private VariableValue ParseJsonElementValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    if (!element.EnumerateObject().Any())
                    {
                        return new VariableValue((object)null);
                    }
                    var nestedDict = ParseJsonElementToDictionary(element);
                    return new VariableValue(nestedDict);

                case JsonValueKind.Array:
                    var array = new List<VariableValue>();
                    foreach (var item in element.EnumerateArray())
                    {
                        array.Add(ParseJsonElementValue(item));
                    }
                    return new VariableValue(array);

                case JsonValueKind.String:
                    var stringValue = element.GetString();
                    if (stringValue == null) return new VariableValue((object)null);

                    if (int.TryParse(stringValue, out int intValue))
                        return new VariableValue(intValue);
                    else if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
                        return new VariableValue(doubleValue);
                    else if (bool.TryParse(stringValue, out bool boolValue))
                        return new VariableValue(boolValue);
                    else
                        return new VariableValue(stringValue);

                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intVal))
                        return new VariableValue(intVal);
                    else if (element.TryGetDouble(out double doubleVal))
                        return new VariableValue(doubleVal);
                    else
                        return new VariableValue(0);

                case JsonValueKind.True:
                    return new VariableValue(true);

                case JsonValueKind.False:
                    return new VariableValue(false);

                case JsonValueKind.Null:
                    return new VariableValue((object)null);

                default:
                    return new VariableValue(0);
            }
        }

        private object ParseVariableValue(VariableType type, string value, ExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(value))
                return VariableTypeHelper.CreateDefaultValue(type);

            try
            {
                // Для объектов используем специальную логику
                if (type == VariableType.Object)
                {
                    // Если строка выглядит как выражение, вычисляем его
                    if (value.Contains(".") || value.Contains("[") || ContainsLetters(value))
                    {
                        var result1 = context.ExpressionEvaluator.Evaluate(value, context.Variables);
                        return ExtractValue(result1);
                    }
                    // Иначе парсим как примитивное значение
                    else
                    {
                        return ParsePrimitiveValue(value);
                    }
                }

                // Для остальных типов используем стандартную логику
                var result = context.ExpressionEvaluator.Evaluate(value, context.Variables);
                return ExtractValue(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка парсинга переменной: тип={type}, значение={value}, ошибка={ex.Message}");
                return VariableTypeHelper.CreateDefaultValue(type);
            }
        }

        private object ParsePrimitiveValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            // Пробуем разные типы по порядку
            if (int.TryParse(value, out int intValue))
                return intValue;
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;
            if (bool.TryParse(value, out bool boolValue))
                return boolValue;

            return value; // строка по умолчанию
        }

        private bool ContainsLetters(string str)
        {
            return !string.IsNullOrEmpty(str) && str.Any(char.IsLetter);
        }


        private void InitializeStandardVariables(ExecutionContext context)
        {
            // Стандартные переменные для алгоритмов
            var standardVars = new Dictionary<string, object>
            {
                ["i"] = 0,
                ["j"] = 0,
                ["k"] = 0,
                ["n"] = 0,
                ["temp"] = 0,
                ["swapped"] = false,
                ["last_comparison"] = 0,
                ["result"] = 0
            };

            foreach (var stdVar in standardVars)
            {
                if (!context.Variables.Contains(stdVar.Key))
                {
                    context.Variables.Set(stdVar.Key, new VariableValue(stdVar.Value));
                }
            }

            InitializeStructureVariable(context);
        }

        private void InitializeStructureVariable(ExecutionContext context)
        {
            var structure = context.Structure;
            var structProperties = new Dictionary<string, object>();

            Console.WriteLine($"🔍 Инициализация переменной 'struct' для типа: {structure.Type}");

            // Базовые свойства для всех структур
            structProperties["type"] = structure.Type;
            structProperties["id"] = structure.Id;

            // Специфические свойства для разных типов структур
            switch (structure.Type.ToLower())
            {
                case "array":
                    int[] arrayState = structure.GetState() as int[] ?? Array.Empty<int>();
                    structProperties["len"] = arrayState.Length;
                    structProperties["first"] = arrayState.Length > 0 ? arrayState[0] : 0;
                    structProperties["last"] = arrayState.Length > 0 ? arrayState[^1] : 0;
                    structProperties["isEmpty"] = arrayState.Length == 0;
                    structProperties["values"] = arrayState;
                    break;

                case "binarytree":
                    var treeState = structure.GetState() as TreeNode;
                    structProperties["value"] = treeState?.Value ?? 0;
                    structProperties["hasLeft"] = treeState?.Left != null;
                    structProperties["hasRight"] = treeState?.Right != null;
                    structProperties["isLeaf"] = treeState?.Left == null && treeState?.Right == null;
                    structProperties["height"] = CalculateTreeHeight(treeState);
                    structProperties["nodeCount"] = CountTreeNodes(treeState);
                    break;

                case "linkedlist":
                    var listState = structure.GetState() as ListNode;
                    structProperties["headValue"] = listState?.Value ?? 0;
                    structProperties["hasNext"] = listState?.Next != null;
                    structProperties["length"] = CalculateListLength(listState);
                    break;

                case "graph":
                    var graphState = structure.GetState() as GraphState;
                    structProperties["nodeCount"] = graphState?.Nodes.Count ?? 0;
                    structProperties["edgeCount"] = graphState?.Edges.Count ?? 0;
                    structProperties["isDirected"] = false;
                    break;

                default:
                    structProperties["description"] = $"Структура типа {structure.Type}";
                    break;
            }

            // Создаем объект struct со всеми свойствами
            var structObject = new Dictionary<string, VariableValue>();
            foreach (var prop in structProperties)
            {
                structObject[prop.Key] = new VariableValue(prop.Value);
            }

            context.Variables.Set("struct", new VariableValue(structObject));

            Console.WriteLine($"✅ Переменная 'struct' инициализирована с {structProperties.Count} свойствами");
            foreach (var prop in structProperties)
            {
                Console.WriteLine($"   - {prop.Key}: {prop.Value}");
            }
        }

        protected object ExtractValue(object value)
        {
            if (value is VariableValue variableValue)
            {
                // Извлекаем значение и рекурсивно применяем ExtractValue
                var extracted = variableValue.Value;
                return ExtractValue(extracted); // Рекурсивно извлекаем
            }

            // Если это примитивный тип, возвращаем как есть
            return value;
        }

        private object ExtractVariableValue(object value)
        {
            if (value is VariableValue variableValue)
            {
                // Используем безопасное свойство для сериализации
                return variableValue.SerializableValue;
            }

            return value;
        }
        private void ExecuteAlgorithm(ExecutionContext context)
        {
            // Валидация запроса
            if (context.Request == null)
            {
                throw new ArgumentNullException(nameof(context.Request), "Запрос не может быть null");
            }

            if (context.Request.steps == null || !context.Request.steps.Any())
            {
                throw new InvalidOperationException("Запрос должен содержать хотя бы один шаг");
            }

            // Определяем точку входа
            var entryPoint = "start";
            
            // Проверяем существование шага "start"
            var startStep = context.Request.steps.FirstOrDefault(s => s.id == entryPoint);
            if (startStep == null && context.Request.functions != null)
            {
                // Ищем в функциях
                foreach (var function in context.Request.functions)
                {
                    if (function.entryPoint == entryPoint || function.steps.Any(s => s.id == entryPoint))
                    {
                        startStep = function.steps.FirstOrDefault(s => s.id == entryPoint);
                        if (startStep != null) break;
                    }
                }
            }

            if (startStep == null)
            {
                // Пытаемся использовать первый шаг как точку входа
                startStep = context.Request.steps.FirstOrDefault();
                if (startStep != null)
                {
                    entryPoint = startStep.id;
                }
                else
                {
                    throw new InvalidOperationException("Не найдена точка входа для выполнения алгоритма");
                }
            }

            _stepExecutor.Execute(entryPoint, context);
        }

        private int CalculateTreeHeight(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(CalculateTreeHeight(node.Left), CalculateTreeHeight(node.Right));
        }

        private int CountTreeNodes(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + CountTreeNodes(node.Left) + CountTreeNodes(node.Right);
        }

        private int CalculateListLength(ListNode head)
        {
            int count = 0;
            var current = head;
            while (current != null)
            {
                count++;
                current = current.Next;
            }
            return count;
        }

        private int GetStructureLength(IDataStructure structure)
        {
            try
            {
                Console.WriteLine($"🔍 Получение длины структуры типа: {structure.Type}");

                return structure.Type.ToLower() switch
                {
                    "array" => GetArrayLength(structure),
                    "binarytree" => GetTreeSize(structure),
                    "linkedlist" => GetListLength(structure),
                    "graph" => GetGraphNodeCount(structure),
                    _ => 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка получения длины структуры: {ex.Message}");
                return 0;
            }
        }

        private int GetArrayLength(IDataStructure structure)
        {
            var state = structure.GetState();
            if (state is int[] array) return array.Length;
            if (state is Array genericArray) return genericArray.Length;

            // Рефлексия для других типов массивов
            var lengthProperty = state.GetType().GetProperty("Length");
            if (lengthProperty != null) return (int)lengthProperty.GetValue(state);

            var countProperty = state.GetType().GetProperty("Count");
            if (countProperty != null) return (int)countProperty.GetValue(state);

            throw new InvalidOperationException("Не удается определить длину массива");
        }

        private int GetTreeSize(IDataStructure structure)
        {
            var treeState = structure.GetState() as TreeNode;
            return CountTreeNodes(treeState);
        }

        private int GetListLength(IDataStructure structure)
        {
            var listState = structure.GetState() as ListNode;
            return CalculateListLength(listState);
        }

        private int GetGraphNodeCount(IDataStructure structure)
        {
            var graphState = structure.GetState() as GraphState;
            return graphState?.Nodes.Count ?? 0;
        }

        private CustomAlgorithmResult CreateSuccessResult(ExecutionContext context, TimeSpan executionTime)
        {
            return new CustomAlgorithmResult
            {
                success = true,
                message = "Алгоритм выполнен успешно",
                result = new AlgorithmResult
                {
                    AlgorithmName = context.Request.name,
                    SessionId = Guid.NewGuid().ToString(),
                    StructureType = context.Structure.Type,
                    Steps = context.VisualizationSteps,
                    Statistics = context.Statistics.Clone(),
                    ExecutionTime = executionTime,
                    OutputData = CreateOutputData(context)
                },
                executionState = context.Variables.GetAllVariables()
            };
        }

        private Dictionary<string, object> CreateOutputData(ExecutionContext context)
        {
            var variables = context.Variables.GetAllVariables();

            // Преобразуем VariableValue в простые значения для вывода
            var simpleVariables = new Dictionary<string, object>();
            foreach (var variable in variables)
            {
                simpleVariables[variable.Key] = ExtractVariableValue(variable.Value);
            }

            return new Dictionary<string, object>
            {
                ["start_structure"] = context.Structure.GetOriginState(),
                ["final_structure"] = context.Structure.GetState(),
                ["variables"] = simpleVariables,
                ["call_depth"] = context.FunctionStack.CurrentDepth,
                ["function_calls"] = context.Statistics.RecursiveCalls,
                ["total_steps"] = context.Statistics.Steps
            };
        }

        private CustomAlgorithmResult CreateErrorResult(Exception ex, TimeSpan executionTime, ExecutionContext context)
        {
            return new CustomAlgorithmResult
            {
                success = false,
                message = $"Ошибка выполнения: {ex.Message}",
                result = new AlgorithmResult
                {
                    AlgorithmName = context.Request.name,
                    ExecutionTime = executionTime,
                    Steps = context.VisualizationSteps,
                    Statistics = context.Statistics
                }
            };
        }

        private object EvaluateExpression(string expression, ExecutionContext context)
        {
            return _expressionEvaluator.Evaluate(expression, context.Variables);
        }

        private bool EvaluateCondition(string condition, ExecutionContext context)
        {
            return _expressionEvaluator.EvaluateCondition(condition, context.Variables);
        }

    }
}
