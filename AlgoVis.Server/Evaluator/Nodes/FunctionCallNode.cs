using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class FunctionCallNode : IExpressionNode
    {
        private readonly string _functionName;
        private readonly IReadOnlyList<IExpressionNode> _arguments;

        public FunctionCallNode(string functionName, IList<IExpressionNode> arguments)
        {
            _functionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            _arguments = (arguments ?? Array.Empty<IExpressionNode>()).AsReadOnly();
        }

        public IVariableValue Evaluate(IVariableScope variables)
        {
            var argumentValues = _arguments.Select(arg => arg.Evaluate(variables)).ToArray();

            return _functionName.ToLower() switch
            {
                // Математические функции
                "sin" => MathSin(argumentValues),
                "cos" => MathCos(argumentValues),
                "tan" => MathTan(argumentValues),
                "sqrt" => MathSqrt(argumentValues),
                "abs" => MathAbs(argumentValues),
                "min" => MathMin(argumentValues),
                "max" => MathMax(argumentValues),
                "pow" => MathPow(argumentValues),
                "round" => MathRound(argumentValues),
                "floor" => MathFloor(argumentValues),
                "ceil" => MathCeil(argumentValues),
                "log" => MathLog(argumentValues),
                "log10" => MathLog10(argumentValues),
                "exp" => MathExp(argumentValues),

                // Строковые функции
                "length" => StringLength(argumentValues),
                "substring" => Substring(argumentValues),
                "concat" => Concat(argumentValues),
                "toupper" => ToUpper(argumentValues),
                "tolower" => ToLower(argumentValues),
                "trim" => Trim(argumentValues),
                "contains" => Contains(argumentValues),
                "startswith" => StartsWith(argumentValues),
                "endswith" => EndsWith(argumentValues),
                "replace" => Replace(argumentValues),
                "indexof" => IndexOf(argumentValues),
                "lastindexof" => LastIndexOf(argumentValues),
                "split" => Split(argumentValues),

                // Функции для работы с массивами
                "count" => ArrayCount(argumentValues),
                "first" => ArrayFirst(argumentValues),
                "last" => ArrayLast(argumentValues),
                "reverse" => ArrayReverse(argumentValues),
                "sort" => ArraySort(argumentValues),
                "sum" => ArraySum(argumentValues),
                "average" => ArrayAverage(argumentValues),

                // Функции преобразования типов
                "int" => ConvertToInt(argumentValues),
                "double" => ConvertToDouble(argumentValues),
                "string" => ConvertToString(argumentValues),
                "bool" => ConvertToBool(argumentValues),

                // Безопасные строковые функции
                "charat" => SafeCharAt(argumentValues),
                "isvalidindex" => IsValidIndex(argumentValues),
                "isvalidrange" => IsValidRange(argumentValues),

                _ => throw new ArgumentException($"Unknown function: {_functionName}")
            };
        }

        #region Математические функции

        private IVariableValue MathSin(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Sin(args[0].ToDouble()));
        }

        private IVariableValue MathCos(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Cos(args[0].ToDouble()));
        }

        private IVariableValue MathTan(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Tan(args[0].ToDouble()));
        }

        private IVariableValue MathSqrt(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            var value = args[0].ToDouble();
            if (value < 0)
                throw new ArgumentException("Square root of negative number");
            return new DoubleValue(Math.Sqrt(value));
        }

        private IVariableValue MathAbs(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return args[0].Type == VariableType.Int
                ? new IntValue(Math.Abs(args[0].ToInt()))
                : new DoubleValue(Math.Abs(args[0].ToDouble()));
        }

        private IVariableValue MathMin(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            return new DoubleValue(Math.Min(args[0].ToDouble(), args[1].ToDouble()));
        }

        private IVariableValue MathMax(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            return new DoubleValue(Math.Max(args[0].ToDouble(), args[1].ToDouble()));
        }

        private IVariableValue MathPow(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            return new DoubleValue(Math.Pow(args[0].ToDouble(), args[1].ToDouble()));
        }

        private IVariableValue MathRound(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Round(args[0].ToDouble()));
        }

        private IVariableValue MathFloor(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Floor(args[0].ToDouble()));
        }

        private IVariableValue MathCeil(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Ceiling(args[0].ToDouble()));
        }

        private IVariableValue MathLog(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            var value = args[0].ToDouble();
            if (value <= 0)
                throw new ArgumentException("Logarithm of non-positive number");
            return new DoubleValue(Math.Log(value));
        }

        private IVariableValue MathLog10(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            var value = args[0].ToDouble();
            if (value <= 0)
                throw new ArgumentException("Logarithm of non-positive number");
            return new DoubleValue(Math.Log10(value));
        }

        private IVariableValue MathExp(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Exp(args[0].ToDouble()));
        }

        #endregion

        #region Строковые функции

        private IVariableValue StringLength(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new IntValue(args[0].ToString().Length);
        }

        private IVariableValue Substring(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2, 3);
            var str = args[0].ToString();
            var startIndex = args[1].ToInt();

            // Корректируем startIndex, если он выходит за границы
            if (startIndex < 0)
                startIndex = 0;
            if (startIndex >= str.Length)
                return new StringValue(""); // Возвращаем пустую строку, если startIndex за пределами

            if (args.Length == 3)
            {
                var length = args[2].ToInt();
                if (length <= 0)
                    return new StringValue(""); // Возвращаем пустую строку для неположительной длины

                // Корректируем length, если он выходит за границы
                if (startIndex + length > str.Length)
                    length = str.Length - startIndex;

                return new StringValue(str.Substring(startIndex, length));
            }
            else
            {
                // Без length - берем до конца строки
                return new StringValue(str.Substring(startIndex));
            }
        }

        private IVariableValue Concat(IVariableValue[] args)
        {
            if (args.Length == 0)
                return new StringValue("");

            var result = string.Concat(args.Select(arg => arg.ToString()));
            return new StringValue(result);
        }

        private IVariableValue ToUpper(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new StringValue(args[0].ToString().ToUpper());
        }

        private IVariableValue ToLower(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new StringValue(args[0].ToString().ToLower());
        }

        private IVariableValue Trim(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new StringValue(args[0].ToString().Trim());
        }

        private IVariableValue Contains(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            return new BoolValue(str.Contains(substring));
        }

        private IVariableValue StartsWith(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            return new BoolValue(str.StartsWith(substring));
        }

        private IVariableValue EndsWith(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            return new BoolValue(str.EndsWith(substring));
        }

        private IVariableValue Replace(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 3);
            var str = args[0].ToString();
            var oldValue = args[1].ToString();
            var newValue = args[2].ToString();
            return new StringValue(str.Replace(oldValue, newValue));
        }

        private IVariableValue IndexOf(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            var index = str.IndexOf(substring);
            return new IntValue(index >= 0 ? index : -1);
        }

        private IVariableValue LastIndexOf(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            var index = str.LastIndexOf(substring);
            return new IntValue(index >= 0 ? index : -1);
        }

        private IVariableValue Split(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1, 2);
            var str = args[0].ToString();
            var separator = args.Length > 1 ? args[1].ToString() : ",";

            var parts = str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            var array = new ArrayValue(parts.Select(p => new StringValue(p) as IVariableValue));

            return array;
        }

        #endregion

        #region Функции для работы с массивами

        private IVariableValue ArrayCount(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
                return new IntValue(array.Length);
            throw new ArgumentException("Argument must be an array");
        }

        private IVariableValue ArrayFirst(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
                return array.Length > 0 ? array[0] : new IntValue(0);
            throw new ArgumentException("Argument must be an array");
        }

        private IVariableValue ArrayLast(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
                return array.Length > 0 ? array[array.Length - 1] : new IntValue(0);
            throw new ArgumentException("Argument must be an array");
        }

        private IVariableValue ArrayReverse(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
            {
                var items = new List<IVariableValue>();
                for (int i = array.Length - 1; i >= 0; i--)
                {
                    items.Add(array[i]);
                }
                return new ArrayValue(items);
            }
            throw new ArgumentException("Argument must be an array");
        }

        private IVariableValue ArraySort(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
            {
                var items = new List<IVariableValue>();
                for (int i = 0; i < array.Length; i++)
                {
                    items.Add(array[i]);
                }
                items.Sort((a, b) => a.ToDouble().CompareTo(b.ToDouble()));
                return new ArrayValue(items);
            }
            throw new ArgumentException("Argument must be an array");
        }

        private IVariableValue ArraySum(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
            {
                double sum = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    sum += array[i].ToDouble();
                }
                return new DoubleValue(sum);
            }
            throw new ArgumentException("Argument must be an array");
        }

        private IVariableValue ArrayAverage(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
            {
                if (array.Length == 0)
                    return new DoubleValue(0);

                double sum = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    sum += array[i].ToDouble();
                }
                return new DoubleValue(sum / array.Length);
            }
            throw new ArgumentException("Argument must be an array");
        }

        #endregion

        #region Функции преобразования типов

        private IVariableValue ConvertToInt(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new IntValue(args[0].ToInt());
        }

        private IVariableValue ConvertToDouble(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(args[0].ToDouble());
        }

        private IVariableValue ConvertToString(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new StringValue(args[0].ToString());
        }

        private IVariableValue ConvertToBool(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new BoolValue(args[0].ToBool());
        }

        #endregion

        #region Вспомогательные методы

        private void ValidateArgumentCount(IVariableValue[] args, int expectedCount)
        {
            if (args.Length != expectedCount)
                throw new ArgumentException($"Function '{_functionName}' requires {expectedCount} arguments, but got {args.Length}");
        }

        private void ValidateArgumentCount(IVariableValue[] args, int minCount, int maxCount)
        {
            if (args.Length < minCount || args.Length > maxCount)
                throw new ArgumentException($"Function '{_functionName}' requires between {minCount} and {maxCount} arguments, but got {args.Length}");
        }

        // Безопасное получение символа по индексу (возвращает пустую строку при выходе за границы)
        private IVariableValue SafeCharAt(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var index = args[1].ToInt();

            if (index < 0 || index >= str.Length)
                return new StringValue("");

            return new StringValue(str[index].ToString());
        }

        // Безопасная проверка выхода за границы
        private IVariableValue IsValidIndex(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var index = args[1].ToInt();

            return new BoolValue(index >= 0 && index < str.Length);
        }

        // Безопасная проверка диапазона
        private IVariableValue IsValidRange(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 3);
            var str = args[0].ToString();
            var startIndex = args[1].ToInt();
            var length = args[2].ToInt();

            return new BoolValue(startIndex >= 0 &&
                                length >= 0 &&
                                startIndex + length <= str.Length);
        }

        #endregion

        public override string ToString() =>
            $"{_functionName}({string.Join(", ", _arguments)})";
    }
}
