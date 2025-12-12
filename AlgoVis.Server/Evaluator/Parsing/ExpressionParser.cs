using AlgoVis.Evaluator.Evaluator.Nodes;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
{
    public interface IParser
    {
        IExpressionNode Parse(string expression);
    }

    public class ExpressionParser : IParser
    {
        private readonly ITokenizer _tokenizer;

        public ExpressionParser(ITokenizer tokenizer = null)
        {
            _tokenizer = tokenizer ?? new ExpressionTokenizer();
        }

        public IExpressionNode Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return new ConstantNode(new IntValue(0));

            try
            {
                var tokens = _tokenizer.Tokenize(expression);
                return new ParserCore(tokens).Parse();
            }
            catch (ParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ParseException($"Error parsing expression: {expression}", 0, ex);
            }
        }

        // Внутренний класс для инкапсуляции логики парсинга
        private class ParserCore
        {
            private readonly IReadOnlyList<Token> _tokens;
            private int _position;

            public ParserCore(IReadOnlyList<Token> tokens)
            {
                _tokens = tokens;
                _position = 0;
            }

            public IExpressionNode Parse()
            {
                if (_tokens.Count == 0)
                    return new ConstantNode(new IntValue(0));

                var expression = ParseExpression();

                if (!CurrentToken.IsEndOfExpression)
                    throw new ParseException($"Unexpected token: {CurrentToken}", CurrentToken.Position);

                return expression;
            }

            private IExpressionNode ParseExpression(int precedence = 0)
            {
                var left = ParsePrimary();

                while (true)
                {
                    var current = CurrentToken;
                    if (current.Type != TokenType.Operator) break;

                    var currentPrecedence = ExpressionTokenizer.GetPrecedence(current.Value);
                    if (currentPrecedence < precedence) break;

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
                var token = CurrentToken;

                return token.Type switch
                {
                    TokenType.Number => ParseNumber(),
                    TokenType.String => ParseString(),
                    TokenType.Variable => ParseVariable(),
                    TokenType.Function => ParseFunctionCall(),
                    TokenType.LeftParenthesis => ParseParenthesizedExpression(),
                    TokenType.Operator when token.Value.StartsWith("u") => ParseUnaryOperator(),
                    _ => throw CreateUnexpectedTokenException(token)
                };
            }

            private IExpressionNode ParseNumber()
            {
                var token = ExpectAndConsume(TokenType.Number);
                var value = double.Parse(token.Value, CultureInfo.InvariantCulture);

                // Определяем, целое это число или дробное
                return value % 1 == 0
                    ? new ConstantNode(new IntValue((int)value))
                    : new ConstantNode(new DoubleValue(value));
            }

            private IExpressionNode ParseString()
            {
                var token = ExpectAndConsume(TokenType.String);
                return new ConstantNode(new StringValue(token.Value));
            }

            private IExpressionNode ParseVariable()
            {
                var token = ExpectAndConsume(TokenType.Variable);
                var node = new VariableNode(token.Value);
                return ParseMemberAccess(node);
            }

            private IExpressionNode ParseUnaryOperator()
            {
                var token = ExpectAndConsume(TokenType.Operator);
                var operand = ParsePrimary();

                return token.Value switch
                {
                    "u+" => new UnaryOperationNode(operand, "u+"),
                    "u-" => new UnaryOperationNode(operand, "u-"),
                    "u!" => new UnaryOperationNode(operand, "u!"),
                    _ => throw new ParseException($"Unknown unary operator: {token.Value}", token.Position)
                };
            }

            private IExpressionNode ParseMemberAccess(IExpressionNode leftNode)
            {
                IExpressionNode node = leftNode;

                while (_position < _tokens.Count)
                {
                    var current = CurrentToken;

                    switch (current.Type)
                    {
                        case TokenType.Dot:
                            node = ParsePropertyAccess(node);
                            break;
                        case TokenType.LeftBracket:
                            node = ParseArrayAccess(node);
                            break;
                        default:
                            return node;
                    }
                }

                return node;
            }

            private IExpressionNode ParsePropertyAccess(IExpressionNode target)
            {
                ExpectAndConsume(TokenType.Dot);

                var propertyToken = Expect(TokenType.Variable);
                _position++;

                // Проверяем, является ли это вызовом метода
                if (CurrentToken.Type == TokenType.LeftParenthesis)
                {
                    return ParseMethodCall(target, propertyToken.Value);
                }

                return new MemberAccessNode(target, propertyToken.Value);
            }

            private IExpressionNode ParseArrayAccess(IExpressionNode target)
            {
                ExpectAndConsume(TokenType.LeftBracket);

                var indexExpression = ParseExpression();

                ExpectAndConsume(TokenType.RightBracket);

                return new ArrayAccessNode(target, indexExpression);
            }

            private IExpressionNode ParseMethodCall(IExpressionNode target, string methodName)
            {
                ExpectAndConsume(TokenType.LeftParenthesis);

                var arguments = new List<IExpressionNode>();

                if (CurrentToken.Type != TokenType.RightParenthesis)
                {
                    arguments.Add(ParseExpression());

                    while (CurrentToken.Type == TokenType.Comma)
                    {
                        _position++;
                        arguments.Add(ParseExpression());
                    }
                }

                ExpectAndConsume(TokenType.RightParenthesis);

                return new MethodCallNode(target, methodName, arguments);
            }

            private IExpressionNode ParseFunctionCall()
            {
                var functionToken = ExpectAndConsume(TokenType.Function);
                ExpectAndConsume(TokenType.LeftParenthesis);

                var arguments = new List<IExpressionNode>();

                if (CurrentToken.Type != TokenType.RightParenthesis)
                {
                    arguments.Add(ParseExpression());

                    while (CurrentToken.Type == TokenType.Comma)
                    {
                        _position++;
                        arguments.Add(ParseExpression());
                    }
                }

                ExpectAndConsume(TokenType.RightParenthesis);

                return new FunctionCallNode(functionToken.Value, arguments);
            }

            private IExpressionNode ParseParenthesizedExpression()
            {
                ExpectAndConsume(TokenType.LeftParenthesis);
                var expression = ParseExpression();
                ExpectAndConsume(TokenType.RightParenthesis);
                return expression;
            }

            private Token CurrentToken =>
                _position < _tokens.Count ? _tokens[_position] : Token.EndOfExpression;

            private Token Expect(TokenType expectedType)
            {
                var current = CurrentToken;
                if (current.Type != expectedType)
                    throw CreateUnexpectedTokenException(current, expectedType);

                return current;
            }

            private Token ExpectAndConsume(TokenType expectedType)
            {
                var token = Expect(expectedType);
                _position++;
                return token;
            }

            private ParseException CreateUnexpectedTokenException(Token token, TokenType? expected = null)
            {
                var message = expected.HasValue
                    ? $"Expected {expected}, but got {token.Type} '{token.Value}' at position {token.Position}"
                    : $"Unexpected token: {token.Type} '{token.Value}' at position {token.Position}";

                return new ParseException(message, token.Position);
            }
        }
    }
}