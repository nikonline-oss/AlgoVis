using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Interfaces;

namespace testing
{
    internal class RegisterAlgo
    {
        public readonly Dictionary<string, IVisualizationAlgorithm> _algorithms = new();
        public void RegisterAlgorithm(IVisualizationAlgorithm algorithm) => _algorithms.Add(algorithm.Name, algorithm);
        public IVisualizationAlgorithm? GetAlgorithm(string name) => _algorithms.GetValueOrDefault(name);
    }
}
