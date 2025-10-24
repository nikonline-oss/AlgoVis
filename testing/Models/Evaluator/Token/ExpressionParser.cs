using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator.Token
{
    public class ExpressionParser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public ExpressionParser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        public IExpressionNode Parse()
        {
            var expression = ParseExpression();
            Expect(TokenType.EndOfExpression);
            return expression;
        }

        private IExpressionNode ParseExpression(int precedence = 0)
        {
            var left = ParsePrimary();

            while (true)
            {
                var current = CurrentToken();
                if (current.Type != TokenType.Operator) break;

                var currentPrecedence = ExpressionTokenizer.GetPrecedence(current.Value);
                if (currentPrecedence < precedence) break;

                // Для правоассоциативных операторов (как ^) уменьшаем precedence
                var nextPrecedence = ExpressionTokenizer.IsRightAssociative(current.Value)
                    ? currentPrecedence
                    : currentPrecedence + 1;

                _position++;
                var right = ParseExpression(nextPrecedence);
                left = new BinaryOperationNode(left, right, current.Value);
            }

            return left;
        }

        private IExpressionNode ParsePrimary()
        {
            var token = CurrentToken();

            switch (token.Type)
            {
                case TokenType.Number:
                    _position++;
                    return new NumberNode(double.Parse(token.Value, CultureInfo.InvariantCulture));

                case TokenType.Variable:
                    _position++;
                    return new VariableNode(token.Value);

                case TokenType.Function:
                    return ParseFunctionCall();

                case TokenType.LeftParenthesis:
                    return ParseParenthesizedExpression();

                case TokenType.Operator when token.Value.StartsWith("u"): // Унарные операторы
                    _position++;
                    var operand = ParsePrimary();
                    return new UnaryOperationNode(operand, token.Value);

                default:
                    throw new ArgumentException($"Неожиданный токен: {token.Type} '{token.Value}' в позиции {token.Position}");
            }
        }

        private IExpressionNode ParseParenthesizedExpression()
        {
            Expect(TokenType.LeftParenthesis);
            var expression = ParseExpression();
            Expect(TokenType.RightParenthesis);
            return expression;
        }

        private IExpressionNode ParseFunctionCall()
        {
            var functionToken = CurrentToken();
            Expect(TokenType.Function);
            Expect(TokenType.LeftParenthesis);

            var arguments = new List<IExpressionNode>();

            // Парсим аргументы функции
            if (CurrentToken().Type != TokenType.RightParenthesis)
            {
                arguments.Add(ParseExpression());

                while (CurrentToken().Type == TokenType.Comma)
                {
                    _position++;
                    arguments.Add(ParseExpression());
                }
            }

            Expect(TokenType.RightParenthesis);
            return new FunctionNode(functionToken.Value, arguments);
        }

        private Token CurrentToken()
        {
            return _position < _tokens.Count ? _tokens[_position] : new Token(TokenType.EndOfExpression, "", -1);
        }

        private void Expect(TokenType expectedType)
        {
            var current = CurrentToken();
            if (current.Type != expectedType)
            {
                throw new ArgumentException($"Ожидался {expectedType}, но получен {current.Type} '{current.Value}' в позиции {current.Position}");
            }
            _position++;
        }
    }
}
