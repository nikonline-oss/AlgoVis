using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{
    public class GraphEdge
    {
        public string from { get; set; } = string.Empty;
        public string to { get; set; } = string.Empty;
        public double weight { get; set; }
        public string? Label { get; set; }
    }
}
