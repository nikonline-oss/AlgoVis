using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.DataStructures.Interfaces
{
    public interface IStructureInitializer
    {
        string StructureType { get; }
        Dictionary<string, object> GetStructureProperties(IDataStructure structure);
    }
}
