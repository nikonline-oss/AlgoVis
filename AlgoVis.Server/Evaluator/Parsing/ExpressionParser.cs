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
    /// <summary>
    /// Интерфейс парсера для разбора математических выражений.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Разбирает строковое выражение в AST (Abstract Syntax Tree).
        /// </summary>
        /// <param name="expression">Строковое представление выражения для разбора.</param>
        /// <returns>Корневой узел AST представления выражения.</returns>
        IExpressionNode Parse(string expression);
    }

    /// <summary>
    /// Парсер математических выражений с поддержкой операторов, функций, переменных и доступа к членам.
    /// </summary>
    public class ExpressionParser : IParser
    {
        private readonly ITokenizer _tokenizer;

        /// <summary>
        /// Инициализирует новый экземпляр парсера выражений.
        /// </summary>
        /// <param name="tokenizer">Токенизатор для разбиения выражения на токены. Если не указан, используется ExpressionTokenizer по умолчанию.</param>
        public ExpressionParser(ITokenizer tokenizer = null)
        {
            _tokenizer = tokenizer ?? new ExpressionTokenizer();
        }

        /// <summary>
        /// Разбирает строковое выражение в AST.
        /// </summary>
        /// <param name="expression">Строковое выражение для разбора.</param>
        /// <returns>Корневой узел AST представления выражения.</returns>
        /// <exception cref="ParseException">Выбрасывается при ошибке разбора выражения.</exception>
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

        /// <summary>
        /// Внутренний класс для инкапсуляции логики разбора выражения по алгоритму операторного предшествования.
        /// </summary>
        private class ParserCore
        {
            private readonly IReadOnlyList<Token> _tokens;
            private int _position;

            /// <summary>
            /// Инициализирует новый экземпляр парсера с заданным списком токенов.
            /// </summary>
            /// <param name="tokens">Список токенов для разбора.</param>
            public ParserCore(IReadOnlyList<Token> tokens)
            {
                _tokens = tokens;
                _position = 0;
            }

            /// <summary>
            /// Основной метод разбора выражения из списка токенов.
            /// </summary>
            /// <returns>Корневой узел AST представления выражения.</returns>
            /// <exception cref="ParseException">Выбрасывается при наличии лишних токенов в конце выражения.</exception>
            public IExpressionNode Parse()
            {
                if (_tokens.Count == 0)
                    return new ConstantNode(new IntValue(0));

                var expression = ParseExpression();

                if (!CurrentToken.IsEndOfExpression)
                    throw new ParseException($"Unexpected token: {CurrentToken}", CurrentToken.Position);

                return expression;
            }

            /// <summary>
            /// Разбирает выражение с учетом приоритетов операторов (алгоритм операторного предшествования).
            /// </summary>
            /// <param name="precedence">Минимальный приоритет оператора для разбора.</param>
            /// <returns>Узел AST, представляющий выражение.</returns>
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

            /// <summary>
            /// Разбирает первичные выражения (числа, строки, переменные, функции, скобки, унарные операторы).
            /// </summary>
            /// <returns>Узел AST, представляющий первичное выражение.</returns>
            /// <exception cref="ParseException">Выбрасывается при неожиданном токене.</exception>
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

            /// <summary>
            /// Разбирает числовой литерал.
            /// </summary>
            /// <returns>Узел константы с целым или вещественным значением.</returns>
            private IExpressionNode ParseNumber()
            {
                var token = ExpectAndConsume(TokenType.Number);
                var value = double.Parse(token.Value, CultureInfo.InvariantCulture);

                // Определяем, целое это число или дробное
                return value % 1 == 0
                    ? new ConstantNode(new IntValue((int)value))
                    : new ConstantNode(new DoubleValue(value));
            }

            /// <summary>
            /// Разбирает строковый литерал.
            /// </summary>
            /// <returns>Узел константы со строковым значением.</returns>
            private IExpressionNode ParseString()
            {
                var token = ExpectAndConsume(TokenType.String);
                return new ConstantNode(new StringValue(token.Value));
            }

            /// <summary>
            /// Разбирает переменную с возможным доступом к членам (свойствам, методам, элементам массива).
            /// </summary>
            /// <returns>Узел переменной или доступа к члену.</returns>
            private IExpressionNode ParseVariable()
            {
                var token = ExpectAndConsume(TokenType.Variable);
                var node = new VariableNode(token.Value);
                return ParseMemberAccess(node);
            }

            /// <summary>
            /// Разбирает унарный оператор (унарный плюс, минус или логическое отрицание).
            /// </summary>
            /// <returns>Узел унарной операции.</returns>
            /// <exception cref="ParseException">Выбрасывается при неизвестном унарном операторе.</exception>
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

            /// <summary>
            /// Разбирает цепочку обращений к членам (свойствам, методам, элементам массива).
            /// </summary>
            /// <param name="leftNode">Целевой узел для доступа.</param>
            /// <returns>Узел доступа к члену.</returns>
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

            /// <summary>
            /// Разбирает доступ к свойству или вызов метода.
            /// </summary>
            /// <param name="target">Целевой узел, к члену которого осуществляется доступ.</param>
            /// <returns>Узел доступа к свойству или вызова метода.</returns>
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

            /// <summary>
            /// Разбирает доступ к элементу массива.
            /// </summary>
            /// <param name="target">Целевой узел массива.</param>
            /// <returns>Узел доступа к элементу массива.</returns>
            private IExpressionNode ParseArrayAccess(IExpressionNode target)
            {
                ExpectAndConsume(TokenType.LeftBracket);

                var indexExpression = ParseExpression();

                ExpectAndConsume(TokenType.RightBracket);

                return new ArrayAccessNode(target, indexExpression);
            }

            /// <summary>
            /// Разбирает вызов метода.
            /// </summary>
            /// <param name="target">Целевой узел, метод которого вызывается.</param>
            /// <param name="methodName">Имя вызываемого метода.</param>
            /// <returns>Узел вызова метода.</returns>
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

            /// <summary>
            /// Разбирает вызов функции.
            /// </summary>
            /// <returns>Узел вызова функции.</returns>
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

            /// <summary>
            /// Разбирает выражение в скобках.
            /// </summary>
            /// <returns>Узел выражения в скобках.</returns>
            private IExpressionNode ParseParenthesizedExpression()
            {
                ExpectAndConsume(TokenType.LeftParenthesis);
                var expression = ParseExpression();
                ExpectAndConsume(TokenType.RightParenthesis);
                return expression;
            }

            /// <summary>
            /// Получает текущий токен или токен конца выражения.
            /// </summary>
            private Token CurrentToken =>
                _position < _tokens.Count ? _tokens[_position] : Token.EndOfExpression;

            /// <summary>
            /// Проверяет, что текущий токен имеет ожидаемый тип.
            /// </summary>
            /// <param name="expectedType">Ожидаемый тип токена.</param>
            /// <returns>Текущий токен.</returns>
            /// <exception cref="ParseException">Выбрасывается при несоответствии типа токена.</exception>
            private Token Expect(TokenType expectedType)
            {
                var current = CurrentToken;
                if (current.Type != expectedType)
                    throw CreateUnexpectedTokenException(current, expectedType);

                return current;
            }

            /// <summary>
            /// Проверяет тип текущего токена и перемещает позицию парсера вперед.
            /// </summary>
            /// <param name="expectedType">Ожидаемый тип токена.</param>
            /// <returns>Текущий токен.</returns>
            /// <exception cref="ParseException">Выбрасывается при несоответствии типа токена.</exception>
            private Token ExpectAndConsume(TokenType expectedType)
            {
                var token = Expect(expectedType);
                _position++;
                return token;
            }

            /// <summary>
            /// Создает исключение ParseException для неожиданного токена.
            /// </summary>
            /// <param name="token">Неожиданный токен.</param>
            /// <param name="expected">Ожидаемый тип токена (опционально).</param>
            /// <returns>Исключение ParseException.</returns>
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