using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator.Token
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
        "sin", "cos", "tan", "sqrt", "abs", "min", "max", "pow"
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

                if (char.IsDigit(current) || current == '.')
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
