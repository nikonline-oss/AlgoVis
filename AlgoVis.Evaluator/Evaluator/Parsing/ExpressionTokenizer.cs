using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
{
    public class ExpressionTokenizer
    {
        private static readonly Dictionary<string, int> OperatorPrecedence = new Dictionary<string, int>
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
            ["!"] = 8  // Унарные операторы
        };

        private static readonly HashSet<string> Functions = new HashSet<string>
        {
            "sin", "cos", "tan", "sqrt", "abs", "min", "max", "pow",
            "length", "substring", "concat", "toupper", "tolower"  // Добавляем строковые функции
        };

        public List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            int position = 0;
            bool expectUnary = true;

            while (position < expression.Length)
            {
                char current = expression[position];

                if (char.IsWhiteSpace(current))
                {
                    position++;
                    continue;
                }

                // ДОБАВЛЕНО: Обработка строковых литералов
                if (current == '"' || current == '\'')
                {
                    tokens.Add(ReadString(expression, ref position, current));
                    expectUnary = false;
                    continue;
                }

                // ИЗМЕНЕНИЕ: Сначала проверяем точку как отдельный токен
                if (current == '.')
                {
                    tokens.Add(new Token(TokenType.Dot, ".", position));
                    position++;
                    expectUnary = true;
                    continue;
                }

                // ИЗМЕНЕНИЕ: Число должно начинаться с цифры, точка в числе обрабатывается в ReadNumber
                if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber(expression, ref position));
                    expectUnary = false;
                }
                else if (char.IsLetter(current) || current == '_')
                {
                    var token = ReadIdentifier(expression, ref position);
                    tokens.Add(token);
                    expectUnary = false;
                }
                else if (current == '(')
                {
                    tokens.Add(new Token(TokenType.LeftParenthesis, "(", position));
                    position++;
                    expectUnary = true;
                }
                else if (current == ')')
                {
                    tokens.Add(new Token(TokenType.RightParenthesis, ")", position));
                    position++;
                    expectUnary = false;
                }
                else if (current == '[')
                {
                    tokens.Add(new Token(TokenType.LeftBracket, "[", position));
                    position++;
                    expectUnary = true;
                }
                else if (current == ']')
                {
                    tokens.Add(new Token(TokenType.RightBracket, "]", position));
                    position++;
                    expectUnary = false;
                }
                else if (current == ',')
                {
                    tokens.Add(new Token(TokenType.Comma, ",", position));
                    position++;
                    expectUnary = true;
                }
                else if (IsOperator(current))
                {
                    var token = ReadOperator(expression, ref position, expectUnary);
                    tokens.Add(token);
                    expectUnary = true;
                }
                else
                {
                    throw new ArgumentException($"Неизвестный символ: '{current}' в позиции {position}");
                }
            }

            return tokens;
        }

        // ДОБАВЛЕНО: Метод для чтения строковых литералов
        private Token ReadString(string expression, ref int position, char quoteChar)
        {
            int start = position;
            position++; // Пропускаем открывающую кавычку

            var sb = new StringBuilder();
            bool escapeNext = false;

            while (position < expression.Length)
            {
                char current = expression[position];

                if (escapeNext)
                {
                    // Обработка escape-последовательностей
                    switch (current)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '\\': sb.Append('\\'); break;
                        case '"': sb.Append('"'); break;
                        case '\'': sb.Append('\''); break;
                        default: sb.Append(current); break;
                    }
                    escapeNext = false;
                    position++;
                }
                else if (current == '\\')
                {
                    // Начало escape-последовательности
                    escapeNext = true;
                    position++;
                }
                else if (current == quoteChar)
                {
                    // Закрывающая кавычка - конец строки
                    position++;
                    return new Token(TokenType.String, sb.ToString(), start);
                }
                else
                {
                    // Обычный символ строки
                    sb.Append(current);
                    position++;
                }
            }

            throw new ArgumentException($"Незакрытая строковая константа, начинающаяся с позиции {start}");
        }

        private Token ReadNumber(string expression, ref int position)
        {
            int start = position;
            bool hasDecimal = false;

            while (position < expression.Length)
            {
                char c = expression[position];
                if (char.IsDigit(c))
                {
                    position++;
                }
                else if (c == '.' && !hasDecimal)
                {
                    hasDecimal = true;
                    position++;

                    // Проверяем, что после точки есть цифра
                    if (position >= expression.Length || !char.IsDigit(expression[position]))
                    {
                        throw new ArgumentException($"Некорректное число: ожидалась цифра после точки в позиции {position}");
                    }
                }
                else
                {
                    break;
                }
            }

            string number = expression.Substring(start, position - start);
            return new Token(TokenType.Number, number, start);
        }

        private Token ReadIdentifier(string expression, ref int position)
        {
            int start = position;

            while (position < expression.Length && (char.IsLetterOrDigit(expression[position]) || expression[position] == '_'))
            {
                position++;
            }

            string identifier = expression.Substring(start, position - start);

            if (Functions.Contains(identifier.ToLower()))
            {
                return new Token(TokenType.Function, identifier, start);
            }
            else
            {
                return new Token(TokenType.Variable, identifier, start);
            }
        }

        private Token ReadOperator(string expression, ref int position, bool expectUnary)
        {
            int start = position;

            // Пробуем прочитать двухсимвольные операторы
            if (position + 1 < expression.Length)
            {
                string twoChar = expression.Substring(position, 2);
                if (twoChar == "||" || twoChar == "&&" || twoChar == "==" || twoChar == "!=" ||
                    twoChar == "<=" || twoChar == ">=")
                {
                    position += 2;

                    // Для логического НЕ в начале выражения
                    if (expectUnary && twoChar == "!=")
                    {
                        return new Token(TokenType.Operator, "u!", start);
                    }

                    return new Token(TokenType.Operator, twoChar, start);
                }
            }

            char opChar = expression[position];
            position++;

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

        private bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' || c == '%' ||
                   c == '^' || c == '<' || c == '>' || c == '=' || c == '!' ||
                   c == '|' || c == '&';
        }

        public static int GetPrecedence(string op)
        {
            return OperatorPrecedence.TryGetValue(op, out int precedence) ? precedence : 0;
        }

        public static bool IsRightAssociative(string op)
        {
            return op == "^" || op.StartsWith("u");
        }
    }
}