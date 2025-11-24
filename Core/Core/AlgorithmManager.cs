using AlgoVis.Core.Core.Algorithms.Graph;
using AlgoVis.Core.Core.Algorithms.Sorting;
using AlgoVis.Core.Core.Algorithms.Tree;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Suport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core
{
    public class AlgorithmManager
    {
        private readonly Dictionary<string, Type> _algorithms = new();
        private readonly AlgorithmInterpreter algorithmInterpreter;

        public AlgorithmManager()
        {
            algorithmInterpreter = new AlgorithmInterpreter();

            RegisterAlgorithm<ArrayStructure, int[]>("BubbleSort", typeof(BubbleSortAlgorithm));
            RegisterAlgorithm<ArrayStructure, int[]>("QuickSort", typeof(QuickSortAlgorithm));
            RegisterAlgorithm<GraphStructure, GraphState>("GraphDFS", typeof(GraphDfsAlgorithm));
            RegisterAlgorithm<BinaryTreeStructure, TreeNode>("TreeInOrder", typeof(TreeInOrderAlgorithm));
        }

        public void RegisterAlgorithm<TStructure, TState>(string name, Type algorithmType)
            where TStructure : IDataStructure<TState>
        {
            _algorithms[name] = algorithmType;
        }

        public AlgorithmResult ExecuteAlgorithm(AlgorithmConfig config, IDataStructure structure)
        {
            if (!_algorithms.ContainsKey(config.Name))
                throw new ArgumentException($"Algorithm '{config.Name}' not found");

            var algorithmType = _algorithms[config.Name];
            var algorithmInstance = Activator.CreateInstance(algorithmType);

            // Используем рефлексию для вызова метода Execute
            var executeMethod = algorithmType.GetMethod("Execute");
            return executeMethod?.Invoke(algorithmInstance, new object[] { config, structure }) as AlgorithmResult
                ?? throw new InvalidOperationException("Failed to execute algorithm");
        }
        public CustomAlgorithmResult ExecuteCustomAlgorithm(CustomAlgorithmRequest request, IDataStructure structure)
        {
            return algorithmInterpreter.Execute(request, structure);
        }

        public List<string> GetAvailableAlgorithms() => _algorithms.Keys.ToList();
    }
}
