using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Interfaces;
using testing.models;
using testing.models.enums;

namespace testing.Algorithms
{
    internal class QuickSort : IVisualizationAlgorithm
    {
        public string Name => "quicksort";

        public StructureType _structureType => StructureType.array;

        /// <summary>
        /// возможные config.parametrs
        /// [ Detailed, ]
        /// </summary>
        /// <param name="_steps"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public AlgorithmResult Execute(List<SortingStep> _steps, AlgorithmConfig config)
        {
            var array = config.IsArgs
                ? config.Args
                : GenerateRandomArray(config.Length);

            var original = (int[])array.Clone();
            var stats = new AlgorithmStatistics();
            bool detailed = config.Parameters.ContainsKey("Detailed") && (bool)config.Parameters["Detailed"];

            rety
        }

        private static int[] GenerateRandomArray(int length)
        {
            var random = new Random();
            return Enumerable.Range(0, length)
                .Select(_ => random.Next(0, 1000))
                .ToArray();
        }
    }
}
