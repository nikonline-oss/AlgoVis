using AlgoVis.Evaluator.Evaluator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Interfaces
{
    public interface IVariableManager
    {
        IVariableScope CreateScope();
        IVariableScope CreateChildScope(VariableScope parent);
    }
}
