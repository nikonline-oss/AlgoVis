using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
{
    public class ParseException : Exception
    {
        public int Position { get; }

        public ParseException(string message, int position) : base(message)
        {
            Position = position;
        }

        public ParseException(string message, int position, Exception innerException)
            : base(message, innerException)
        {
            Position = position;
        }
    }
}
