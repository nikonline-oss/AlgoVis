using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Functions
{
    public class FunctionContext
    {
        public string FunctionName { get; set; } = string.Empty;
        public string ReturnStepId { get; set; } = string.Empty;
        public IVariableScope Variables { get; set; }
        public int Depth { get; set; }
    }
}
