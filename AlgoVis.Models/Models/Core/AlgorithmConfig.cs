using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Core
{
    public class AlgorithmConfig
    {
        public string Name { get; set; } = string.Empty;
        public int Length { get; set; } = 10;
        public bool IsArgs {  get; set; } = false;
        public int[] Args {  get; set; } = Array.Empty<int>();
        public string SessionId { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
