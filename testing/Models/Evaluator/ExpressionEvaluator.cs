using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using testing.Models.Evaluator.Token;

namespace testing.Models.Evaluator
{
    public class ExpressionEvaluator : IExpressionEvaluator
    {
        public object Evaluate(string expression, IVariableScope variables)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return 0;

            expression = expression.Trim();

            try
            {
                // Сложный доступ: obj.property.array[index].subproperty
                if (ContainsComplexAccess(expression))
                {
                    return EvaluateComplexAccess(expression, variables);
                }

                // Доступ к элементам массива: array[index]
                if (expression.Contains("[") && expression.Contains("]"))
                {
                    return EvaluateArrayAccess(expression, variables);
                }

                // Доступ к свойствам объекта: obj.property
                if (expression.Contains(".") && !expression.Contains("["))
                {
                    return EvaluatePropertyAccess(expression, variables);
                }

                // Проверяем простые случаи
                if (IsSimpleValue(expression, out object simpleResult))
                    return simpleResult;

                if (IsBooleanLiteral(expression, out bool boolResult))
                    return boolResult;

                // Токенизируем и парсим выражение
                var tokenizer = new ExpressionTokenizer();
                var tokens = tokenizer.Tokenize(expression);

                var parser = new ExpressionParser(tokens);
                var expressionTree = parser.Parse();

                // Вычисляем выражение
                var result = expressionTree.Evaluate(variables);

                // Если результат - VariableValue, извлекаем его значение
                return ExtractValue(result);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка вычисления выражения '{expression}': {ex.Message}", ex);
            }
        }

        private bool ContainsComplexAccess(string expression)
        {
            return expression.Contains(".") && expression.Contains("[");
        }

        private object EvaluatePropertyAccess(string expression, IVariableScope variables)
        {
            return variables.Get(expression);
        }

        private object EvaluateComplexAccess(string expression, IVariableScope variables)
        {
            // Разбираем выражения вида: obj.array[0].property
            var parts = new List<string>();
            var currentPart = new StringBuilder();
            bool inBrackets = false;

            foreach (char c in expression)
            {
                if (c == '[') inBrackets = true;
                if (c == ']') inBrackets = false;

                if (c == '.' && !inBrackets)
                {
                    parts.Add(currentPart.ToString());
                    currentPart.Clear();
                }
                else
                {
                    currentPart.Append(c);
                }
            }
            parts.Add(currentPart.ToString());

            // Оцениваем первую часть
            object current = EvaluateSimpleExpression(parts[0], variables);

            // Обрабатываем остальные части
            for (int i = 1; i < parts.Count; i++)
            {
                var part = parts[i];

                if (part.Contains("["))
                {
                    // Доступ к массиву: property[index]
                    var arrayMatch = Regex.Match(part, @"^([a-zA-Z_][a-zA-Z0-9_]*)\[(.+)\]$");
                    if (arrayMatch.Success)
                    {
                        string propertyName = arrayMatch.Groups[1].Value;
                        string indexExpr = arrayMatch.Groups[2].Value;

                        if (current is VariableValue currentVar)
                        {
                            current = currentVar.GetProperty(propertyName);
                            int index = Convert.ToInt32(Evaluate(indexExpr, variables));

                            if (current is VariableValue arrayVar)
                            {
                                current = arrayVar.GetElement(index);
                            }
                        }
                    }
                }
                else
                {
                    // Простое свойство
                    if (current is VariableValue currentVar)
                    {
                        current = currentVar.GetProperty(part);
                    }
                }
            }

            return ExtractValue(current);
        }

        private object EvaluateSimpleExpression(string expression, IVariableScope variables)
        {
            if (IsSimpleValue(expression, out object result))
                return result;

            return variables.Get(expression);
        }

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
        }

        private object EvaluateArrayAccess(string expression, IVariableScope variables)
        {
            // Обрабатываем выражения типа array[index] или array[index1 + index2]
            var pattern = @"^([a-zA-Z_][a-zA-Z0-9_]*)\[(.+)\]$";
            var match = Regex.Match(expression, pattern);

            if (match.Success)
            {
                string arrayName = match.Groups[1].Value;
                string indexExpression = match.Groups[2].Value;

                // Вычисляем индекс
                int index = Convert.ToInt32(Evaluate(indexExpression, variables));

                // Получаем элемент массива
                return variables.GetElement(arrayName, index);
            }

            throw new ArgumentException($"Некорректный доступ к массиву: {expression}");
        }

        private bool IsSimpleValue(string expression, out object result)
        {
            result = null;

            // Проверяем, является ли выражение числом
            if (double.TryParse(expression, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                result = number;
                return true;
            }

            // Проверяем, является ли выражение строковой константой
            if (expression.StartsWith("\"") && expression.EndsWith("\""))
            {
                result = expression.Substring(1, expression.Length - 2);
                return true;
            }

            return false;
        }

        private bool IsBooleanLiteral(string expression, out bool result)
        {
            var lower = expression.ToLower();

            switch (lower)
            {
                case "true":
                case "истина":
                case "да":
                case "yes":
                    result = true;
                    return true;

                case "false":
                case "ложь":
                case "нет":
                case "no":
                    result = false;
                    return true;

                default:
                    result = false;
                    return false;
            }
        }

        public bool EvaluateCondition(string condition, IVariableScope variables)
        {
            var result = Evaluate(condition, variables);
            var value = ExtractValue(result);

            return value switch
            {
                bool boolValue => boolValue,
                int intValue => intValue != 0,
                double doubleValue => Math.Abs(doubleValue) > 1e-10,
                string strValue => !string.IsNullOrEmpty(strValue),
                _ => Convert.ToBoolean(value)
            };
        }

    }
}
