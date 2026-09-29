using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
{
    /// <summary>
    /// Определяет контракт для токенизатора выражений.
    /// </summary>
    /// <remarks>
    /// Реализации этого интерфейса разбивают строковое выражение на последовательность
    /// лексических токенов для последующего синтаксического анализа.
    /// </remarks>
    public interface ITokenizer
    {
        /// <summary>
        /// Преобразует строковое выражение в список токенов.
        /// </summary>
        /// <param name="expression">Выражение для токенизации.</param>
        /// <returns>Доступный только для чтения список токенов.</returns>
        /// <exception cref="ParseException">Выбрасывается при обнаружении синтаксической ошибки.</exception>
        IReadOnlyList<Token> Tokenize(string expression);
    }

    /// <summary>
    /// Реализация токенизатора для математических и логических выражений.
    /// </summary>
    /// <remarks>
    /// Поддерживает числа, строки, переменные, функции, операторы и скобки.
    /// Автоматически определяет унарные операторы на основе контекста.
    /// </remarks>
    public class ExpressionTokenizer : ITokenizer
    {
        /// <summary>
        /// Таблица приоритетов операторов.
        /// </summary>
        /// <remarks>
        /// Более высокое значение означает более высокий приоритет.
        /// Операторы с префиксом "u" обозначают унарные операторы.
        /// </remarks>
        private static readonly Dictionary<string, int> _operatorPrecedence = new()
        {
            ["||"] = 1,
            ["&&"] = 2,
            ["=="] = 3,
            ["!="] = 3,
            ["<"] = 4,
            ["<="] = 4,
            [">"] = 4,
            [">="] = 4,
            ["+"] = 5,
            ["-"] = 5,
            ["*"] = 6,
            ["/"] = 6,
            ["%"] = 6,
            ["^"] = 7,
            ["u+"] = 8,
            ["u-"] = 8,
            ["u!"] = 8
        };

        /// <summary>
        /// Множество встроенных функций, поддерживаемых токенизатором.
        /// </summary>
        private static readonly HashSet<string> _builtInFunctions = new()
        {
            "sin", "cos", "tan", "sqrt", "abs", "min", "max", "pow", "round", "floor", "ceil",
            "length", "substring", "concat", "toupper", "tolower", "trim", "contains",
            "count", "first", "last"
        };

        /// <inheritdoc/>
        public IReadOnlyList<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            var reader = new StringReader(expression);
            bool expectUnary = true; // Флаг для определения унарных операторов

            Token token;
            do
            {
                token = ReadNextToken(reader, ref expectUnary);
                if (token.Type != TokenType.EndOfExpression)
                    tokens.Add(token);
            }
            while (token.Type != TokenType.EndOfExpression);

            return tokens.AsReadOnly();
        }

        /// <summary>
        /// Читает следующий токен из входной строки.
        /// </summary>
        /// <param name="reader">Читатель строки для получения символов.</param>
        /// <param name="expectUnary">Флаг, указывающий, ожидается ли унарный оператор.</param>
        /// <returns>Прочитанный токен.</returns>
        /// <exception cref="ParseException">Выбрасывается при ошибке чтения токена.</exception>
        private Token ReadNextToken(StringReader reader, ref bool expectUnary)
        {
            reader.SkipWhitespace();

            if (!reader.HasMore)
                return Token.EndOfExpression;

            var current = reader.Peek();

            if (current == '\0')
                return Token.EndOfExpression;

            try
            {
                Token token;
                switch (current)
                {
                    case '"':
                    case '\'':
                        token = ReadString(reader);
                        expectUnary = false;
                        break;
                    case '.':
                        token = ReadDot(reader);
                        expectUnary = true;
                        break;
                    case '(':
                        token = ReadSingleChar(reader, TokenType.LeftParenthesis);
                        expectUnary = true;
                        break;
                    case ')':
                        token = ReadSingleChar(reader, TokenType.RightParenthesis);
                        expectUnary = false;
                        break;
                    case '[':
                        token = ReadSingleChar(reader, TokenType.LeftBracket);
                        expectUnary = true;
                        break;
                    case ']':
                        token = ReadSingleChar(reader, TokenType.RightBracket);
                        expectUnary = false;
                        break;
                    case ',':
                        token = ReadSingleChar(reader, TokenType.Comma);
                        expectUnary = true;
                        break;
                    default:
                        if (char.IsDigit(current))
                        {
                            token = ReadNumber(reader);
                            expectUnary = false;
                        }
                        else if (char.IsLetter(current) || current == '_')
                        {
                            token = ReadIdentifier(reader);
                            expectUnary = false;
                        }
                        else if (IsOperator(current))
                        {
                            token = ReadOperator(reader, expectUnary);
                            // После оператора обычно ожидается унарный оператор
                            expectUnary = true;
                        }
                        else
                        {
                            throw new ParseException($"Unexpected character: '{current}'", reader.Position);
                        }
                        break;
                }

                return token;
            }
            catch (Exception ex)
            {
                throw new ParseException($"Error reading token at position {reader.Position}: {ex.Message}", reader.Position, ex);
            }
        }

        /// <summary>
        /// Читает оператор из входной строки.
        /// </summary>
        /// <param name="reader">Читатель строки.</param>
        /// <param name="expectUnary">Определяет, следует ли интерпретировать оператор как унарный.</param>
        /// <returns>Токен оператора.</returns>
        /// <remarks>
        /// Автоматически различает унарные и бинарные операторы на основе контекста.
        /// Унарные операторы получают префикс "u" (например, "u+", "u-", "u!").
        /// </remarks>
        private Token ReadOperator(StringReader reader, bool expectUnary)
        {
            var start = reader.Position;

            // Пробуем прочитать двухсимвольные операторы
            if (reader.RemainingLength >= 2)
            {
                var twoChar = reader.PeekString(2);
                if (twoChar != null && _operatorPrecedence.ContainsKey(twoChar))
                {
                    // Для логического НЕ в унарном контексте
                    if (expectUnary && twoChar == "!=")
                    {
                        // Это унарный !, а не !=
                        reader.Advance(1); // Читаем только '!'
                        return new Token(TokenType.Operator, "u!", start);
                    }

                    reader.Advance(2);
                    return new Token(TokenType.Operator, twoChar, start);
                }
            }

            // Односимвольные операторы
            if (reader.HasMore)
            {
                var opChar = reader.Read();

                // Обработка унарных операторов
                if (expectUnary)
                {
                    return opChar switch
                    {
                        '+' => new Token(TokenType.Operator, "u+", start),
                        '-' => new Token(TokenType.Operator, "u-", start),
                        '!' => new Token(TokenType.Operator, "u!", start),
                        _ => new Token(TokenType.Operator, opChar.ToString(), start)
                    };
                }

                return new Token(TokenType.Operator, opChar.ToString(), start);
            }

            throw new ParseException("Unexpected end of input while reading operator", start);
        }

        /// <summary>
        /// Читает строковый литерал из входной строки.
        /// </summary>
        /// <param name="reader">Читатель строки.</param>
        /// <returns>Токен строкового литерала.</returns>
        /// <remarks>
        /// Поддерживает одинарные (') и двойные (") кавычки.
        /// Обрабатывает escape-последовательности: \n, \t, \r, \\, \", \'.
        /// </remarks>
        private Token ReadString(StringReader reader)
        {
            var start = reader.Position;
            var quoteChar = reader.Read(); // Читаем открывающую кавычку (' или ")
            var sb = new StringBuilder();
            bool escapeNext = false;

            while (reader.HasMore)
            {
                var current = reader.Peek();

                if (current == '\0')
                    break;

                if (escapeNext)
                {
                    reader.Read(); // Пропускаем escape-символ
                    sb.Append(current switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        'r' => '\r',
                        '\\' => '\\',
                        '"' => '"',
                        '\'' => '\'',
                        _ => current
                    });
                    escapeNext = false;
                }
                else if (current == '\\')
                {
                    reader.Read(); // Пропускаем backslash
                    escapeNext = true;
                }
                else if (current == quoteChar)
                {
                    reader.Read(); // Пропускаем закрывающую кавычку
                    return new Token(TokenType.String, sb.ToString(), start);
                }
                else
                {
                    reader.Read(); // Пропускаем обычный символ
                    sb.Append(current);
                }
            }

            throw new ParseException($"Unclosed string literal. Expected closing '{quoteChar}'", start);
        }

        /// <summary>
        /// Читает числовой литерал из входной строки.
        /// </summary>
        /// <param name="reader">Читатель строки.</param>
        /// <returns>Токен числового литерала.</returns>
        /// <remarks>
        /// Поддерживает целые числа и числа с плавающей точкой.
        /// Требует наличия цифры после десятичной точки.
        /// </remarks>
        private Token ReadNumber(StringReader reader)
        {
            var start = reader.Position;
            var sb = new StringBuilder();
            bool hasDecimal = false;

            while (reader.HasMore)
            {
                var current = reader.Peek();
                if (current == '\0') break;

                if (char.IsDigit(current))
                {
                    sb.Append(reader.Read());
                }
                else if (current == '.' && !hasDecimal)
                {
                    hasDecimal = true;
                    sb.Append(reader.Read());

                    // Проверяем, что после точки есть цифра
                    if (!reader.HasMore || !char.IsDigit(reader.Peek()))
                    {
                        throw new ParseException("Expected digit after decimal point", reader.Position);
                    }
                }
                else
                {
                    break;
                }
            }

            var numberStr = sb.ToString();
            if (string.IsNullOrEmpty(numberStr))
                throw new ParseException("Expected number", start);

            return new Token(TokenType.Number, numberStr, start);
        }

        /// <summary>
        /// Читает идентификатор (переменную или функцию) из входной строки.
        /// </summary>
        /// <param name="reader">Читатель строки.</param>
        /// <returns>Токен переменной или функции.</returns>
        /// <remarks>
        /// Идентификаторы могут содержать буквы, цифры и подчеркивания.
        /// Автоматически различает встроенные функции и пользовательские переменные.
        /// </remarks>
        private Token ReadIdentifier(StringReader reader)
        {
            var start = reader.Position;
            var sb = new StringBuilder();

            while (reader.HasMore)
            {
                var current = reader.Peek();
                if (current == '\0') break;

                if (char.IsLetterOrDigit(current) || current == '_')
                {
                    sb.Append(reader.Read());
                }
                else
                {
                    break;
                }
            }

            var identifier = sb.ToString();
            if (string.IsNullOrEmpty(identifier))
                throw new ParseException("Expected identifier", start);

            var type = _builtInFunctions.Contains(identifier.ToLower())
                ? TokenType.Function
                : TokenType.Variable;

            return new Token(type, identifier, start);
        }

        /// <summary>
        /// Читает точку из входной строки.
        /// </summary>
        /// <param name="reader">Читатель строки.</param>
        /// <returns>Токен точки или числовой литерал, начинающийся с точки.</returns>
        /// <remarks>
        /// Если после точки следует цифра, интерпретируется как число (например, ".5").
        /// В противном случае возвращается токен точки.
        /// </remarks>
        private Token ReadDot(StringReader reader)
        {
            var start = reader.Position;
            reader.Read(); // Пропускаем точку

            // Проверяем, является ли точка частью числа (например, .5)
            if (reader.HasMore && char.IsDigit(reader.Peek()))
            {
                // Это число, начинающееся с точки
                var number = "." + ReadNumber(reader).Value;
                return new Token(TokenType.Number, number, start);
            }

            return new Token(TokenType.Dot, ".", start);
        }

        /// <summary>
        /// Читает одиночный символ и создает токен указанного типа.
        /// </summary>
        /// <param name="reader">Читатель строки.</param>
        /// <param name="type">Тип создаваемого токена.</param>
        /// <returns>Токен для одиночного символа.</returns>
        private Token ReadSingleChar(StringReader reader, TokenType type)
        {
            var start = reader.Position;
            var value = reader.Read().ToString();
            return new Token(type, value, start);
        }

        /// <summary>
        /// Проверяет, является ли символ оператором.
        /// </summary>
        /// <param name="c">Проверяемый символ.</param>
        /// <returns>true, если символ является оператором; иначе false.</returns>
        private bool IsOperator(char c)
        {
            return "+-*/%^<>=!&|".Contains(c);
        }

        /// <summary>
        /// Возвращает приоритет оператора.
        /// </summary>
        /// <param name="op">Оператор для проверки.</param>
        /// <returns>Приоритет оператора или 0, если оператор не найден.</returns>
        public static int GetPrecedence(string op)
        {
            return _operatorPrecedence.TryGetValue(op, out int precedence) ? precedence : 0;
        }

        /// <summary>
        /// Определяет, является ли оператор правоассоциативным.
        /// </summary>
        /// <param name="op">Оператор для проверки.</param>
        /// <returns>true, если оператор правоассоциативен; иначе false.</returns>
        /// <remarks>
        /// Возведение в степень (^) и унарные операторы являются правоассоциативными.
        /// </remarks>
        public static bool IsRightAssociative(string op)
        {
            return op == "^" || op.StartsWith("u");
        }
    }

    /// <summary>
    /// Вспомогательный класс для последовательного чтения строки с отслеживанием позиции.
    /// </summary>
    public class StringReader
    {
        private readonly string _input;
        private int _position;

        /// <summary>
        /// Инициализирует новый экземпляр класса StringReader.
        /// </summary>
        /// <param name="input">Входная строка для чтения.</param>
        public StringReader(string input)
        {
            _input = input ?? "";
            _position = 0;
        }

        /// <summary>
        /// Получает значение, указывающее, есть ли в строке еще символы для чтения.
        /// </summary>
        public bool HasMore => _position < _input.Length;

        /// <summary>
        /// Получает текущую позицию чтения в строке.
        /// </summary>
        public int Position => _position;

        /// <summary>
        /// Получает количество оставшихся символов для чтения.
        /// </summary>
        public int RemainingLength => _input.Length - _position;

        /// <summary>
        /// Читает следующий символ из строки.
        /// </summary>
        /// <returns>Прочитанный символ.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при попытке чтения за пределами строки.</exception>
        public char Read()
        {
            if (!HasMore)
                throw new InvalidOperationException("No more characters to read");
            return _input[_position++];
        }

        /// <summary>
        /// Просматривает следующий символ без перемещения позиции чтения.
        /// </summary>
        /// <returns>Следующий символ или '\0', если достигнут конец строки.</returns>
        public char Peek()
        {
            if (!HasMore)
                return '\0';
            return _input[_position];
        }

        /// <summary>
        /// Просматривает строку заданной длины без перемещения позиции чтения.
        /// </summary>
        /// <param name="length">Количество символов для просмотра.</param>
        /// <returns>Строка указанной длины или null, если осталось меньше символов.</returns>
        public string PeekString(int length)
        {
            if (_position + length > _input.Length)
                return null;
            return _input.Substring(_position, length);
        }

        /// <summary>
        /// Перемещает позицию чтения вперед на указанное количество символов.
        /// </summary>
        /// <param name="count">Количество символов для пропуска.</param>
        public void Advance(int count)
        {
            _position = Math.Min(_position + count, _input.Length);
        }

        /// <summary>
        /// Пропускает все пробельные символы на текущей позиции.
        /// </summary>
        public void SkipWhitespace()
        {
            while (HasMore && char.IsWhiteSpace(Peek()))
                _position++;
        }
    }
}