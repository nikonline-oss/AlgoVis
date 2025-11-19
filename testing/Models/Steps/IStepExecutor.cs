using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;


namespace testing.Models.Steps
{
    public interface IStepExecutor
    {
        void Execute(string stepId, ExecutionContext context);
    }
}
