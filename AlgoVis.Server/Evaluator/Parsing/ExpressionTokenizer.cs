using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
{
    public interface ITokenizer
    {
        IReadOnlyList<Token> Tokenize(string expression);
    }

    public class ExpressionTokenizer : ITokenizer
    {
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

        private static readonly HashSet<string> _builtInFunctions = new()
    {
        "sin", "cos", "tan", "sqrt", "abs", "min", "max", "pow", "round", "floor", "ceil",
        "length", "substring", "concat", "toupper", "tolower", "trim", "contains",
        "count", "first", "last"
    };

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

        private Token ReadOperator(StringReader reader)
        {
            var start = reader.Position;

            // Пробуем прочитать двухсимвольные операторы
            if (reader.RemainingLength >= 2)
            {
                var twoChar = reader.PeekString(2);
                if (twoChar != null && _operatorPrecedence.ContainsKey(twoChar))
                {
                    reader.Advance(2);
                    return new Token(TokenType.Operator, twoChar, start);
                }
            }

            // Односимвольные операторы
            if (reader.HasMore)
            {
                var singleChar = reader.Read().ToString();
                return new Token(TokenType.Operator, singleChar, start);
            }

            throw new ParseException("Unexpected end of input while reading operator", start);
        }

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

        private Token ReadSingleChar(StringReader reader, TokenType type)
        {
            var start = reader.Position;
            var value = reader.Read().ToString();
            return new Token(type, value, start);
        }

        private bool IsOperator(char c)
        {
            return "+-*/%^<>=!&|".Contains(c);
        }

        public static int GetPrecedence(string op)
        {
            return _operatorPrecedence.TryGetValue(op, out int precedence) ? precedence : 0;
        }

        public static bool IsRightAssociative(string op)
        {
            return op == "^" || op.StartsWith("u");
        }
    }

    // Вспомогательный класс для чтения строки
    public class StringReader
    {
        private readonly string _input;
        private int _position;

        public StringReader(string input)
        {
            _input = input ?? "";
            _position = 0;
        }

        public bool HasMore => _position < _input.Length;
        public int Position => _position;
        public int RemainingLength => _input.Length - _position;

        public char Read()
        {
            if (!HasMore)
                throw new InvalidOperationException("No more characters to read");
            return _input[_position++];
        }

        public char Peek()
        {
            if (!HasMore)
                return '\0';
            return _input[_position];
        }

        public string PeekString(int length)
        {
            if (_position + length > _input.Length)
                return null;
            return _input.Substring(_position, length);
        }

        public void Advance(int count)
        {
            _position = Math.Min(_position + count, _input.Length);
        }

        public void SkipWhitespace()
        {
            while (HasMore && char.IsWhiteSpace(Peek()))
                _position++;
        }
    }
}