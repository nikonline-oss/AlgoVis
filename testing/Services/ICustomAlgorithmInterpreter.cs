using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Custom;
using testing.Models.DataStructures;

namespace testing.Services
{
    public interface ICustomAlgorithmInterpreter
    {
        CustomAlgorithmResult Execute(CustomAlgorithmRequest request, IDataStructure structure);
    }
}
