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
        public int StepNumber { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public VisualizationData VisualizationData { get; set; } = new();
    }
}
