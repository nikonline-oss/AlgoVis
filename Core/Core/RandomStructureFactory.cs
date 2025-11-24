using AlgoVis.Core.Core.GenerateState;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core
{
    public class RandomStructureFactory
    {
        private readonly Dictionary<string, IRandomStructureGenerator> _generators;

        public RandomStructureFactory()
        {
            _generators = new Dictionary<string, IRandomStructureGenerator>
            {
                { "array", new ArrayRandomGenerator() },
                { "graph", new GraphRandomGenerator() },
                { "binarytree", new BinaryTreeRandomGenerator() }
            };
        }

        public void RegisterGenerator(string structureType, IRandomStructureGenerator generator)
        {
            _generators[structureType] = generator;
        }

        public IDataStructure GenerateStructure(string structureType, Dictionary<string, object> parameters = null)
        {
            if (_generators.TryGetValue(structureType.ToLower(), out var generator))
            {
                var actualParameters = parameters ?? generator.GetDefaultParameters();
                return generator.Generate(actualParameters);
            }

            throw new ArgumentException($"No generator found for structure type: {structureType}");
        }

        public Dictionary<string, object> GetDefaultParameters(string structureType)
        {
            if (_generators.TryGetValue(structureType.ToLower(), out var generator))
            {
                return generator.GetDefaultParameters();
            }

            throw new ArgumentException($"No generator found for structure type: {structureType}");
        }

        public List<string> GetAvailableStructures()
        {
            return _generators.Keys.ToList();
        }
    }
}
