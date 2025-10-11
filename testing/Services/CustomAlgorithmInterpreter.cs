using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using testing.Models.Core;
using testing.Models.Custom;
using testing.Models.DataStructures;
using testing.Models.Visualization;

namespace testing.Services
{
    public class CustomAlgorithmInterpreter : ICustomAlgorithmInterpreter
    {
        private Dictionary<string, AlgorithmStep> _steps;
        private Dictionary<string, object> _variables;
        private List<VisualizationStep> _visualizationSteps;
        private AlgorithmStatistics _statistics;
        private IDataStructure _structure;
        private CustomAlgorithmRequest _request;

        public CustomAlgorithmResult Execute(CustomAlgorithmRequest request, IDataStructure structure)
        {
            try
            {
                _request = request;
                _structure = structure;
                _steps = request.steps.ToDictionary(s => s.id);
                _variables = InitializeVariables(request.variables);
                _visualizationSteps = new List<VisualizationStep>();
                _statistics = new AlgorithmStatistics();

                // Начальный шаг
                AddVisualizationStep("start", "Начало выполнения кастомного алгоритма");

                // Выполняем алгоритм
                ExecuteStep("start");

                AddVisualizationStep("complete", "Кастомный алгоритм завершен");

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
                        ExecutionTime = TimeSpan.Zero,
                        OutputData = new Dictionary<string, object>
                        {
                            ["custom_algorithm"] = true,
                            ["variables"] = _variables
                        }
                    },
                    executionState = new Dictionary<string, object>(_variables)
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

        private void ExecuteStep(string stepId)
        {
            if (!_steps.ContainsKey(stepId)) return;

            var step = _steps[stepId];

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
                    ExecuteCondition(step);
                    return;
                case "loop":
                    ExecuteLoop(step);
                    return;
                default:
                    ExecuteGenericStep(step);
                    break;
            }

            if (string.IsNullOrEmpty(step.nextStep) && step.type != "condition" && step.type != "loop")
            {
                var nextStep = GetNextStep(stepId);
                if (!string.IsNullOrEmpty(nextStep))
                {
                    ExecuteStep(nextStep);
                }
            }
            else if (!string.IsNullOrEmpty(step.nextStep))
            {
                ExecuteStep(step.nextStep);
            }
        }

        private void ExecuteCompare(AlgorithmStep step)
        {
            _statistics.Comparisons++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Compare step requires 2 parameters");

            var index1 = EvaluateExpression(step.parameters[0]);
            var index2 = EvaluateExpression(step.parameters[1]);

            var array = GetArrayState();
            var value1 = array[(int)index1];
            var value2 = array[(int)index2];

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

            _variables["last_comparison"] = comparisonResult;
            _variables[$"compare_{step.id}"] = comparisonResult;
        }

        private void ExecuteSwap(AlgorithmStep step)
        {
            _statistics.Swaps++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Swap step requires 2 parameters");

            var index1 = EvaluateExpression(step.parameters[0]);
            var index2 = EvaluateExpression(step.parameters[1]);

            var array = GetArrayState();
            (array[(int)index2], array[(int)index1]) = (array[(int)index1], array[(int)index2]);

            UpdateArrayState(array);

            var description = step.description ?? $"Обмен элементов [{index1}] и [{index2}]";

            AddVisualizationStep("swap", description, new List<HighlightedElement>
            {
                new() { ElementId = index1.ToString(), HighlightType = "swapping", Color = "red" },
                new() { ElementId = index2.ToString(), HighlightType = "swapping", Color = "red" }
            });
        }

        private void ExecuteAssign(AlgorithmStep step)
        {
            if (step.parameters.Count < 2)
                throw new ArgumentException("Assign step requires 2 parameters (variable, value)");

            var variableName = step.parameters[0];
            var value = EvaluateExpression(step.parameters[1]);

            _variables[variableName] = value;

            var description = step.description ?? $"Присвоение {variableName} = {value}";

            //AddVisualizationStep("assign", description, metadata: new Dictionary<string, object>
            //{
            //    ["variable"] = variableName,
            //    ["value"] = value
            //});
        }

