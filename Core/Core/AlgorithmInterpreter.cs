using AlgoVis.Core.Core.Interfaces;
using AlgoVis.Evaluator.Evaluator.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Parsing;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
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
        private readonly IParser _expressionParser;
        private readonly IVariableManager _variableManager;
        private readonly IFunctionManager _functionManager;
        private readonly IStepExecutor _stepExecutor;
        private readonly UniversalStructureConverter _structureConverter;

        public AlgorithmInterpreter(
            IOperationExecutor operationExecutor = null,
            IParser expressionParser = null,
            IVariableManager variableManager = null,
            IFunctionManager functionManager = null,
            IStepExecutor stepExecutor = null)
        {
            _operationExecutor = operationExecutor ?? new OperationExecutor();
            _expressionParser = expressionParser ?? new ExpressionParser();
            _variableManager = variableManager ?? new VariableManager();
            _functionManager = functionManager ?? new FunctionManager();
            _stepExecutor = stepExecutor ?? new StepExecutor();
            _structureConverter = new UniversalStructureConverter();
        }

        public CustomAlgorithmResult Execute(CustomAlgorithmRequest request, IDataStructure structure)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Запрос не может быть null");

            if (structure == null)
                throw new ArgumentNullException(nameof(structure), "Структура данных не может быть null");

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
                if (context == null)
                {
                    try
                    {
                        context = CreateExecutionContext(request, structure);
                    }
                    catch
                    {
                        return CreateCriticalErrorResult(ex, stopwatch.Elapsed, request);
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
                Variables = new VariableScope(),
                FunctionStack = new FunctionStack(),
                StepHistory = new StepExecutionHistory(),
                ExpressionParser = _expressionParser,
                OperationExecutor = _operationExecutor
            };
        }

        private void InitializeExecution(ExecutionContext context)
        {
            InitializeStructureVariable(context);
            if (context.Request.variables == null)
            {
                context.Request.variables = new List<VariableDefinition>();
            }

            foreach (var variableDef in context.Request.variables)
            {
                if (string.IsNullOrWhiteSpace(variableDef.name))
                    throw new ArgumentException("Имя переменной не может быть пустым");

                IVariableValue variableValue = variableDef.type.ToLower() switch
                {
                    "array" => InitializeArrayVariable(variableDef, context),
                    "object" => InitializeObjectVariable(variableDef, context),
                    _ => InitializePrimitiveVariable(variableDef, context)
                };

                context.Variables.Set(variableDef.name, variableValue);
                Console.WriteLine($"✅ Инициализирована переменная '{variableDef.name}' типа {variableDef.type} = {variableValue}");
            }

            InitializeStandardVariables(context);
        }

        private IVariableValue InitializePrimitiveVariable(VariableDefinition variableDef, ExecutionContext context)
        {
            var initialValue = variableDef.initialValue?.ToString() ?? "";

            var result = EvaluateExpression(initialValue, context).ToValueString();

            return variableDef.type.ToLower() switch
            {
                "int" => CreateIntValue(result),
                "double" => CreateDoubleValue(result),
                "bool" => CreateBoolValue(result),
                "string" => CreateStringValue(result),
                _ => CreateStringValue(result) // По умолчанию строка
            };
        }

        private IVariableValue InitializeArrayVariable(VariableDefinition variableDef, ExecutionContext context)
        {
            var initialValue = variableDef.initialValue?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(initialValue))
                return new ArrayValue();

            // Попробуем парсить как JSON массив
            if (IsJsonArray(initialValue))
            {
                try
                {
                    return ArrayValue.CreateFromJsonArray(initialValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Ошибка парсинга JSON массива '{variableDef.name}': {ex.Message}");
                }
            }

            // Если не JSON, пробуем парсить как простой массив через запятую
            return ParseSimpleArray(initialValue);
        }

        private IVariableValue InitializeObjectVariable(VariableDefinition variableDef, ExecutionContext context)
        {
            var initialValue = variableDef.initialValue?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(initialValue))
                return new ObjectValue();

            try
            {
                // Пробуем парсить как JSON объект
                if (IsJsonObject(initialValue))
                {
                    using var jsonDocument = JsonDocument.Parse(initialValue);
                    var root = jsonDocument.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        var properties = ParseJsonElementToDictionary(root);
                        return new ObjectValue(properties);
                    }
                }

                // Если не JSON, создаем простой объект с одним свойством "value"
                var simpleProperties = new Dictionary<string, IVariableValue>
                {
                    ["value"] = CreateStringValue(initialValue)
                };
                return new ObjectValue(simpleProperties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка инициализации объекта '{variableDef.name}': {ex.Message}");
                return new ObjectValue();
            }
        }

        // Вспомогательные методы для создания значений
        private IVariableValue CreateIntValue(string value)
        {
            if (int.TryParse(value, out int result))
                return new IntValue(result);
            return new IntValue(0);
        }

        private IVariableValue CreateDoubleValue(string value)
        {
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return new DoubleValue(result);
            return new DoubleValue(0);
        }

        private IVariableValue CreateBoolValue(string value)
        {
            if (bool.TryParse(value, out bool result))
                return new BoolValue(result);

            // Поддержка различных форматов булевых значений
            var lowerValue = value.ToLower();
            return new BoolValue(lowerValue == "true" || lowerValue == "1" || lowerValue == "yes" || lowerValue == "да");
        }

        private IVariableValue CreateStringValue(string value)
        {
            // Убираем кавычки если они есть
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            else if (value.StartsWith("'") && value.EndsWith("'"))
                value = value.Substring(1, value.Length - 2);

            return new StringValue(value);
        }

        private IVariableValue ParseSimpleArray(string value)
        {
            var items = new List<IVariableValue>();

            if (string.IsNullOrWhiteSpace(value))
                return new ArrayValue(items);

            try
            {
                var elements = value.Split(',')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToArray();

                foreach (var element in elements)
                {
                    // Пробуем определить тип элемента
                    if (int.TryParse(element, out int intValue))
                        items.Add(new IntValue(intValue));
                    else if (double.TryParse(element, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
                        items.Add(new DoubleValue(doubleValue));
                    else if (bool.TryParse(element, out bool boolValue))
                        items.Add(new BoolValue(boolValue));
                    else
                        items.Add(CreateStringValue(element));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка парсинга простого массива: {ex.Message}");
            }

            return new ArrayValue(items);
        }

        private Dictionary<string, IVariableValue> ParseJsonElementToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, IVariableValue>();

            foreach (var property in element.EnumerateObject())
            {
                var value = ParseJsonElementValue(property.Value);
                dict[property.Name] = value;
            }

            return dict;
        }

        private IVariableValue ParseJsonElementValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => new ObjectValue(ParseJsonElementToDictionary(element)),
                JsonValueKind.Array => ParseJsonArray(element),
                JsonValueKind.String => new StringValue(element.GetString() ?? ""),
                JsonValueKind.Number => element.TryGetInt32(out int intVal)
                    ? new IntValue(intVal)
                    : new DoubleValue(element.GetDouble()),
                JsonValueKind.True => new BoolValue(true),
                JsonValueKind.False => new BoolValue(false),
                JsonValueKind.Null => new NullValue(),
                _ => new StringValue(element.ToString())
            };
        }

        private IVariableValue ParseJsonArray(JsonElement element)
        {
            var array = new List<IVariableValue>();
            foreach (var item in element.EnumerateArray())
            {
                array.Add(ParseJsonElementValue(item));
            }
            return new ArrayValue(array);
        }

        private bool IsJsonArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim();
            return trimmed.StartsWith("[") && trimmed.EndsWith("]");
        }

        private bool IsJsonObject(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim();
            return trimmed.StartsWith("{") && trimmed.EndsWith("}");
        }

        private void InitializeStandardVariables(ExecutionContext context)
        {
            var standardVars = new Dictionary<string, IVariableValue>
            {
                ["temp"] = new IntValue(0),
                ["swapped"] = new BoolValue(false),
                ["last_comparison"] = new IntValue(0),
                ["result"] = new IntValue(0)
            };

            foreach (var stdVar in standardVars)
            {
                if (!context.Variables.Contains(stdVar.Key))
                {
                    context.Variables.Set(stdVar.Key, stdVar.Value);
                }
            }
        }

        private void InitializeStructureVariable(ExecutionContext context)
        {
            try
            {
                var structValue = _structureConverter.ConvertToVariableValue(context.Structure);
                context.Variables.Set("struct", structValue);
                Console.WriteLine($"✅ Переменная 'struct' инициализирована для структуры типа {context.Structure.Type}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка инициализации переменной 'struct': {ex.Message}");
                // Создаем базовую структуру в случае ошибки
                var fallbackStruct = new ObjectValue(new Dictionary<string, IVariableValue>
                {
                    ["type"] = new StringValue(context.Structure.Type),
                    ["id"] = new StringValue(context.Structure.Id.ToString()),
                    ["error"] = new StringValue($"Failed to initialize: {ex.Message}")
                });
                context.Variables.Set("struct", fallbackStruct);
            }
        }

        // Остальные методы остаются без изменений
        private void ExecuteAlgorithm(ExecutionContext context)
        {
            if (context.Request.steps == null || !context.Request.steps.Any())
                throw new InvalidOperationException("Запрос должен содержать хотя бы один шаг");

            var entryPoint = "start";
            var startStep = context.Request.steps.FirstOrDefault(s => s.id == entryPoint);

            if (startStep == null && context.Request.functions != null)
            {
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

        private IVariableValue EvaluateExpression(string expression, ExecutionContext context)
        {
            try
            {
                var node = context.ExpressionParser.Parse(expression);
                return node.Evaluate(context.Variables);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка вычисления выражения '{expression}': {ex.Message}");
                return new IntValue(0);
            }
        }

        private bool EvaluateCondition(string condition, ExecutionContext context)
        {
            try
            {
                var result = EvaluateExpression(condition, context);
                return result.ToBool();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка вычисления условия '{condition}': {ex.Message}");
                return false;
            }
        }

        private int CalculateTreeHeight(TreeNode node) => node == null ? 0 : 1 + Math.Max(CalculateTreeHeight(node.Left), CalculateTreeHeight(node.Right));
        private int CountTreeNodes(TreeNode node) => node == null ? 0 : 1 + CountTreeNodes(node.Left) + CountTreeNodes(node.Right);
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
                executionState = ConvertVariablesToDictionary(context.Variables)
            };
        }

        private Dictionary<string, object> ConvertVariablesToDictionary(IVariableScope variables)
        {
            var result = new Dictionary<string, object>();
            try
            {
                var allVariables = variables.GetAllVariables();
                foreach (var variable in allVariables)
                {
                    result[variable.Key] = ConvertVariableValue(variable.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка преобразования переменных: {ex.Message}");
            }
            return result;
        }

        private object ConvertVariableValue(object value)
        {
            if (value is IVariableValue variableValue)
            {
                return variableValue switch
                {
                    IntValue intVal => intVal.RawValue,
                    DoubleValue doubleVal => doubleVal.RawValue,
                    BoolValue boolVal => boolVal.RawValue,
                    StringValue stringVal => stringVal.RawValue,
                    NullValue => null,
                    ArrayValue arrayVal => ConvertArrayValue(arrayVal),
                    ObjectValue objVal => ConvertObjectValue(objVal),
                    _ => variableValue.ToString()
                };
            }
            return value;
        }

        private List<object> ConvertArrayValue(ArrayValue arrayValue)
        {
            var result = new List<object>();
            for (int i = 0; i < arrayValue.Length; i++)
            {
                result.Add(ConvertVariableValue(arrayValue[i]));
            }
            return result;
        }

        private Dictionary<string, object> ConvertObjectValue(ObjectValue objectValue)
        {
            var result = new Dictionary<string, object>();
            var rawValue = objectValue.RawValue as Dictionary<string, IVariableValue>;

            if (rawValue != null)
            {
                foreach (var property in rawValue)
                {
                    result[property.Key] = ConvertVariableValue(property.Value);
                }
            }

            return result;
        }

        private Dictionary<string, object> CreateOutputData(ExecutionContext context)
        {
            return new Dictionary<string, object>
            {
                ["start_structure"] = context.Structure.GetOriginState(),
                ["final_structure"] = context.Variables.Get("struct").ToValueString(),
                ["variables"] = ConvertVariablesToDictionary(context.Variables),
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

        private CustomAlgorithmResult CreateCriticalErrorResult(Exception ex, TimeSpan executionTime, CustomAlgorithmRequest request)
        {
            return new CustomAlgorithmResult
            {
                success = false,
                message = $"Критическая ошибка: {ex.Message}",
                result = new AlgorithmResult
                {
                    AlgorithmName = request?.name ?? "Unknown",
                    ExecutionTime = executionTime
                }
            };
        }
    }
}