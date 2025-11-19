using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.Algorithms
{
    public interface IAlgorithm<TStructure, TState>
        where TStructure : IDataStructure<TState>
    {
        string Name { get; }
        AlgorithmResult Execute(AlgorithmConfig config, TStructure structure);
    }
}
