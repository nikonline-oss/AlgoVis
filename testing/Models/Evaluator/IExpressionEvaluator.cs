using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator
{
    public interface IExpressionEvaluator
    {
        object Evaluate(string expression, IVariableScope variables);
        bool EvaluateCondition(string condition, IVariableScope variables);
    }
}
