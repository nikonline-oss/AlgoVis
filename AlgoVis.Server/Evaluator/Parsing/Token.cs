using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
{
    /// <summary>
    /// Определяет типы лексических токенов, поддерживаемых токенизатором.
    /// </summary>
    public enum TokenType
    {
        /// <summary>Числовой литерал.</summary>
        Number,

        /// <summary>Строковый литерал.</summary>
        String,

        /// <summary>Идентификатор переменной.</summary>
        Variable,

        /// <summary>Оператор (математический или логический).</summary>
        Operator,

        /// <summary>Идентификатор функции.</summary>
        Function,

        /// <summary>Левая круглая скобка '('.</summary>
        LeftParenthesis,

        /// <summary>Правая круглая скобка ')'.</summary>
        RightParenthesis,

        /// <summary>Левая квадратная скобка '['.</summary>
        LeftBracket,

        /// <summary>Правая квадратная скобка ']'.</summary>
        RightBracket,

        /// <summary>Точка '.'.</summary>
        Dot,

        /// <summary>Запятая ','.</summary>
        Comma,

        /// <summary>Маркер конца выражения.</summary>
        EndOfExpression
    }

    /// <summary>
    /// Представляет лексический токен с типом, значением и позицией в исходной строке.
    /// </summary>
    public readonly struct Token : IEquatable<Token>
    {
        /// <summary>
        /// Получает тип токена.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Получает строковое значение токена.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Получает позицию токена в исходной строке.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Инициализирует новый экземпляр структуры Token.
        /// </summary>
        /// <param name="type">Тип токена.</param>
        /// <param name="value">Значение токена.</param>
        /// <param name="position">Позиция токена в исходной строке.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если value равно null.</exception>
        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Position = position;
        }

        /// <summary>
        /// Получает токен, представляющий конец выражения.
        /// </summary>
        public static Token EndOfExpression => new Token(TokenType.EndOfExpression, "", -1);

        /// <summary>
        /// Определяет, является ли токен маркером конца выражения.
        /// </summary>
        public bool IsEndOfExpression => Type == TokenType.EndOfExpression;

        /// <summary>
        /// Возвращает строковое представление токена.
        /// </summary>
        /// <returns>Строка, представляющая токен.</returns>
        public override string ToString()
        {
            return IsEndOfExpression
                ? "EndOfExpression"
                : $"{Type}('{Value}') at {Position}";
        }

        /// <summary>
        /// Определяет, равен ли текущий токен другому токену.
        /// </summary>
        /// <param name="other">Токен для сравнения.</param>
        /// <returns>true, если токены равны; иначе false.</returns>
        public bool Equals(Token other)
        {
            return Type == other.Type && Value == other.Value && Position == other.Position;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Token other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value, Position);
        }

        /// <summary>
        /// Оператор равенства для токенов.
        /// </summary>
        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Оператор неравенства для токенов.
        /// </summary>
        public static bool operator !=(Token left, Token right)
        {
            return !left.Equals(right);
        }
    }
}