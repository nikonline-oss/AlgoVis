using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Visualization;

namespace testing.Models.Visualization
{
    public class VisualizationData
    {
        public string StructureType { get; set; } = string.Empty;
        public Dictionary<string, object> Elements { get; set; } = new();
        public List<HighlightedElement> Highlights { get; set; } = new();
        public List<Connection> Connections { get; set; } = new();
    }
}
