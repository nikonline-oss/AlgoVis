using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgoVis.Models.Models.Suport;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Visualization;

namespace AlgoVis.Models.Models.DataStructures
{
    public class LinkedListStructure : IDataStructure<ListNode>
    {
        public string Type => "linkedlist";
        public string Id { get; } = Guid.NewGuid().ToString();
        public ListNode Head { get; set; }
        public ListNode Tail { get; set; }

        public ListNode GetState() => CloneList(Head);

        public void ApplyState(ListNode state) => Head = CloneList(state);

        public VisualizationData ToVisualizationData()
        {
            var data = new VisualizationData { structureType = "linkedlist" };
            var visited = new HashSet<string>();
            var current = Head;

            while (current != null && !visited.Contains(current.Id))
            {
                data.elements[current.Id] = new
                {
                    value = current.Value,
                    label = $"Node: {current.Value}"
                };

                if (current.Next != null)
                {
                    data.connections.Add(new Connection
                    {
                        FromId = current.Id,
                        ToId = current.Next.Id,
                        Type = "next"
                    });
                }

                visited.Add(current.Id);
                current = current.Next;
            }

            return data;
        }

        private ListNode CloneList(ListNode head)
        {
            if (head == null) return null;

            var newHead = new ListNode { Value = head.Value };
            var currentOriginal = head.Next;
            var currentNew = newHead;

            while (currentOriginal != null)
            {
                currentNew.Next = new ListNode { Value = currentOriginal.Value };
                currentNew = currentNew.Next;
                currentOriginal = currentOriginal.Next;
            }

            return newHead;
        }

        public ListNode GetOriginState()
        {
            throw new NotImplementedException();
        }
    }
}
