using System.Text.RegularExpressions;
using testing.Models.Custom;

namespace testing.Services
{
    public partial class CustomAlgorithmInterpreter
    {
        // Методы для работы с переменными: инициализация, получение, установка, проверка существования
        // InitializeVariables, GetVariableValue, SetVariableValue, VariableExists, GetAllVariables
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
        private object GetDefaultValueForVariable(string name)
        {
            // Для стандартных переменных алгоритма возвращаем 0
            if (name.StartsWith("last_") || name.StartsWith("compare_") || name == "i" || name == "j")
                return 0;

            return 0; // int по умолчанию
        }
        private bool IsKeyword(string word)
        {
            string[] keywords = { "true", "false", "and", "or", "not", "array" };
            return keywords.Contains(word.ToLower());
        }
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
    }
}