        private void ExecuteCondition(AlgorithmStep step)
        {
            var conditionResult = EvaluateCondition(step.parameters.FirstOrDefault() ?? "");
            var description = step.description ?? $"Проверка условия: {step.parameters.FirstOrDefault()}";

            //AddVisualizationStep("condition", description, metadata: new Dictionary<string, object>
            //{
            //    ["condition"] = step.parameters.FirstOrDefault(),
            //    ["result"] = conditionResult
            //});

            var nextStep = conditionResult ?
                step.conditionCases.FirstOrDefault(c => c.condition == "true")?.nextStep :
                step.conditionCases.FirstOrDefault(c => c.condition == "false")?.nextStep;

            if (!string.IsNullOrEmpty(nextStep))
            {
                ExecuteStep(nextStep);
            }
        }

        private void ExecuteLoop(AlgorithmStep step)
        {
            var loopConfig = _request.loops.FirstOrDefault(l => l.id == step.parameters.FirstOrDefault());
            if (loopConfig == null) return;

            switch (loopConfig.type.ToLower())
            {
                case "for":
                    ExecuteForLoop(loopConfig);
                    break;
                case "while":
                    ExecuteWhileLoop(loopConfig);
                    break;
            }
        }

        private void ExecuteForLoop(LoopDefinition loop)
        {
            var from = EvaluateExpression(loop.from);
            var to = EvaluateExpression(loop.to);

            _variables[loop.variable] = from;

            //AddVisualizationStep("loop_start", $"Начало цикла for: {loop.variable} от {from} до {to}",
            //    metadata: new Dictionary<string, object>
            //    {
            //        ["loop_type"] = "for",
            //        ["variable"] = loop.variable,
            //        ["from"] = from,
            //        ["to"] = to
            //    });

            for (int i = (int)from; i < (int)to; i++)
            {
                _variables[loop.variable] = i;

                foreach (var stepId in loop.steps)
                {
                    ExecuteStep(stepId);
                }

                //AddVisualizationStep("loop_iteration", $"Итерация цикла: {loop.variable} = {i}",
                //    metadata: new Dictionary<string, object>
                //    {
                //        ["iteration"] = i,
                //        ["variable"] = loop.variable
                //    });
            }

            //AddVisualizationStep("loop_end", "Цикл for завершен");
        }

        private void ExecuteWhileLoop(LoopDefinition loop)
        {
            int iteration = 0;

            //AddVisualizationStep("loop_start", $"Начало цикла while: {loop.condition}",
            //    metadata: new Dictionary<string, object>
            //    {
            //        ["loop_type"] = "while",
            //        ["condition"] = loop.condition
            //    });

            while (EvaluateCondition(loop.condition) && iteration < 1000)
            {
                foreach (var stepId in loop.steps)
                {
                    ExecuteStep(stepId);
                }

                iteration++;
                //AddVisualizationStep("loop_iteration", $"Итерация цикла while: {iteration}",
                //    metadata: new Dictionary<string, object>
                //    {
                //        ["iteration"] = iteration,
                //        ["condition"] = loop.condition
                //    });
            }

            //AddVisualizationStep("loop_end", "Цикл while завершен");
        }

        private void ExecuteGenericStep(AlgorithmStep step)
        {
            AddVisualizationStep(step.operation, step.description, metadata: step.metadata);
        }

        // Вспомогательные методы
        private Dictionary<string, object> InitializeVariables(List<VariableDefinition> variableDefs)
        {
            var variables = new Dictionary<string, object>
            {
                ["array_length"] = GetArrayState().Length
            };

            foreach (var varDef in variableDefs)
            {
                variables[varDef.name] = varDef.initialValue;
            }

            return variables;
        }

        private double EvaluateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return 0;

            expression = expression.Trim();
            Console.WriteLine($"🔢 Evaluating expression: {expression}");

            // Сначала заменяем переменные
            foreach (var variable in _variables)
            {
                expression = expression.Replace(variable.Key, variable.Value.ToString());
            }

