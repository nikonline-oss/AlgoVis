using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{
    public class ListNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Value { get; set; }
        public ListNode? Next { get; set; }
        public ListNode? Previous { get; set; }
    }
}
