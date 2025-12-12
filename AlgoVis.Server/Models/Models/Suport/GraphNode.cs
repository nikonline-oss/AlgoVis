using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{
    public class GraphNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Value { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string? Label { get; set; }
    }
}
