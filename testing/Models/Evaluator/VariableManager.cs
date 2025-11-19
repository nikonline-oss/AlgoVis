using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator
{
    public class VariableManager : IVariableManager
    {
        public IVariableScope CreateScope()
        {
            return new VariableScope();
        }

        public IVariableScope CreateChildScope(IVariableScope parent)
        {
            return new VariableScope(parent);
        }
    }
}
