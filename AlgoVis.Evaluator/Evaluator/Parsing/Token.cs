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
        Variable,
        Operator,
        Function,
        LeftParenthesis,
        RightParenthesis,
        Comma,
        EndOfExpression,
        LeftBracket,
        RightBracket,
        Dot,
        String
    }
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public override string ToString() => $"{Type}: '{Value}'";
    }
}
