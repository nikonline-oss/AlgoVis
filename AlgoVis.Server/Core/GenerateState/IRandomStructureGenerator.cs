using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    public interface IRandomStructureGenerator
    {
        string StructureType { get; }
        IDataStructure Generate(Dictionary<string, object> parameters);
        Dictionary<string, object> GetDefaultParameters();
    }

    public interface IRandomStructureGenerator<T> : IRandomStructureGenerator where T : IDataStructure
    {
        new T Generate(Dictionary<string, object> parameters);
    }
}
