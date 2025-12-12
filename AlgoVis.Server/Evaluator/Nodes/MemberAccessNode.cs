using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class MemberAccessNode : IExpressionNode
    {
        private readonly IExpressionNode _target;
        private readonly string _memberName;

        public MemberAccessNode(IExpressionNode target, string memberName)
        {
            _target = target;
            _memberName = memberName;
        }

        public IVariableValue Evaluate(IVariableScope variables)
        {
            var targetValue = _target.Evaluate(variables);
            return targetValue.GetProperty(_memberName);
        }

        public override string ToString() => $"{_target}.{_memberName}";
    }
}
