using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Core;
using testing.Models.Custom;
using testing.Models.DataStructures;
using testing.Models.Visualization;

namespace testing.Services
{
    /// <summary>
    /// Основной интерпретатор пользовательских алгоритмов.
    /// </summary>
    public partial class CustomAlgorithmInterpreter : ICustomAlgorithmInterpreter
    {
        // Основные поля для хранения состояния выполнения
        private Dictionary<string, AlgorithmStep> _steps = new();
        private Stack<Dictionary<string, object>> _variableScopes = new();
        private Dictionary<string, object> _globalVariables = new();
        private Dictionary<string, FunctionGroup> _functions = new();
        private List<VisualizationStep> _visualizationSteps = new();
        private AlgorithmStatistics _statistics = new();
        private IDataStructure _structure;
        private CustomAlgorithmRequest _request;
        private Stopwatch _stopwatch;
        private Stack<FunctionContext> _callStack = new();
        private int _currentCallDepth = 0;
        private const int MAX_CALL_DEPTH = 100;

        /// <summary>
        /// Выполняет пользовательский алгоритм и возвращает результат.
        /// </summary>
        public CustomAlgorithmResult Execute(CustomAlgorithmRequest request, IDataStructure structure)
        {
            _stopwatch = Stopwatch.StartNew();
            try
            {
                _request = request;
                _structure = structure;
                _steps = request.steps.ToDictionary(s => s.id);
                _functions = request.functions?.ToDictionary(s => s.name) ?? new Dictionary<string, FunctionGroup>();
                _visualizationSteps = new List<VisualizationStep>();
                _statistics = new AlgorithmStatistics();
                _callStack.Clear();
                _currentCallDepth = 0;
                _variableScopes.Clear();
                _globalVariables.Clear();

                _globalVariables = InitializeVariables(request.variables);
                _globalVariables["array_length"] = GetArrayState().Length;

                _variableScopes.Push(new Dictionary<string, object>());

                var originalArray = GetArrayState().Clone();

                // Выполняем алгоритм
                ExecuteStep("start");

                _stopwatch.Stop();

                return new CustomAlgorithmResult
                {
                    success = true,
                    message = "Алгоритм выполнен успешно",
                    result = new AlgorithmResult
                    {
                        AlgorithmName = request.name,
                        SessionId = Guid.NewGuid().ToString(),
                        StructureType = structure.Type,
                        Steps = _visualizationSteps,
                        Statistics = _statistics,
                        ExecutionTime = _stopwatch.Elapsed,
                        OutputData = new Dictionary<string, object>
                        {
                            ["start_structure"] = originalArray,
                            ["final_structure"] = GetArrayState(),
                            ["custom_algorithm"] = true,
                            ["variables"] = GetAllVariables(),
                            ["call_depth"] = _currentCallDepth,
                            ["function_calls"] = _statistics.RecursiveCalls
                        }
                    },
                    executionState = new Dictionary<string, object>(GetAllVariables())
                };
            }
            catch (Exception ex)
            {
                return new CustomAlgorithmResult
                {
                    success = false,
                    message = $"Ошибка выполнения: {ex.Message}",
                    result = new AlgorithmResult()
                };
            }
        }
    }
}
