using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using testing.Models.Evaluator;

namespace testing.Models.Functions
{

    public interface IFunctionManager
    {
        FunctionContext CreateContext(string functionName, string returnStepId, IVariableScope parentScope);
        FunctionGroup GetFunction(string name, CustomAlgorithmRequest request);
    }

}
