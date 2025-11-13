using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Parsing;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Core
{
    public class ExpressionEvaluator : IExpressionEvaluator
    {
        public object Evaluate(string expression, IVariableScope variables)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return 0;

            expression = expression.Trim();

            Console.WriteLine($"🔍 Вычисление выражения: '{expression}'");

            try
            {
                // Проверяем простые случаи
                if (IsSimpleValue(expression, out object simpleResult))
                {
                    Console.WriteLine($"🔍 Простое значение: {simpleResult}, тип = {simpleResult?.GetType()}");
                    return simpleResult;
                }

                if (IsBooleanLiteral(expression, out bool boolResult))
                {
                    Console.WriteLine($"🔍 Булево значение: {boolResult}");
                    return boolResult;
                }

                // Токенизируем и парсим выражение
                var tokenizer = new ExpressionTokenizer();
                var tokens = tokenizer.Tokenize(expression);
                Console.WriteLine($"🔍 Токены: {string.Join(", ", tokens.Select(t => $"{t.Type}:'{t.Value}'"))}");

                var parser = new ExpressionParser(tokens);
                var expressionTree = parser.Parse();

                // Вычисляем выражение
                var result = expressionTree.Evaluate(variables);
                var finalResult = ExtractValue(result);

                Console.WriteLine($"🔍 Результат вычисления: {finalResult}, тип = {finalResult?.GetType()}");

                return finalResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка вычисления выражения '{expression}': {ex.Message}");
                throw new ArgumentException($"Ошибка вычисления выражения '{expression}': {ex.Message}", ex);
            }
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

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
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