            Console.WriteLine($"🔢 After variable substitution: {expression}");

            try
            {
                // Удаляем пробелы для упрощения парсинга
                expression = expression.Replace(" ", "");

                // Обрабатываем сложные выражения с приоритетами операций
                return EvaluateComplexExpression(expression);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error evaluating expression '{expression}': {ex.Message}");
                throw;
            }
        }

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

        private string ReplaceOperation(string expression, int opIndex, double left, double right, double result)
        {
            int leftStart = opIndex - left.ToString().Length;
            int rightEnd = opIndex + 1 + right.ToString().Length;

            return expression.Substring(0, leftStart) + result.ToString() + expression.Substring(rightEnd);
        }

        // Новый метод для безопасной замены переменных
        private string ReplaceVariables(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string result = input;

            // Сортируем переменные по длине (от самых длинных к самым коротким)
            // чтобы избежать проблем с частичным перекрытием (например, "i" и "index")
            var sortedVariables = _variables
                .OrderByDescending(v => v.Key.Length)
                .ThenByDescending(v => v.Key);

            foreach (var variable in sortedVariables)
            {
                // Используем регулярное выражение для замены только целых слов
                string pattern = $@"\b{Regex.Escape(variable.Key)}\b";
                result = Regex.Replace(result, pattern, variable.Value.ToString());
            }

            return result;
        }

        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return false;

            condition = condition.Trim();

            Console.WriteLine($"🔍 Evaluating condition: {condition}"); // Отладочная информация

            // Безопасная замена переменных
            condition = ReplaceVariables(condition);
            Console.WriteLine($"🔍 After variable substitution: {condition}");


            // Поддерживаемые операторы (в порядке приоритета для разделения)
            var operators = new[] { ">=", "<=", "==", "!=", ">", "<" };

            foreach (var op in operators)
            {
                int opIndex = condition.IndexOf(op);
                if (opIndex >= 0)
                {
                    // Разделяем условие на левую и правую части
                    string leftPart = condition.Substring(0, opIndex).Trim();
                    string rightPart = condition.Substring(opIndex + op.Length).Trim();

                    Console.WriteLine($"🔍 Split condition: '{leftPart}' {op} '{rightPart}'");

                    try
                    {
                        // Вычисляем обе части как выражения
                        double leftValue = EvaluateExpression(leftPart);
                        double rightValue = EvaluateExpression(rightPart);

                        Console.WriteLine($"🔍 Values: {leftValue} {op} {rightValue}");

                        // Выполняем сравнение
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

            // Если нет операторов сравнения, пытаемся вычислить как булево выражение
            try
            {
                double result = EvaluateExpression(condition);
                Console.WriteLine($"🔍 Boolean expression result: {result} (non-zero = true)");
                return Math.Abs(result) > 0.0001; // Любое ненулевое значение = true
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

        private int[] GetArrayState()
        {
            var state = _structure.GetState();
            if (state is int[] array)
                return array;

            throw new InvalidOperationException("Кастомные алгоритмы поддерживают только массивы");
        }

        private void UpdateArrayState(int[] array)
        {
            _structure.ApplyState(array);
        }

        private void AddVisualizationStep(string operation, string description,
            List<HighlightedElement> highlights = null, Dictionary<string, object> metadata = null)
        {
            _statistics.Steps++;

            var step = new VisualizationStep
            {
                StepNumber = _visualizationSteps.Count + 1,
                Operation = operation,
                Description = description,
                VisualizationData = _structure.ToVisualizationData(),
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            if (highlights != null)
            {
                step.VisualizationData.Highlights.AddRange(highlights);
            }

            _visualizationSteps.Add(step);
        }

        private string GetNextStep(string currentStepId)
        {
            var stepIds = _request.steps.Select(s => s.id).ToList();
            var currentIndex = stepIds.IndexOf(currentStepId);

            return currentIndex >= 0 && currentIndex < stepIds.Count - 1 ?
                stepIds[currentIndex + 1] : null;
        }
    }
}
