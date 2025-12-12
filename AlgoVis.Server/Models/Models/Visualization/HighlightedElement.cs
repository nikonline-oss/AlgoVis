using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    public class HighlightedElement
    {
        public string ElementId { get; set; } = string.Empty;
        public string HighlightType { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
