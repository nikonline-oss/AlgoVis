using AlgoVis.Models.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext  = AlgoVis.Models.Models.DataStructures.ExecutionContext;

namespace AlgoVis.Models.Models.Operations.Interfaces
{
    public interface IOperationHandler
    {
        void Execute(AlgorithmStep step, ExecutionContext context);
    }
}
