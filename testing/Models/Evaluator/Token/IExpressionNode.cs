using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator.Token
{
    public interface IExpressionNode
    {
        object Evaluate(IVariableScope variables);
    }
}
