using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
{
    public enum TokenType
    {
        Number,
        String,
        Variable,
        Operator,
        Function,
        LeftParenthesis,
        RightParenthesis,
        LeftBracket,
        RightBracket,
        Dot,
        Comma,
        EndOfExpression
    }
    public readonly struct Token : IEquatable<Token>
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Position = position;
        }

        public static Token EndOfExpression => new Token(TokenType.EndOfExpression, "", -1);

        public bool IsEndOfExpression => Type == TokenType.EndOfExpression;

        public override string ToString()
        {
            return IsEndOfExpression
                ? "EndOfExpression"
                : $"{Type}('{Value}') at {Position}";
        }

        public bool Equals(Token other)
        {
            return Type == other.Type && Value == other.Value && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            return obj is Token other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value, Position);
        }

        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Token left, Token right)
        {
            return !left.Equals(right);
        }
    }
}
