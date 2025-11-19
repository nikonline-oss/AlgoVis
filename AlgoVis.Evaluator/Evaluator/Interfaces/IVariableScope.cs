using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Interfaces
{
    public interface IVariableScope
    {
        IVariableValue Get(string name);
        void Set(string name, IVariableValue value);
        bool Contains(string name);

        IVariableValue GetNested(string path);
        void SetNested(string path, IVariableValue value);

        // Для обратной совместимости со старым кодом (постепенно уберем)
        Dictionary<string, object> GetAllVariables();
    }

}
