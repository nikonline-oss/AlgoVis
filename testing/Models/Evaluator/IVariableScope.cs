using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Evaluator
{
    public interface IVariableScope
    {
        object Get(string name);
        void Set(string name, object value);
        bool Contains(string name);
        Dictionary<string, object> GetAllVariables();
        object GetElement(string arrayName, int index);
        void SetElement(string arrayName, int index, object value);
        object GetProperty(string objectName, string propertyName);
        void SetProperty(string objectName, string propertyName, object value);
    }

}
