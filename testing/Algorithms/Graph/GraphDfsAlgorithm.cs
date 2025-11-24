using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Algorithms.Core;
using testing.Models.Core;
using testing.Models.DataStructures;
using testing.Models.Visualization;
using testing.Support;

namespace testing.Algorithms.Graph
{
    public class GraphDfsAlgorithm : BaseAlgorithm<GraphStructure, GraphState>
    {
        public override string Name => "GraphDFS";

        protected override void ExecuteAlgorithm(AlgorithmConfig config, GraphStructure structure)
        {
            var startNodeId = config.Parameters?.ContainsKey("StartNodeId") == true
                ? (string)config.Parameters["StartNodeId"]
                : structure.Nodes.First().Id;

            var visited = new HashSet<string>();
            var stack = new Stack<string>();

            stack.Push(startNodeId);

            while (stack.Count > 0)
            {
                var currentNodeId = stack.Pop();

                if (visited.Contains(currentNodeId)) continue;

                visited.Add(currentNodeId);
                RecordMemoryOperation();

                AddStep("visit", $"Посещение узла {currentNodeId}", structure,
                    highlights: new List<HighlightedElement>
                    {
                        new() { ElementId = currentNodeId, HighlightType = "current", Color = "blue", Label = "Current" }
                    });

                var neighbors = structure.Edges
                    .Where(e => e.FromId == currentNodeId)
                    .Select(e => e.ToId)
                    .Where(id => !visited.Contains(id));

                foreach (var neighborId in neighbors.Reverse())
                {
                    stack.Push(neighborId);

                    AddStep("explore", $"Исследование ребра к {neighborId}", structure,
                        highlights: new List<HighlightedElement>
                        {
                            new() { ElementId = neighborId, HighlightType = "neighbor", Color = "orange" }
                        },
                        connections: new List<Connection>
                        {
                            new() { FromId = currentNodeId, ToId = neighborId, IsHighlighted = true }
                        });
                }
            }
        }

        protected override Dictionary<string, object> GetOutputData(GraphStructure structure)
        {
            return new Dictionary<string, object>
            {
                ["traversal_order"] = Steps.Where(s => s.operation == "visit").Select(s => s.description).ToList(),
                ["visited_count"] = Steps.Count(s => s.operation == "visit")
            };
        }
    }
}
