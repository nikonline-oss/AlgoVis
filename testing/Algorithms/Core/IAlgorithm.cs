using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Core;
using testing.Models.DataStructures;

namespace testing.Algorithms.Core
{
    public interface IAlgorithm<TStructure, TState>
        where TStructure : IDataStructure<TState>
    {
        string Name { get; }
        AlgorithmResult Execute(AlgorithmConfig config, TStructure structure);
    }
}
