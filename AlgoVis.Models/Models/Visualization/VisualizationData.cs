using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    public class VisualizationData
    {
        public string structureType { get; set; } = string.Empty;
        public Dictionary<string, object> elements { get; set; } = new();
        public List<HighlightedElement> highlights { get; set; } = new();
        public List<Connection> connections { get; set; } = new();
    }
}
