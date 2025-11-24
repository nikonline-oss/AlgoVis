using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;

namespace testing.Models.Functions
{
    public class FunctionStack
    {
        private readonly Stack<FunctionContext> _stack = new Stack<FunctionContext>();

        public int CurrentDepth => _stack.Count;
        public FunctionContext Current => _stack.Count > 0 ? _stack.Peek() : null;

        public void Push(FunctionContext context) => _stack.Push(context);
        public FunctionContext Pop() => _stack.Count > 0 ? _stack.Pop() : null;
        public void Clear() => _stack.Clear();
    }
}
