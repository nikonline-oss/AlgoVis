using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Visualization
{
    public class Connection
    {
        public string FromId { get; set; } = string.Empty;
        public string ToId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Weight { get; set; }
        public bool IsHighlighted { get; set; }
    }
}
