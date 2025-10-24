using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;

namespace testing.Models.Operations
{
    public interface IOperationExecutor
    {
        void Execute(AlgorithmStep step, ExecutionContext context);
    }
}
