using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;


namespace testing.Models.Operations
{
    public class OperationExecutor : IOperationExecutor
    {
        private readonly Dictionary<string, IOperationHandler> _handlers;

        public OperationExecutor( )
        {
            _handlers = new Dictionary<string, IOperationHandler>(StringComparer.OrdinalIgnoreCase)
            {
                ["assign"] = new AssignOperationHandler(),
                ["condition"] = new ConditionOperationHandler(),
                ["compare"] = new CompareOperationHandler(),
                ["swap"] = new SwapOperationHandler(),
                ["call_function"] = new FunctionCallOperationHandler(),
                ["return"] = new ReturnOperationHandler(),
                ["generic"] = new GenericOperationHandler()
            };
        }

        public void Execute(AlgorithmStep step, ExecutionContext context)
        {
            if (_handlers.TryGetValue(step.type, out var handler))
            {
                handler.Execute(step, context);
            }
            else
            {
                throw new InvalidOperationException($"Неизвестный тип операции: {step.type}");
            }
        }
    }
}
