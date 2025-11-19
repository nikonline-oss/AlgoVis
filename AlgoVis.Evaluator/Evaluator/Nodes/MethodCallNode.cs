using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class MethodCallNode : IExpressionNode
    {
        private readonly IExpressionNode _target;
        private readonly string _methodName;
        private readonly IReadOnlyList<IExpressionNode> _arguments;

        public MethodCallNode(IExpressionNode target, string methodName, IList<IExpressionNode> arguments)
        {
            _target = target;
            _methodName = methodName;
            _arguments = (arguments ?? Array.Empty<IExpressionNode>()).AsReadOnly();
        }

        public IVariableValue Evaluate(IVariableScope variables)
        {
            var targetValue = _target.Evaluate(variables);
            var argumentValues = _arguments.Select(arg => arg.Evaluate(variables)).ToArray();

            return targetValue.CallMethod(_methodName, argumentValues);
        }

        public override string ToString() =>
            $"{_target}.{_methodName}({string.Join(", ", _arguments)})";
    }
}
