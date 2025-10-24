using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using testing.Models.Evaluator;

namespace testing.Models.Functions
{
    public class FunctionManager : IFunctionManager
    {
        public FunctionContext CreateContext(string functionName, string returnStepId, IVariableScope parentScope)
        {
            return new FunctionContext
            {
                FunctionName = functionName,
                ReturnStepId = returnStepId,
                Variables = new VariableScope(parentScope),
                Depth = 0 
            };
        }

        public FunctionGroup GetFunction(string name, CustomAlgorithmRequest request)
        {
            return request.functions?.FirstOrDefault(f => f.name == name);
        }
    }
}
