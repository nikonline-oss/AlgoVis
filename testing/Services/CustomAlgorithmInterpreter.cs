using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using testing.Models.Core;
using testing.Models.Custom;
using testing.Models.DataStructures;
using testing.Models.Visualization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace testing.Services
{
    public class CustomAlgori1thmInterpreter : ICustomAlgorithmInterpreter
    {
        private Dictionary<string, AlgorithmStep> _steps = new();
        private Stack<Dictionary<string, object>> _variableScopes = new Stack<Dictionary<string, object>>();
        private Dictionary<string, object> _globalVariables = new Dictionary<string, object>();
        private Dictionary<string, FunctionGroup> _functions = new();
        //private Dictionary<string, object> _variables = new();
        private List<VisualizationStep> _visualizationSteps = new();
        private AlgorithmStatistics _statistics = new();
        private IDataStructure _structure;
        private CustomAlgorithmRequest _request;
        private Stopwatch _stopwatch;

        private Stack<FunctionContext> _callStack = new Stack<FunctionContext>();
        private int _currentCallDepth = 0;
        private const int MAX_CALL_DEPTH = 100;

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
        //----------------
        private void ExecuteStep(string stepId)
        {
            if (string.IsNullOrEmpty(stepId)) return;

            if (_currentCallDepth > MAX_CALL_DEPTH)
            {
                throw new InvalidOperationException($"Превышена максимальная глубина рекурсии: {MAX_CALL_DEPTH}");
            }

            // Определяем, выполняем ли мы шаг из основной программы или из функции
            var (step, function) = GetStepAndFunction(stepId);


            if (step == null)
            {
                Console.WriteLine($"❌ Шаг '{stepId}' не найден");
                return;
            }

            Console.WriteLine($" шаг №{step.id}, тип :{step.type}, описание: {step.description}");


            switch (step.type.ToLower())
            {
                case "compare":
                    ExecuteCompare(step);
                    break;
                case "swap":
                    ExecuteSwap(step);
                    break;
                case "assign":
                    ExecuteAssign(step);
                    break;
                case "condition":
                    ExecuteCondition(step, function);
                    break;
                case "call_function": 
                    ExecuteFunctionCall(step);
                    break;
                case "return":
                    ExecuteReturn(step);
                    break;
                default:
                    ExecuteGenericStep(step);
                    break;
            }

            if (step.type != "condition" &&
                step.type != "call_function" &&
                step.type != "return")
            {
                string nextStepId = step.nextStep;

                // Если nextStep не указан явно, пытаемся получить следующий шаг по порядку
                if (string.IsNullOrEmpty(nextStepId))
                {
                    nextStepId = GetNextStep(stepId, function);
                }

                if (!string.IsNullOrEmpty(nextStepId))
                {
                    ExecuteStep(nextStepId);
                }
            }
            else if (!string.IsNullOrEmpty(step.nextStep))
            {
                ExecuteStep(step.nextStep);
            }
        }

        #region function
        //----------------
        private (AlgorithmStep step, FunctionGroup function) GetStepAndFunction(string stepId)
        {
            // Сначала ищем в основной программе
            if (_steps.ContainsKey(stepId))
            {
                return (_steps[stepId], null);
            }

            // Затем ищем в функциях
            foreach (var function in _functions.Values)
            {
                var step = function.steps.FirstOrDefault(s => s.id == stepId);
                if (step != null)
                {
                    return (step, function);
                }
            }

            return (null, null);
        }
        //----------------
        private string GetNextStep(string currentStepId, FunctionGroup function)
        {
            var steps = function?.steps ?? _request.steps;
            var stepIds = steps.Select(s => s.id).ToList();
            var currentIndex = stepIds.IndexOf(currentStepId);

            return currentIndex >= 0 && currentIndex < stepIds.Count - 1 ?
                stepIds[currentIndex + 1] : null;
        }
        //----------------
        private void ExecuteFunctionCall(AlgorithmStep step)
        {
            if (_currentCallDepth >= MAX_CALL_DEPTH)
                throw new InvalidOperationException($"Превышена максимальная глубина вызовов: {MAX_CALL_DEPTH}");

            if (!_functions.ContainsKey(step.functionName))
                throw new ArgumentException($"Функция '{step.functionName}' не найдена");

            var function = _functions[step.functionName];
            _statistics.RecursiveCalls++;
            _currentCallDepth++;

            // Создаем новую область видимости для функции
            var localVariables = new Dictionary<string, object>();
            _variableScopes.Push(localVariables);

            // Сохраняем контекст вызова
            var context = new FunctionContext
            {
                callerStepId = step.returnToStep,
                depth = _currentCallDepth,
                functionName = step.functionName
            };
            _callStack.Push(context);

            // Инициализируем параметры функции
            foreach (var param in step.functionParameters)
            {
                try
                {
                    var value = EvaluateExpression(param.Value);
                    localVariables[param.Key] = value;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Ошибка инициализации параметра {param.Key}: {ex.Message}");
                }
            }


            var description = step.description ?? $"Вызов функции: {step.functionName}";
            AddVisualizationStep("call_function", description, metadata: new Dictionary<string, object>
            {
                ["call_depth"] = _currentCallDepth,
                ["function_name"] = step.functionName,
                ["parameters"] = step.functionParameters
            });

            ExecuteStep(function.entryPoint);
        }
        //----------------
        private void ExecuteReturn(AlgorithmStep step)
        {
            if (_callStack.Count == 0)
            {
                return;
            }

            var context = _callStack.Pop();
            _currentCallDepth--;

            // Удаляем область видимости функции (если это не глобальная область)
            if (_variableScopes.Count > 1)
            {
                _variableScopes.Pop();
            }

            var description = step.description ?? $"Возврат из функции";
            AddVisualizationStep("return", description, metadata: new Dictionary<string, object>
            {
                ["call_depth"] = _currentCallDepth,
                ["function_name"] = context.functionName
            });

            // Возвращаемся к шагу после вызова
            if (!string.IsNullOrEmpty(context.callerStepId))
            {
                ExecuteStep(context.callerStepId);
            }
        }
        //----------------
        private void ExecuteCondition(AlgorithmStep step, FunctionGroup function)
        {
            var conditionResult = EvaluateCondition(step.parameters.FirstOrDefault() ?? "");
            var description = step.description ?? $"Проверка условия: {step.parameters.FirstOrDefault()}";

            AddVisualizationStep("condition", description, metadata: new Dictionary<string, object>
            {
                ["condition"] = step.parameters.FirstOrDefault(),
                ["result"] = conditionResult
            });

            var nextStep = conditionResult ?
                step.conditionCases.FirstOrDefault(c => c.condition == "true")?.nextStep :
                step.conditionCases.FirstOrDefault(c => c.condition == "false")?.nextStep;

            if (!string.IsNullOrEmpty(nextStep))
            {
                ExecuteStep(nextStep);
            }
        }
        #endregion
        //----------------
        private void ExecuteCompare(AlgorithmStep step)
        {
            _statistics.Comparisons++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Compare step requires 2 parameters");

            var index1 = EvaluateExpression(step.parameters[0]);
            var index2 = EvaluateExpression(step.parameters[1]);

            var array = GetArrayState();
            var value1 = array[ConvertToInt(index1)];
            var value2 = array[ConvertToInt(index2)];

            var comparisonResult = value1.CompareTo(value2);
            var description = step.description ?? $"Сравнение [{index1}]={value1} и [{index2}]={value2}";

            AddVisualizationStep("compare", description, new List<HighlightedElement>
            {
                new() { ElementId = index1.ToString(), HighlightType = "comparing", Color = "yellow" },
                new() { ElementId = index2.ToString(), HighlightType = "comparing", Color = "yellow" }
            }, new Dictionary<string, object>
            {
                ["comparison_result"] = comparisonResult,
                ["value1"] = value1,
                ["value2"] = value2
            });

            // Безопасно устанавливаем переменные сравнения
            SetVariableValue("last_comparison", comparisonResult);
            if (!string.IsNullOrEmpty(step.id))
            {
                SetVariableValue($"compare_{step.id}", comparisonResult);
            }
        }
        //------------------
        private void ExecuteSwap(AlgorithmStep step)
        {
            _statistics.Swaps++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Swap step requires 2 parameters");

            var index1 = EvaluateExpression(step.parameters[0]);
            var index2 = EvaluateExpression(step.parameters[1]);

            var array = GetArrayState();
            (array[ConvertToInt(index2)], array[ConvertToInt(index1)]) = (array[ConvertToInt(index1)], array[ConvertToInt(index2)]);

            UpdateArrayState(array);

            var description = step.description ?? $"Обмен элементов [{index1}] и [{index2}]";

            AddVisualizationStep("swap", description, new List<HighlightedElement>
            {
                new() { ElementId = index1.ToString(), HighlightType = "swapping", Color = "red" },
                new() { ElementId = index2.ToString(), HighlightType = "swapping", Color = "red" }
            });
        }
        //------------------
        private void ExecuteAssign(AlgorithmStep step)
        {
            if (step.parameters.Count < 2)
                throw new ArgumentException("Assign step requires 2 parameters (variable, value)");

            var variableName = step.parameters[0];
            var value = EvaluateExpression(step.parameters[1]);

            SetVariableValue(variableName, value);

            var description = step.description ?? $"Присвоение {variableName} = {value}";

            AddVisualizationStep("assign", description, metadata: new Dictionary<string, object>
            {
                ["variable"] = variableName,
                ["value"] = value
            });
        }

        //private void ExecuteCondition(AlgorithmStep step)
        //{
        //    var conditionResult = EvaluateCondition(step.parameters.FirstOrDefault() ?? "");
        //    var description = step.description ?? $"Проверка условия: {step.parameters.FirstOrDefault()}";

        //    AddVisualizationStep("condition", description, metadata: new Dictionary<string, object>
        //    {
        //        ["condition"] = step.parameters.FirstOrDefault(),
        //        ["result"] = conditionResult
        //    });

        //    var nextStep = conditionResult ?
        //        step.conditionCases.FirstOrDefault(c => c.condition == "true")?.nextStep :
        //        step.conditionCases.FirstOrDefault(c => c.condition == "false")?.nextStep;

        //    if (!string.IsNullOrEmpty(nextStep))
        //    {
        //        ExecuteStep(nextStep);
        //    }
        //}
        //------------------
        private void ExecuteGenericStep(AlgorithmStep step)
        {
            AddVisualizationStep(step.operation, step.description, metadata: step.metadata);
        }



        // Вспомогательные методы
        //----------------
        private Dictionary<string, object> InitializeVariables(List<VariableDefinition> variableDefs)
        {
            var variables = new Dictionary<string, object>
            {
                ["array_length"] = GetArrayState().Length
            };

            foreach (var varDef in variableDefs)
            {
                variables[varDef.name] = ParseVariableValue(varDef.type, varDef.initialValue.ToString());
            }

            return variables;
        }
        //----------------
        private object EvaluateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return 0;

            expression = expression.Trim();

            try
            {
                // Определяем тип выражения
                var expressionType = DetectExpressionType(expression);

                return expressionType switch
                {
                    "bool" => ParseBooleanValue(expression),
                    "string" => EvaluateStringExpression(expression),
                    _ => EvaluateNumericExpression(expression)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка вычисления выражения '{expression}': {ex.Message}");
                throw new ArgumentException($"Не удалось вычислить выражение: {expression}");
            }
        }
        //----------------
        private double EvaluateNumericExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return 0;

            expression = expression.Trim();

            // Обрабатываем доступ к элементам массива: array[index]
            if (expression.Contains("array["))
            {
                expression = ProcessArrayAccess(expression);
            }

            expression = ReplaceVariables(expression);
            Console.WriteLine($"🔢 After variable substitution: {expression}");

            try
            {
                expression = expression.Replace(" ", "");
                return EvaluateComplexExpression(expression);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error evaluating numeric expression '{expression}': {ex.Message}");
                throw;
            }
        }
        //----------------
        private string ProcessArrayAccess(string expression)
        {
            // Обрабатываем выражения типа array[index]
            var arrayAccessPattern = @"array\[([^\]]+)\]";
            var matches = Regex.Matches(expression, arrayAccessPattern);

            foreach (Match match in matches)
            {
                string indexExpr = match.Groups[1].Value;
                int index = ConvertToInt(EvaluateExpression(indexExpr));

                var array = GetArrayState();
                if (index >= 0 && index < array.Length)
                {
                    expression = expression.Replace(match.Value, array[index].ToString());
                }
                else
                {
                    throw new ArgumentException($"Invalid array index: {index}");
                }
            }

            return expression;
        }
        //----------------
        private double EvaluateComplexExpression(string expression)
        {
            // Обрабатываем выражения в скобках сначала
            while (expression.Contains('('))
            {
                int openParen = expression.LastIndexOf('(');
                int closeParen = expression.IndexOf(')', openParen);

                if (closeParen == -1)
                    throw new ArgumentException("Unmatched parentheses");

                string innerExpr = expression.Substring(openParen + 1, closeParen - openParen - 1);
                double innerResult = EvaluateComplexExpression(innerExpr);

                expression = expression.Substring(0, openParen) + innerResult.ToString() +
                            expression.Substring(closeParen + 1);
            }

            // Обрабатываем умножение и деление
            for (int i = 0; i < expression.Length; i++)
            {
                if (expression[i] == '*' || expression[i] == '/')
                {
                    double left = GetLeftOperand(expression, i);
                    double right = GetRightOperand(expression, i);
                    double result = expression[i] == '*' ? left * right : left / right;

                    expression = ReplaceOperation(expression, i, left, right, result);
                    i = -1; // Начинаем заново
                }
            }

            // Обрабатываем сложение и вычитание
            for (int i = 0; i < expression.Length; i++)
            {
                if ((expression[i] == '+' || expression[i] == '-') &&
                    (i == 0 || !char.IsDigit(expression[i - 1])))
                {
                    // Это унарный оператор, пропускаем
                    continue;
                }

                if (expression[i] == '+' || expression[i] == '-')
                {
                    double left = GetLeftOperand(expression, i);
                    double right = GetRightOperand(expression, i);
                    double result = expression[i] == '+' ? left + right : left - right;

                    expression = ReplaceOperation(expression, i, left, right, result);
                    i = -1; // Начинаем заново
                }
            }

            // Если осталось просто число
            if (double.TryParse(expression, out double finalResult))
            {
                Console.WriteLine($"🔢 Final expression result: {finalResult}");
                return finalResult;
            }

            throw new ArgumentException($"Could not evaluate expression: {expression}");
        }
        //----------------
        private double GetLeftOperand(string expression, int opIndex)
        {
            // Находим начало левого операнда
            int start = opIndex - 1;
            while (start >= 0 && (char.IsDigit(expression[start]) || expression[start] == '.' ||
                                  expression[start] == '-' && (start == 0 || !char.IsDigit(expression[start - 1]))))
            {
                start--;
            }
            start++; // Корректируем позицию

            string leftStr = expression.Substring(start, opIndex - start);
            if (double.TryParse(leftStr, out double result))
                return result;

            throw new ArgumentException($"Invalid left operand: {leftStr}");
        }
        //----------------
        private double GetRightOperand(string expression, int opIndex)
        {
            // Находим конец правого операнда
            int end = opIndex + 1;
            while (end < expression.Length && (char.IsDigit(expression[end]) || expression[end] == '.' ||
                                             (expression[end] == '-' && end == opIndex + 1)))
            {
                end++;
            }

            string rightStr = expression.Substring(opIndex + 1, end - opIndex - 1);
            if (double.TryParse(rightStr, out double result))
                return result;

            throw new ArgumentException($"Invalid right operand: {rightStr}");
        }
        //----------------
        private string ReplaceOperation(string expression, int opIndex, double left, double right, double result)
        {
            int leftStart = opIndex - left.ToString().Length;
            int rightEnd = opIndex + 1 + right.ToString().Length;

            return expression.Substring(0, leftStart) + result.ToString() + expression.Substring(rightEnd);
        }

        //----------------
        private string ReplaceVariables(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string result = input;
            int maxIterations = 3; // Уменьшили для безопасности

            for (int i = 0; i < maxIterations; i++)
            {
                string previous = result;
                bool changed = false;

                // Ищем переменные вида {имя} или просто имя
                var variablePattern = @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b";
                var matches = Regex.Matches(result, variablePattern);

                foreach (Match match in matches)
                {
                    string varName = match.Groups[1].Value;

                    // Пропускаем ключевые слова и числа
                    if (IsKeyword(varName) || double.TryParse(varName, out _))
                        continue;

                    try
                    {
                        var value = GetVariableValue(varName);
                        string replacement = GetVariableStringRepresentation(value);

                        // Заменяем только если нашли переменную и значение отличается
                        if (replacement != varName)
                        {
                            result = Regex.Replace(result, $@"\b{Regex.Escape(varName)}\b", replacement);
                            changed = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Ошибка при замене переменной {varName}: {ex.Message}");
                        // Не бросаем исключение, продолжаем работу
                    }
                }

                if (!changed || result == previous)
                    break;
            }

            return result;
        }
        //----------------
        private string GetVariableStringRepresentation(object value)
        {
            return value switch
            {
                bool boolVal => boolVal ? "1" : "0", // Для совместимости с числовыми выражениями
                double doubleVal => doubleVal.ToString(System.Globalization.CultureInfo.InvariantCulture),
                int intVal => intVal.ToString(),
                string strVal => strVal,
                _ => value?.ToString() ?? "0"
            };
        }
        //----------------
        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return false;

            condition = condition.Trim();
            //Console.WriteLine($"🔍 Evaluating condition: {condition}");

            // Сначала пробуем вычислить как булево выражение
            var boolResult = EvaluateBooleanCondition(condition);
            if (boolResult.HasValue)
                return boolResult.Value;

            // Затем пробуем как числовое сравнение
            return EvaluateNumericCondition(condition);
        }
        //----------------
        private int[] GetArrayState()
        {
            var state = _structure.GetState();
            if (state is int[] array)
                return array;

            throw new InvalidOperationException("Кастомные алгоритмы поддерживают только массивы");
        }
        //----------------
        private void UpdateArrayState(int[] array)
        {
            _structure.ApplyState(array);
        }

        //----------------
        private void AddVisualizationStep(string operation, string description,
            List<HighlightedElement> highlights = null, Dictionary<string, object> metadata = null)
        {
            _statistics.Steps++;

            var step = new VisualizationStep
            {
                stepNumber = _visualizationSteps.Count + 1,
                operation = operation,
                description = description,
                visualizationData = _structure.ToVisualizationData(),
                metadata = metadata ?? new Dictionary<string, object>()
            };

            if (highlights != null)
            {
                step.visualizationData.highlights.AddRange(highlights);
            }

            var arrayState = GetArrayState();
            step.metadata["array_state"] = arrayState;
            step.metadata["array_string"] = $"[{string.Join(", ", arrayState)}]";

            _visualizationSteps.Add(step);
        }

        //private string GetNextStep(string currentStepId)
        //{
        //    var stepIds = _request.steps.Select(s => s.id).ToList();
        //    var currentIndex = stepIds.IndexOf(currentStepId);

        //    return currentIndex >= 0 && currentIndex < stepIds.Count - 1 ?
        //        stepIds[currentIndex + 1] : null;
        //}
        //----------------
        private object ParseVariableValue(string type, string value)
        {
            try
            {
                return type.ToLower() switch
                {
                    "int" => int.Parse(EvaluateNumericExpression(value).ToString()),
                    "double" => EvaluateNumericExpression(value),
                    "bool" => ParseBooleanValue(value),
                    "string" => EvaluateStringExpression(value),
                    _ => EvaluateNumericExpression(value) // fallback to numeric
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка парсинга переменной: тип={type}, значение={value}, ошибка={ex.Message}");
                return type.ToLower() switch
                {
                    "bool" => false,
                    "string" => string.Empty,
                    _ => 0
                };
            }
        }
        //----------------
        private bool ParseBooleanValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim().ToLower();
            return value switch
            {
                "true" => true,
                "false" => false,
                "да" => true,
                "нет" => false,
                "yes" => true,
                "no" => false,
                _ => bool.Parse(value)
            };
        }
        //----------------
        private string EvaluateStringExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return string.Empty;

            expression = expression.Trim();

            // Обработка строк в кавычках
            if (expression.StartsWith("\"") && expression.EndsWith("\""))
            {
                return expression.Substring(1, expression.Length - 2);
            }

            // Замена переменных
            expression = ReplaceVariables(expression);

            // Вычисление выражений внутри строк
            expression = EvaluateExpressionsInString(expression);

            return expression;
        }
        //----------------
        private string EvaluateExpressionsInString(string input)
        {
            // Обрабатываем выражения в фигурных скобках: "Значение: {variable}"
            var pattern = @"\{([^}]+)\}";
            var matches = Regex.Matches(input, pattern);

            foreach (Match match in matches)
            {
                string expr = match.Groups[1].Value;
                object result;

                try
                {
                    // Пробуем вычислить как число
                    result = EvaluateNumericExpression(expr);
                }
                catch
                {
                    try
                    {
                        // Пробуем вычислить как булево значение
                        result = ParseBooleanValue(expr);
                    }
                    catch
                    {
                        // Оставляем как строку
                        result = expr;
                    }
                }

                input = input.Replace(match.Value, result.ToString());
            }

            return input;
        }
        //----------------
        private string DetectExpressionType(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return "double";

            expression = expression.Trim().ToLower();

            // Булевы литералы
            if (expression == "true" || expression == "false" ||
                expression == "да" || expression == "нет")
                return "bool";

            // Строковые литералы (в кавычках)
            if ((expression.StartsWith("\"") && expression.EndsWith("\"")) ||
                (expression.StartsWith("'") && expression.EndsWith("'")))
                return "string";

            // Содержит арифметические операторы - числовое выражение
            if (expression.Contains("+") || expression.Contains("-") ||
                expression.Contains("*") || expression.Contains("/") ||
                expression.Contains("(") || expression.Contains(")"))
                return "double";

            // По умолчанию считаем числовым
            return "double";
        }
        //----------------
        private bool? EvaluateBooleanCondition(string condition)
        {
            condition = condition.Trim();

            // Прямые булевы значения
            if (condition.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                condition.Equals("1", StringComparison.OrdinalIgnoreCase))
                return true;

            if (condition.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                condition.Equals("0", StringComparison.OrdinalIgnoreCase))
                return false;

            // Булевы операции
            if (condition.Contains("&&") || condition.Contains("||") || condition.Contains("!"))
            {
                return EvaluateBooleanExpression(condition);
            }

            // Простая булева переменная
            if (GetAllVariables().ContainsKey(condition) && GetAllVariables()[condition] is bool boolValue)
            {
                return boolValue;
            }

            return null;
        }
        //----------------
        private bool EvaluateBooleanExpression(string expression)
        {
            expression = ReplaceVariables(expression);
            expression = expression.Trim().ToLower();

            // Простые случаи
            if (expression.Contains("&&"))
            {
                var parts = expression.Split("&&", StringSplitOptions.RemoveEmptyEntries);
                return parts.All(part => EvaluateBooleanCondition(part.Trim()) == true);
            }

            if (expression.Contains("||"))
            {
                var parts = expression.Split("||", StringSplitOptions.RemoveEmptyEntries);
                return parts.Any(part => EvaluateBooleanCondition(part.Trim()) == true);
            }

            if (expression.StartsWith("!"))
            {
                var inner = expression.Substring(1).Trim();
                return EvaluateBooleanCondition(inner) != true;
            }

            // Одиночное выражение
            return EvaluateBooleanCondition(expression) == true;
        }
        //----------------
        private bool EvaluateNumericCondition(string condition)
        {
            condition = ReplaceVariables(condition);
            //Console.WriteLine($"🔍 After variable substitution: {condition}");

            var operators = new[] { ">=", "<=", "==", "!=", ">", "<" };

            foreach (var op in operators)
            {
                int opIndex = condition.IndexOf(op);
                if (opIndex >= 0)
                {
                    string leftPart = condition.Substring(0, opIndex).Trim();
                    string rightPart = condition.Substring(opIndex + op.Length).Trim();

                    //Console.WriteLine($"🔍 Split condition: '{leftPart}' {op} '{rightPart}'");

                    try
                    {
                        double leftValue = EvaluateNumericExpression(leftPart);
                        double rightValue = EvaluateNumericExpression(rightPart);

                        //Console.WriteLine($"🔍 Values: {leftValue} {op} {rightValue}");

                        return op switch
                        {
                            "<" => leftValue < rightValue,
                            "<=" => leftValue <= rightValue,
                            ">" => leftValue > rightValue,
                            ">=" => leftValue >= rightValue,
                            "==" => Math.Abs(leftValue - rightValue) < 0.0001,
                            "!=" => Math.Abs(leftValue - rightValue) > 0.0001,
                            _ => false
                        };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error evaluating condition parts: {ex.Message}");
                        return false;
                    }
                }
            }

            // Если нет операторов сравнения, проверяем как булево значение
            try
            {
                double result = EvaluateNumericExpression(condition);
                Console.WriteLine($"🔍 Numeric expression result: {result} (non-zero = true)");
                return Math.Abs(result) > 0.0001;
            }
            catch
            {
                // Если не удалось вычислить как число, пробуем как булево значение
                if (bool.TryParse(condition, out bool boolResult))
                {
                    Console.WriteLine($"🔍 Boolean parse result: {boolResult}");
                    return boolResult;
                }
            }

            Console.WriteLine($"❌ Could not evaluate condition: {condition}");
            return false;
        }
        //----------------
        private int ConvertToInt(object value)
        {
            return value switch
            {
                int i => i,
                double d => (int)d, // Явное приведение double к int
                float f => (int)f,
                decimal dec => (int)dec,
                string s when int.TryParse(s, out int result) => result,
                _ => throw new InvalidCastException($"Cannot convert {value?.GetType().Name} to int")
            };
        }

        //------------------
        private object GetVariableValue(string name)
        {
            // Сначала ищем в локальных областях видимости (сверху вниз)
            foreach (var scope in _variableScopes)
            {
                if (scope.ContainsKey(name))
                    return scope[name];
            }

            // Затем в глобальных переменных
            if (_globalVariables.ContainsKey(name))
                return _globalVariables[name];

            // Если переменная не найдена, создаем ее со значением по умолчанию
            var defaultValue = GetDefaultValueForVariable(name);
            SetVariableValue(name, defaultValue);
            return defaultValue;
        }
        //------------------
        private object GetDefaultValueForVariable(string name)
        {
            // Для стандартных переменных алгоритма возвращаем 0
            if (name.StartsWith("last_") || name.StartsWith("compare_") || name == "i" || name == "j")
                return 0;

            return 0; // int по умолчанию
        }

        //-------------------
        private void SetVariableValue(string name, object value)
        {
            if (_globalVariables.ContainsKey(name))
            {
                _globalVariables[name] = value;
                return;
            }

            _variableScopes.Peek()[name] = value;
            return;
        }

        //-------------------
        private Dictionary<string, object> GetAllVariables()
        {
            var allVariables = new Dictionary<string, object>();

            // Сначала добавляем глобальные переменные
            foreach (var variable in _globalVariables)
            {
                allVariables[variable.Key] = variable.Value;
            }

            // Затем перезаписываем локальными (более приоритетными)
            foreach (var scope in _variableScopes)
            {
                foreach (var variable in scope)
                {
                    allVariables[variable.Key] = variable.Value;
                }
            }

            return allVariables;
        }
        //-------------------
        private bool IsKeyword(string word)
        {
            string[] keywords = { "true", "false", "and", "or", "not", "array" };
            return keywords.Contains(word.ToLower());
        }
    }
}
