using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Core
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
