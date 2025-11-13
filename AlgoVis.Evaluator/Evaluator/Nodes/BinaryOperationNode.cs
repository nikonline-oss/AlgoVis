using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class BinaryOperationNode : IExpressionNode
    {
        private readonly IExpressionNode _left;
        private readonly IExpressionNode _right;
        private readonly string _operator;

        public BinaryOperationNode(IExpressionNode left, IExpressionNode right, string op)
        {
            _left = left;
            _right = right;
            _operator = op;
        }

        public object Evaluate(IVariableScope variables)
        {
            var leftVal = ExtractValue(_left.Evaluate(variables));
            var rightVal = ExtractValue(_right.Evaluate(variables));

            // ДОБАВЛЕНО: Строковые операции
            if (_operator == "+")
            {
                // Конкатенация строк
                if (leftVal is string leftStr || rightVal is string rightStr)
                {
                    return string.Concat(leftVal?.ToString() ?? "", rightVal?.ToString() ?? "");
                }
            }

            // Для логических операторов
            if (_operator == "&&" || _operator == "||")
            {
                bool leftBool = ConvertToBoolean(leftVal);
                bool rightBool = ConvertToBoolean(rightVal);

                return _operator switch
                {
                    "&&" => leftBool && rightBool,
                    "||" => leftBool || rightBool,
                    _ => throw new ArgumentException($"Неизвестный логический оператор: {_operator}")
                };
            }

            // ДОБАВЛЕНО: Строковые сравнения
            if (_operator == "==" || _operator == "!=" || _operator == "<" || _operator == ">" ||
                _operator == "<=" || _operator == ">=")
            {
                // Если оба операнда - строки
                if (leftVal is string leftString && rightVal is string rightString)
                {
                    int comparison = string.Compare(leftString, rightString, StringComparison.Ordinal);

                    return _operator switch
                    {
                        "==" => comparison == 0,
                        "!=" => comparison != 0,
                        "<" => comparison < 0,
                        ">" => comparison > 0,
                        "<=" => comparison <= 0,
                        ">=" => comparison >= 0,
                        _ => throw new ArgumentException($"Неизвестный оператор сравнения: {_operator}")
                    };
                }

                // Стандартное сравнение для других типов
                bool areEqual = ObjectsEqual(leftVal, rightVal);

                if (_operator == "==") return areEqual;
                if (_operator == "!=") return !areEqual;

                // Для остальных операторов сравнения требуем числа
                if (IsNumeric(leftVal) && IsNumeric(rightVal))
                {
                    double leftNum = Convert.ToDouble(leftVal);
                    double rightNum = Convert.ToDouble(rightVal);

                    return _operator switch
                    {
                        "<" => leftNum < rightNum,
                        ">" => leftNum > rightNum,
                        "<=" => leftNum <= rightNum,
                        ">=" => leftNum >= rightNum,
                        _ => throw new ArgumentException($"Неизвестный оператор: {_operator}")
                    };
                }

                throw new ArgumentException($"Невозможно сравнить {leftVal?.GetType().Name} и {rightVal?.GetType().Name} с оператором {_operator}");
            }

            // Для числовых операторов
            if (IsNumeric(leftVal) && IsNumeric(rightVal))
            {
                double leftNum = Convert.ToDouble(leftVal);
                double rightNum = Convert.ToDouble(rightVal);

                return _operator switch
                {
                    "+" => leftNum + rightNum,
                    "-" => leftNum - rightNum,
                    "*" => leftNum * rightNum,
                    "/" => rightNum != 0 ? leftNum / rightNum : throw new DivideByZeroException("Деление на ноль"),
                    "%" => rightNum != 0 ? leftNum % rightNum : throw new DivideByZeroException("Деление на ноль"),
                    "^" => Math.Pow(leftNum, rightNum),
                    _ => throw new ArgumentException($"Неизвестный оператор: {_operator}")
                };
            }

            throw new ArgumentException($"Неподдерживаемые типы для оператора {_operator}: {leftVal?.GetType().Name} и {rightVal?.GetType().Name}");
        }

        private object ExtractValue(object value)
        {
            return value is VariableValue variableValue ? variableValue.Value : value;
        }

        private bool ConvertToBoolean(object value)
        {
            var extractedValue = ExtractValue(value);

            return extractedValue switch
            {
                bool b => b,
                int i => i != 0,
                double d => Math.Abs(d) > 1e-10,
                string s => !string.IsNullOrEmpty(s),
                _ => Convert.ToBoolean(extractedValue)
            };
        }

        private bool ObjectsEqual(object a, object b)
        {
            var aVal = ExtractValue(a);
            var bVal = ExtractValue(b);

            if (aVal == null && bVal == null) return true;
            if (aVal == null || bVal == null) return false;

            Console.WriteLine($"🔍 Сравнение: {aVal} ({aVal.GetType()}) == {bVal} ({bVal.GetType()})");

            // Приводим оба значения к double для численного сравнения
            if (IsNumeric(aVal) && IsNumeric(bVal))
            {
                double aNum = Convert.ToDouble(aVal);
                double bNum = Convert.ToDouble(bVal);
                bool result = Math.Abs(aNum - bNum) < 1e-10;
                Console.WriteLine($"🔢 Численное сравнение: {aNum} == {bNum} = {result}");
                return result;
            }

            // Для строкового сравнения
            if (aVal is string aStr && bVal is string bStr)
            {
                bool result = string.Equals(aStr, bStr, StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"📝 Строковое сравнение: '{aStr}' == '{bStr}' = {result}");
                return result;
            }

            // Для смешанных типов (число и строка)
            if (IsNumeric(aVal) && bVal is string bString)
            {
                if (double.TryParse(bString, out double bNum))
                {
                    double aNum = Convert.ToDouble(aVal);
                    bool result = Math.Abs(aNum - bNum) < 1e-10;
                    Console.WriteLine($"🔢📝 Смешанное сравнение: {aNum} == '{bString}' = {result}");
                    return result;
                }
            }

            if (aVal is string aString && IsNumeric(bVal))
            {
                if (double.TryParse(aString, out double aNum))
                {
                    double bNum = Convert.ToDouble(bVal);
                    bool result = Math.Abs(aNum - bNum) < 1e-10;
                    Console.WriteLine($"📝🔢 Смешанное сравнение: '{aString}' == {bNum} = {result}");
                    return result;
                }
            }

            // Стандартное сравнение
            bool defaultResult = aVal.Equals(bVal);
            Console.WriteLine($"⚡ Стандартное сравнение: {aVal} == {bVal} = {defaultResult}");
            return defaultResult;
        }

        private bool IsNumeric(object value)
        {
            return value is int || value is long || value is float ||
                   value is double || value is decimal || value is short ||
                   value is byte || value is sbyte || value is ushort ||
                   value is uint || value is ulong;
        }
    }
}