using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Visualization;

namespace testing.Models.Visualization
{
    public class VisualizationStep
    {
        public int stepNumber { get; set; }
        public string operation { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Dictionary<string, object> metadata { get; set; } = new();
        public VisualizationData visualizationData { get; set; } = new();
    }
}
