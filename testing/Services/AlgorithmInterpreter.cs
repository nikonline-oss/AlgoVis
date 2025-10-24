using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Core;
using testing.Models.Custom;
using testing.Models.DataStructures;
using testing.Models.Evaluator;
using testing.Models.Functions;
using testing.Models.Operations;
using testing.Models.Steps;
using testing.Models.Visualization;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;


namespace testing.Services
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
                object value;

                if (variableType == VariableType.Array)
                {
                    // Инициализация массива
                    value = InitializeArray(variableDef, context);
                }
                else
                {
                    // Обычные переменные
                    value = ParseVariableValue(variableType, variableDef.initialValue?.ToString(), context);
                }

                // Создаем VariableValue и устанавливаем переменную
                var variableValue = new VariableValue(variableType, value);
                context.Variables.Set(variableDef.name, variableValue);
            }

            // Установка специальных переменных
            var arrayLength = GetArrayLength(context.Structure);
            context.Variables.Set("array_length", new VariableValue(VariableType.Int, arrayLength));
        }
        private object InitializeArray(VariableDefinition variableDef, ExecutionContext context)
        {
            // Определяем размер массива
            int size = variableDef.arraySize;
            if (size <= 0)
            {
                // Пытаемся вычислить размер из initialValue
                try
                {
                    var sizeValue = context.ExpressionEvaluator.Evaluate(
                        variableDef.initialValue?.ToString() ?? "10",
                        context.Variables);
                    size = Convert.ToInt32(sizeValue);
                }
                catch
                {
                    size = 100; // Размер по умолчанию
                }
            }

            // Создаем массив нужного типа
            var elementType = VariableTypeHelper.ParseType(variableDef.type);
            return VariableTypeHelper.CreateDefaultValue(elementType, size);
        }


        private object ParseVariableValue(VariableType type, string value, ExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(value))
                return VariableTypeHelper.CreateDefaultValue(type);

            try
            {
                // Используем наш вычислитель выражений
                var result = context.ExpressionEvaluator.Evaluate(value, context.Variables);
                var extractedValue = ExtractValue(result);

                // Приводим к нужному типу
                return type switch
                {
                    VariableType.Int => Convert.ToInt32(extractedValue),
                    VariableType.Double => Convert.ToDouble(extractedValue),
                    VariableType.Bool => Convert.ToBoolean(extractedValue),
                    VariableType.String => extractedValue.ToString(),
                    _ => extractedValue
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка парсинга переменной: тип={type}, значение={value}, ошибка={ex.Message}");
                return VariableTypeHelper.CreateDefaultValue(type);
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
            return new Dictionary<string, object>
            {
                ["start_structure"] = context.Structure.GetOriginState(),
                ["final_structure"] = context.Structure.GetState(),
                ["custom_algorithm"] = true,
                ["variables"] = context.Variables.GetAllVariables(),
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
