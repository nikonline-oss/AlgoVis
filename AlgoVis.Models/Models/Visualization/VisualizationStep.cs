using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    public interface IStep
    {
        public string description { get; set; }
        public Dictionary<string, object> metadata { get; set; }
        public string operation { get; set; }
        public int stepNumber { get; set; }
    }

    public abstract class VisualizationStepBase : IStep
    {
        public int stepNumber { get; set; }
        public string operation { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Dictionary<string, object> metadata { get; set; } = new();
        public VisualizationData visualizationData { get; set; } = new();
    }


    public class VisualizationStep : IStep
    {
        public int stepNumber { get; set; }
        public string operation { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Dictionary<string, object> metadata { get; set; } = new();
        public VisualizationData visualizationData { get; set; } = new();
    }

    // Generic algorithm result
    public class AlgorithmExecutionResult<TStep> where TStep : IStep
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public List<TStep> steps { get; set; } = new();
        public TimeSpan executionTime { get; set; }
    }
}
