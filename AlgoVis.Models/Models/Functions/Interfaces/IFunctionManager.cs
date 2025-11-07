using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Models.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Functions.Interfaces
{

    public interface IFunctionManager
    {
        FunctionContext CreateContext(string functionName, string returnStepId, IVariableScope parentScope);
        FunctionGroup GetFunction(string name, CustomAlgorithmRequest request);
    }

}
