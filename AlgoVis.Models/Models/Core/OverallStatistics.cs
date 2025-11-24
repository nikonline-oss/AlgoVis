using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Core
{
    public class OverallStatistics
    {
        public int TotalAlgorithms { get; set; }
        public int TotalSteps { get; set; }
        public int TotalComparisons { get; set; }
        public int TotalSwaps { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public Dictionary<string, int> AlgorithmDistribution { get; set; } = new();
    }
}
