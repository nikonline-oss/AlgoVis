using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.Interfaces
{
    public interface ICustomAlgorithmInterpreter
    {
        CustomAlgorithmResult Execute(CustomAlgorithmRequest request, IDataStructure structure);
    }
}
