using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;


namespace AlgoVis.Models.Models.Steps.Interfaces
{
    public interface IStepExecutor
    {
        void Execute(string stepId, ExecutionContext context);
    }
}
