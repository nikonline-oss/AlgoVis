using System.Text.RegularExpressions;

namespace testing.Services
{
    public partial class CustomAlgorithmInterpreter
    {
        // Методы для вычисления выражений: числовых, строковых, булевых, условий
        // EvaluateExpression, EvaluateNumericExpression, EvaluateStringExpression, EvaluateCondition и др.

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



    }
}