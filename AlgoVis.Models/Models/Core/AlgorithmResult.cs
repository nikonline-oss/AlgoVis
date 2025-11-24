using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgoVis.Models.Models.Visualization;

namespace AlgoVis.Models.Models.Core
{
    public class AlgorithmResult
    {
        public string AlgorithmName { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string StructureType { get; set; } = string.Empty;
        public List<VisualizationStep> Steps { get; set; } = new();
        public AlgorithmStatistics Statistics { get; set; } = new();
        public TimeSpan ExecutionTime { get; set; }
        public Dictionary<string, object> OutputData { get; set; } = new();
    }
}
