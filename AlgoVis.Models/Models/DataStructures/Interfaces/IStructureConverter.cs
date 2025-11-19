using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.DataStructures.Interfaces
{
    public interface IStructureConverter
    {
        IVariableValue ConvertToVariableValue(IDataStructure structure);
        IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType);
        bool CanConvert(string structureType);
    }


}
