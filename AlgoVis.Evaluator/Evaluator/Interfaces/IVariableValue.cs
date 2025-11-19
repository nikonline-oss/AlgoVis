using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Interfaces
{
    public interface IVariableValue
    {
        VariableType Type { get; }
        object RawValue { get; }

        IVariableValue GetProperty(string name);
        void SetProperty(string name, IVariableValue value);

        IVariableValue CallMethod(string methodName, IVariableValue[] args);

        bool HasProperty(string name);
        bool HasMethod(string methodName);

        int ToInt();
        double ToDouble();
        bool ToBool();
        string ToValueString();
    }
}
