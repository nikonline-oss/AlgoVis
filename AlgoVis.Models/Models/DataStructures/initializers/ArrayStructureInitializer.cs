using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.DataStructures.initializers
{
    public class ArrayStructureInitializer : IStructureInitializer
    {
        public string StructureType => "Array";
        public Dictionary<string, object> GetStructureProperties(IDataStructure structure)
        {
            var arrayState = structure.GetState() as int[] ?? Array.Empty<int>();
            return new Dictionary<string, object>
            {
                ["length"] = arrayState.Length,
                ["values"] = arrayState,
                ["first"] = arrayState.Length > 0 ? arrayState[0] : 0,
                ["last"] = arrayState.Length > 0 ? arrayState[^1] : 0,
                ["isEmpty"] = arrayState.Length == 0
            };
        }
    }
}
