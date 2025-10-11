using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Visualization;
using testing.Support;

namespace testing.Models.DataStructures
{
    public class GraphStructure : IDataStructure<GraphState>
    {
        public string Type => "graph";
        public string Id { get; } = Guid.NewGuid().ToString();
        public List<GraphNode> Nodes { get; set; } = new();
        public List<GraphEdge> Edges { get; set; } = new();

        public GraphState GetState() => new GraphState
        {
            Nodes = Nodes.Select(n => new GraphNode
            {
                Id = n.Id,
                Value = n.Value,
                X = n.X,
                Y = n.Y
            }).ToList(),
            Edges = Edges.Select(e => new GraphEdge
            {
                FromId = e.FromId,
                ToId = e.ToId,
                Weight = e.Weight
            }).ToList()
        };

        public void ApplyState(GraphState state)
        {
            Nodes = state.Nodes;
            Edges = state.Edges;
        }

        public VisualizationData ToVisualizationData()
        {
            var data = new VisualizationData { StructureType = "graph" };

            foreach (var node in Nodes)
            {
                data.Elements[node.Id] = new
                {
                    value = node.Value,
                    label = $"Node: {node.Value}",
                    x = node.X,
                    y = node.Y
                };
            }

            foreach (var edge in Edges)
            {
                data.Connections.Add(new Connection
                {
                    FromId = edge.FromId,
                    ToId = edge.ToId,
                    Type = "edge",
                    Weight = edge.Weight
                });
            }

            return data;
        }
    }
}
