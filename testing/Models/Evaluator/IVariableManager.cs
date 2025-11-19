using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator
{
    public interface IVariableManager
    {
        IVariableScope CreateScope();
        IVariableScope CreateChildScope(IVariableScope parent);
    }
}
