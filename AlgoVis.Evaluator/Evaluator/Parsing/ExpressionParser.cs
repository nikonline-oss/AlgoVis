using AlgoVis.Evaluator.Evaluator.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
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
            IExpressionNode node;

            switch (token.Type)
            {
                case TokenType.Number:
                    _position++;
                    return new NumberNode(double.Parse(token.Value, CultureInfo.InvariantCulture));

                // ДОБАВЛЕНО: Обработка строковых литералов
                case TokenType.String:
                    _position++;
                    return new StringNode(token.Value);

                case TokenType.Variable:
                    _position++;
                    node = new VariableNode(token.Value);
                    return ParseMemberAccess(node);

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

        private IExpressionNode ParseMemberAccess(IExpressionNode leftNode)
        {
            IExpressionNode node = leftNode;

            while (_position < _tokens.Count)
            {
                var current = CurrentToken();

                // Обработка доступа к свойству: obj.property
                if (current.Type == TokenType.Dot)
                {
                    _position++; // Пропускаем точку

                    var propertyToken = CurrentToken();
                    if (propertyToken.Type != TokenType.Variable)
                        throw new ArgumentException($"Ожидался идентификатор после точки в позиции {propertyToken.Position}");

                    _position++;
                    node = new MemberAccessNode(node, propertyToken.Value);
                }
                // Обработка доступа к массиву: array[index]
                else if (current.Type == TokenType.LeftBracket)
                {
                    _position++; // Пропускаем '['

                    var indexExpression = ParseExpression();

                    Expect(TokenType.RightBracket);

                    node = new ArrayAccessNode(node, indexExpression);
                }
                else
                {
                    break;
                }
            }

            return node;
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