using AlgoVis.Core.Core.Interfaces;
using AlgoVis.Evaluator.Evaluator.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Functions;
using AlgoVis.Models.Models.Functions.Interfaces;
using AlgoVis.Models.Models.Operations;
using AlgoVis.Models.Models.Operations.Interfaces;
using AlgoVis.Models.Models.Steps;
using AlgoVis.Models.Models.Steps.Interfaces;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        public CustomAlgorithmResult Execute(CustomAlgorithmRequest request, IDataStructure structure)
        {
            var stopwatch = Stopwatch.StartNew();
            var context = CreateExecutionContext(request, structure);

            try
            {
                InitializeExecution(context);
                ExecuteAlgorithm(context);

                stopwatch.Stop();
                return CreateSuccessResult(context, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
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
            foreach (var variableDef in context.Request.variables)
            {
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

            // Установка специальных переменных
            var arrayLength = GetArrayLength(context.Structure);
            context.Variables.Set("array_length", new VariableValue(VariableType.Int, arrayLength));

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
                    Console.WriteLine($"⚠️ Ошибка инициализации массива '{variableDef.name}': {ex.Message}");
                    // Создаем пустой массив в случае ошибки
                }
            }


            return new VariableValue(array);
        }

        private VariableValue InitializeObject(VariableDefinition variableDef, ExecutionContext context)
        {
            var obj = new Dictionary<string, VariableValue>();

            // Инициализация из ObjectProperties
            if (variableDef.ObjectProperties != null && variableDef.ObjectProperties.Any())
            {
                foreach (var prop in variableDef.ObjectProperties)
                {
                    var propValue = ParseVariableValue(VariableType.Object, prop.Value?.ToString(), context);
                    obj[prop.Key] = new VariableValue(propValue);
                }
            }

            // Инициализация из initialValue (как JSON-строка)
            else if (variableDef.initialValue is string initStr && !string.IsNullOrWhiteSpace(initStr))
            {
                try
                {
                    // Упрощенный парсинг JSON-объекта для демонстрации
                    if (initStr.Trim().StartsWith("{") && initStr.Trim().EndsWith("}"))
                    {
                        var content = initStr.Trim().Trim('{', '}');
                        var properties = content.Split(',');

                        foreach (var property in properties)
                        {
                            var parts = property.Split(':');
                            if (parts.Length == 2)
                            {
                                var key = parts[0].Trim().Trim('"', '\'');
                                var value = parts[1].Trim();
                                var propValue = ParseVariableValue(VariableType.Object, value, context);
                                obj[key] = new VariableValue(propValue);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Ошибка инициализации объекта '{variableDef.name}': {ex.Message}");
                }
            }

            return new VariableValue(obj);
        }

        private object ParseVariableValue(VariableType type, string value, ExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(value))
                return VariableTypeHelper.CreateDefaultValue(type);

            try
            {
                // Используем наш вычислитель выражений
                var result = context.ExpressionEvaluator.Evaluate(value, context.Variables);
                return ExtractValue(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка парсинга переменной: тип={type}, значение={value}, ошибка={ex.Message}");
                return VariableTypeHelper.CreateDefaultValue(type);
            }
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
        }

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
        }

        private object GetDefaultValueForType(string type)
        {
            return type?.ToLower() switch
            {
                "int" => 0,
                "double" => 0.0,
                "bool" => false,
                "string" => string.Empty,
                _ => 0
            };
        }

        private object ExtractVariableValue(object value)
        {
            if (value is VariableValue variableValue)
            {
                // Для массивов преобразуем в список
                if (variableValue.Type == VariableType.Array)
                {
                    var list = new List<object>();
                    foreach (var item in variableValue.ArrayValue)
                    {
                        list.Add(ExtractVariableValue(item.Value));
                    }
                    return list;
                }

                // Для объектов преобразуем в словарь
                if (variableValue.Type == VariableType.Object)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in variableValue.ObjectValue)
                    {
                        dict[prop.Key] = ExtractVariableValue(prop.Value.Value);
                    }
                    return dict;
                }

                return variableValue.Value;
            }

            return value;
        }

        private void ExecuteAlgorithm(ExecutionContext context)
        {
            _stepExecutor.Execute("start", context);
        }

        private int GetArrayLength(IDataStructure structure)
        {
            try
            {
                var state = structure.GetState();
                if (state is int[] array)
                    return array.Length;
                if (state is Array genericArray)
                    return genericArray.Length;

                // Попробуем получить длину через рефлексию, если это другой тип массива
                var lengthProperty = state.GetType().GetProperty("Length");
                if (lengthProperty != null)
                    return (int)lengthProperty.GetValue(state);

                var countProperty = state.GetType().GetProperty("Count");
                if (countProperty != null)
                    return (int)countProperty.GetValue(state);


                throw new InvalidOperationException("Не удается определить длину структуры данных");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка получения длины массива: {ex.Message}");
                return 0; // Возвращаем 0 как fallback
            }
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
                ["custom_algorithm"] = true,
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
