using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Core
{
    public class AlgorithmStatistics
    {
        public int Comparisons { get; set; }
        public int Swaps { get; set; }
        public int Steps { get; set; }
        public int RecursiveCalls { get; set; }
        public int MemoryOperations { get; set; }
        public double TimeComplexity { get; set; }
        public double SpaceComplexity { get; set; }
        public Dictionary<string, double> CustomMetrics { get; set; } = new();

        public void Reset()
        {
            Comparisons = 0;
            Swaps = 0;
            Steps = 0;
            RecursiveCalls = 0;
            MemoryOperations = 0;
            CustomMetrics.Clear();
        }

        public AlgorithmStatistics Clone()
        {
            return new AlgorithmStatistics
            {
                Comparisons = Comparisons,
                Swaps = Swaps,
                Steps = Steps,
                RecursiveCalls = RecursiveCalls,
                MemoryOperations = MemoryOperations,
                TimeComplexity = TimeComplexity,
                SpaceComplexity = SpaceComplexity,
                CustomMetrics = new Dictionary<string, double>(CustomMetrics)
            };
        }
    }
}
