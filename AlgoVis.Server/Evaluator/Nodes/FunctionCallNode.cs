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
    /// <summary>
    /// Представляет узел AST для вызова функции в выражении.
    /// </summary>
    /// <remarks>
    /// Этот класс реализует интерфейс <see cref="IExpressionNode"/> и отвечает за 
    /// обработку вызовов функций с различными аргументами. Поддерживаются математические,
    /// строковые, массивные функции и функции преобразования типов.
    /// </remarks>
    /// <seealso cref="IExpressionNode"/>
    public class FunctionCallNode : IExpressionNode
    {
        private readonly string _functionName;
        private readonly IReadOnlyList<IExpressionNode> _arguments;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FunctionCallNode"/>.
        /// </summary>
        /// <param name="functionName">Название вызываемой функции (регистронезависимое).</param>
        /// <param name="arguments">Список аргументов функции.</param>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, когда <paramref name="functionName"/> равен <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Если <paramref name="arguments"/> равен <c>null</c>, используется пустой список.
        /// </remarks>
        public FunctionCallNode(string functionName, IList<IExpressionNode> arguments)
        {
            _functionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            _arguments = (arguments ?? Array.Empty<IExpressionNode>()).AsReadOnly();
        }

        /// <summary>
        /// Вычисляет значение вызова функции.
        /// </summary>
        /// <param name="variables">Область видимости переменных для вычисления аргументов.</param>
        /// <returns>Результат вычисления функции в виде <see cref="IVariableValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда функция с именем <see cref="_functionName"/> не найдена.
        /// </exception>
        /// <remarks>
        /// Метод вычисляет все аргументы, затем вызывает соответствующую функцию на основе имени.
        /// Имя функции обрабатывается в нижнем регистре (регистронезависимое сопоставление).
        /// </remarks>
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

        /// <summary>
        /// Вычисляет синус угла в радианах.
        /// </summary>
        /// <param name="args">Массив аргументов: [угол в радианах].</param>
        /// <returns>Синус угла в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue MathSin(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Sin(args[0].ToDouble()));
        }

        /// <summary>
        /// Вычисляет косинус угла в радианах.
        /// </summary>
        /// <param name="args">Массив аргументов: [угол в радианах].</param>
        /// <returns>Косинус угла в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue MathCos(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Cos(args[0].ToDouble()));
        }

        /// <summary>
        /// Вычисляет тангенс угла в радианах.
        /// </summary>
        /// <param name="args">Массив аргументов: [угол в радианах].</param>
        /// <returns>Тангенс угла в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue MathTan(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Tan(args[0].ToDouble()));
        }

        /// <summary>
        /// Вычисляет квадратный корень числа.
        /// </summary>
        /// <param name="args">Массив аргументов: [число].</param>
        /// <returns>Квадратный корень в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или число отрицательное.
        /// </exception>
        private IVariableValue MathSqrt(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            var value = args[0].ToDouble();
            if (value < 0)
                throw new ArgumentException("Square root of negative number");
            return new DoubleValue(Math.Sqrt(value));
        }

        /// <summary>
        /// Вычисляет абсолютное значение числа.
        /// </summary>
        /// <param name="args">Массив аргументов: [число].</param>
        /// <returns>Абсолютное значение в виде <see cref="IntValue"/> или <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        /// <remarks>
        /// Возвращает <see cref="IntValue"/>, если исходное значение целое, иначе <see cref="DoubleValue"/>.
        /// </remarks>
        private IVariableValue MathAbs(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return args[0].Type == VariableType.Int
                ? new IntValue(Math.Abs(args[0].ToInt()))
                : new DoubleValue(Math.Abs(args[0].ToDouble()));
        }

        /// <summary>
        /// Находит минимальное из двух чисел.
        /// </summary>
        /// <param name="args">Массив аргументов: [число1, число2].</param>
        /// <returns>Минимальное значение в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue MathMin(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            return new DoubleValue(Math.Min(args[0].ToDouble(), args[1].ToDouble()));
        }

        /// <summary>
        /// Находит максимальное из двух чисел.
        /// </summary>
        /// <param name="args">Массив аргументов: [число1, число2].</param>
        /// <returns>Максимальное значение в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue MathMax(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            return new DoubleValue(Math.Max(args[0].ToDouble(), args[1].ToDouble()));
        }

        /// <summary>
        /// Возводит число в степень.
        /// </summary>
        /// <param name="args">Массив аргументов: [основание, показатель степени].</param>
        /// <returns>Результат возведения в степень в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue MathPow(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            return new DoubleValue(Math.Pow(args[0].ToDouble(), args[1].ToDouble()));
        }

        /// <summary>
        /// Округляет число до ближайшего целого.
        /// </summary>
        /// <param name="args">Массив аргументов: [число].</param>
        /// <returns>Округленное значение в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue MathRound(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Round(args[0].ToDouble()));
        }

        /// <summary>
        /// Округляет число вниз до ближайшего целого.
        /// </summary>
        /// <param name="args">Массив аргументов: [число].</param>
        /// <returns>Округленное вниз значение в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue MathFloor(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Floor(args[0].ToDouble()));
        }

        /// <summary>
        /// Округляет число вверх до ближайшего целого.
        /// </summary>
        /// <param name="args">Массив аргументов: [число].</param>
        /// <returns>Округленное вверх значение в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue MathCeil(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Ceiling(args[0].ToDouble()));
        }

        /// <summary>
        /// Вычисляет натуральный логарифм (логарифм по основанию e).
        /// </summary>
        /// <param name="args">Массив аргументов: [число].</param>
        /// <returns>Натуральный логарифм в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или число неположительное.
        /// </exception>
        private IVariableValue MathLog(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            var value = args[0].ToDouble();
            if (value <= 0)
                throw new ArgumentException("Logarithm of non-positive number");
            return new DoubleValue(Math.Log(value));
        }

        /// <summary>
        /// Вычисляет десятичный логарифм (логарифм по основанию 10).
        /// </summary>
        /// <param name="args">Массив аргументов: [число].</param>
        /// <returns>Десятичный логарифм в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или число неположительное.
        /// </exception>
        private IVariableValue MathLog10(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            var value = args[0].ToDouble();
            if (value <= 0)
                throw new ArgumentException("Logarithm of non-positive number");
            return new DoubleValue(Math.Log10(value));
        }

        /// <summary>
        /// Вычисляет экспоненту (e в степени x).
        /// </summary>
        /// <param name="args">Массив аргументов: [показатель степени].</param>
        /// <returns>Экспоненту в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue MathExp(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(Math.Exp(args[0].ToDouble()));
        }

        #endregion

        #region Строковые функции

        /// <summary>
        /// Возвращает длину строки.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка].</param>
        /// <returns>Длину строки в виде <see cref="IntValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue StringLength(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new IntValue(args[0].ToString().Length);
        }

        /// <summary>
        /// Извлекает подстроку из строки.
        /// </summary>
        /// <param name="args">
        /// Массив аргументов: [строка, начальный индекс] или [строка, начальный индекс, длина].
        /// </param>
        /// <returns>Извлеченную подстроку в виде <see cref="StringValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не в диапазоне [2, 3].
        /// </exception>
        /// <remarks>
        /// Если начальный индекс выходит за границы, возвращается пустая строка.
        /// Если длина не указана, извлекается подстрока до конца строки.
        /// Если длина неположительная, возвращается пустая строка.
        /// </remarks>
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

        /// <summary>
        /// Объединяет строковые представления всех аргументов.
        /// </summary>
        /// <param name="args">Массив аргументов для конкатенации.</param>
        /// <returns>Результат конкатенации в виде <see cref="StringValue"/>.</returns>
        /// <remarks>
        /// Если аргументов нет, возвращается пустая строка.
        /// </remarks>
        private IVariableValue Concat(IVariableValue[] args)
        {
            if (args.Length == 0)
                return new StringValue("");

            var result = string.Concat(args.Select(arg => arg.ToString()));
            return new StringValue(result);
        }

        /// <summary>
        /// Преобразует строку в верхний регистр.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка].</param>
        /// <returns>Строку в верхнем регистре в виде <see cref="StringValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue ToUpper(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new StringValue(args[0].ToString().ToUpper());
        }

        /// <summary>
        /// Преобразует строку в нижний регистр.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка].</param>
        /// <returns>Строку в нижнем регистре в виде <see cref="StringValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue ToLower(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new StringValue(args[0].ToString().ToLower());
        }

        /// <summary>
        /// Удаляет начальные и конечные пробельные символы из строки.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка].</param>
        /// <returns>Обрезанную строку в виде <see cref="StringValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue Trim(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new StringValue(args[0].ToString().Trim());
        }

        /// <summary>
        /// Проверяет, содержит ли строка указанную подстроку.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, подстрока].</param>
        /// <returns><c>true</c>, если строка содержит подстроку; иначе <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue Contains(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            return new BoolValue(str.Contains(substring));
        }

        /// <summary>
        /// Проверяет, начинается ли строка с указанной подстроки.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, подстрока].</param>
        /// <returns><c>true</c>, если строка начинается с подстроки; иначе <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue StartsWith(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            return new BoolValue(str.StartsWith(substring));
        }

        /// <summary>
        /// Проверяет, заканчивается ли строка указанной подстроки.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, подстрока].</param>
        /// <returns><c>true</c>, если строка заканчивается подстрокой; иначе <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue EndsWith(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            return new BoolValue(str.EndsWith(substring));
        }

        /// <summary>
        /// Заменяет все вхождения подстроки в строке на новую подстроку.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, старая подстрока, новая подстрока].</param>
        /// <returns>Строку с заменой в виде <see cref="StringValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 3.</exception>
        private IVariableValue Replace(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 3);
            var str = args[0].ToString();
            var oldValue = args[1].ToString();
            var newValue = args[2].ToString();
            return new StringValue(str.Replace(oldValue, newValue));
        }

        /// <summary>
        /// Находит индекс первого вхождения подстроки в строке.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, подстрока].</param>
        /// <returns>Индекс первого вхождения или -1, если подстрока не найдена.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue IndexOf(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            var index = str.IndexOf(substring);
            return new IntValue(index >= 0 ? index : -1);
        }

        /// <summary>
        /// Находит индекс последнего вхождения подстроки в строке.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, подстрока].</param>
        /// <returns>Индекс последнего вхождения или -1, если подстрока не найдена.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue LastIndexOf(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var substring = args[1].ToString();
            var index = str.LastIndexOf(substring);
            return new IntValue(index >= 0 ? index : -1);
        }

        /// <summary>
        /// Разделяет строку на части по указанному разделителю.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка] или [строка, разделитель].</param>
        /// <returns>Массив строк в виде <see cref="ArrayValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не в диапазоне [1, 2].
        /// </exception>
        /// <remarks>
        /// Если разделитель не указан, используется запятая.
        /// Пустые элементы удаляются.
        /// </remarks>
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

        /// <summary>
        /// Возвращает количество элементов в массиве.
        /// </summary>
        /// <param name="args">Массив аргументов: [массив].</param>
        /// <returns>Количество элементов в виде <see cref="IntValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или аргумент не является массивом.
        /// </exception>
        private IVariableValue ArrayCount(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
                return new IntValue(array.Length);
            throw new ArgumentException("Argument must be an array");
        }

        /// <summary>
        /// Возвращает первый элемент массива.
        /// </summary>
        /// <param name="args">Массив аргументов: [массив].</param>
        /// <returns>Первый элемент массива или 0, если массив пуст.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или аргумент не является массивом.
        /// </exception>
        private IVariableValue ArrayFirst(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
                return array.Length > 0 ? array[0] : new IntValue(0);
            throw new ArgumentException("Argument must be an array");
        }

        /// <summary>
        /// Возвращает последний элемент массива.
        /// </summary>
        /// <param name="args">Массив аргументов: [массив].</param>
        /// <returns>Последний элемент массива или 0, если массив пуст.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или аргумент не является массивом.
        /// </exception>
        private IVariableValue ArrayLast(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            if (args[0] is ArrayValue array)
                return array.Length > 0 ? array[array.Length - 1] : new IntValue(0);
            throw new ArgumentException("Argument must be an array");
        }

        /// <summary>
        /// Возвращает массив с элементами в обратном порядке.
        /// </summary>
        /// <param name="args">Массив аргументов: [массив].</param>
        /// <returns>Новый массив с элементами в обратном порядке.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или аргумент не является массивом.
        /// </exception>
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

        /// <summary>
        /// Возвращает отсортированный массив по возрастанию.
        /// </summary>
        /// <param name="args">Массив аргументов: [массив].</param>
        /// <returns>Новый отсортированный массив.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или аргумент не является массивом.
        /// </exception>
        /// <remarks>
        /// Сортировка выполняется по числовому значению элементов.
        /// </remarks>
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

        /// <summary>
        /// Вычисляет сумму элементов массива.
        /// </summary>
        /// <param name="args">Массив аргументов: [массив].</param>
        /// <returns>Сумму элементов в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или аргумент не является массивом.
        /// </exception>
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

        /// <summary>
        /// Вычисляет среднее арифметическое элементов массива.
        /// </summary>
        /// <param name="args">Массив аргументов: [массив].</param>
        /// <returns>Среднее арифметическое в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно 1 или аргумент не является массивом.
        /// </exception>
        /// <remarks>
        /// Возвращает 0, если массив пуст.
        /// </remarks>
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

        /// <summary>
        /// Преобразует значение в целое число.
        /// </summary>
        /// <param name="args">Массив аргументов: [значение].</param>
        /// <returns>Целое число в виде <see cref="IntValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue ConvertToInt(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new IntValue(args[0].ToInt());
        }

        /// <summary>
        /// Преобразует значение в число с плавающей запятой.
        /// </summary>
        /// <param name="args">Массив аргументов: [значение].</param>
        /// <returns>Число с плавающей запятой в виде <see cref="DoubleValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue ConvertToDouble(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new DoubleValue(args[0].ToDouble());
        }

        /// <summary>
        /// Преобразует значение в строку.
        /// </summary>
        /// <param name="args">Массив аргументов: [значение].</param>
        /// <returns>Строковое представление в виде <see cref="StringValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue ConvertToString(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new StringValue(args[0].ToString());
        }

        /// <summary>
        /// Преобразует значение в логическое.
        /// </summary>
        /// <param name="args">Массив аргументов: [значение].</param>
        /// <returns>Логическое значение в виде <see cref="BoolValue"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 1.</exception>
        private IVariableValue ConvertToBool(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 1);
            return new BoolValue(args[0].ToBool());
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Проверяет, что количество аргументов соответствует ожидаемому.
        /// </summary>
        /// <param name="args">Массив аргументов для проверки.</param>
        /// <param name="expectedCount">Ожидаемое количество аргументов.</param>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не равно <paramref name="expectedCount"/>.
        /// </exception>
        private void ValidateArgumentCount(IVariableValue[] args, int expectedCount)
        {
            if (args.Length != expectedCount)
                throw new ArgumentException($"Function '{_functionName}' requires {expectedCount} arguments, but got {args.Length}");
        }

        /// <summary>
        /// Проверяет, что количество аргументов находится в указанном диапазоне.
        /// </summary>
        /// <param name="args">Массив аргументов для проверки.</param>
        /// <param name="minCount">Минимальное допустимое количество аргументов.</param>
        /// <param name="maxCount">Максимальное допустимое количество аргументов.</param>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, когда количество аргументов не в диапазоне [<paramref name="minCount"/>, <paramref name="maxCount"/>].
        /// </exception>
        private void ValidateArgumentCount(IVariableValue[] args, int minCount, int maxCount)
        {
            if (args.Length < minCount || args.Length > maxCount)
                throw new ArgumentException($"Function '{_functionName}' requires between {minCount} and {maxCount} arguments, but got {args.Length}");
        }

        /// <summary>
        /// Безопасно возвращает символ строки по индексу.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, индекс].</param>
        /// <returns>
        /// Символ в виде строки или пустую строку, если индекс выходит за границы.
        /// </returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue SafeCharAt(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var index = args[1].ToInt();

            if (index < 0 || index >= str.Length)
                return new StringValue("");

            return new StringValue(str[index].ToString());
        }

        /// <summary>
        /// Проверяет, является ли индекс допустимым для строки.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, индекс].</param>
        /// <returns>
        /// <c>true</c>, если индекс находится в пределах строки; иначе <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 2.</exception>
        private IVariableValue IsValidIndex(IVariableValue[] args)
        {
            ValidateArgumentCount(args, 2);
            var str = args[0].ToString();
            var index = args[1].ToInt();

            return new BoolValue(index >= 0 && index < str.Length);
        }

        /// <summary>
        /// Проверяет, является ли диапазон допустимым для строки.
        /// </summary>
        /// <param name="args">Массив аргументов: [строка, начальный индекс, длина].</param>
        /// <returns>
        /// <c>true</c>, если диапазон полностью находится в пределах строки; иначе <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда количество аргументов не равно 3.</exception>
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

        /// <summary>
        /// Возвращает строковое представление вызова функции.
        /// </summary>
        /// <returns>Строка в формате "имя_функции(аргумент1, аргумент2, ...)".</returns>
        public override string ToString() =>
            $"{_functionName}({string.Join(", ", _arguments)})";
    }
}