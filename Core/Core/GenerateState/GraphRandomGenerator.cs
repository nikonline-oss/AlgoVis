using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.Suport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    public class GraphRandomGenerator : RandomGeneratorBase<GraphStructure>
    {
        public GraphRandomGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public GraphRandomGenerator()
        {
        }

        public override string StructureType => "graph";

        public override GraphStructure Generate(Dictionary<string, object> parameters)
        {
            var nodeCount = GetParameterValue(parameters, "nodeCount", 8);
            var graphType = GetParameterValue(parameters, "type", "random");
            var minWeight = GetParameterValue(parameters, "minWeight", 1);
            var maxWeight = GetParameterValue(parameters, "maxWeight", 10);
            var directed = GetParameterValue(parameters, "directed", 0);

            return graphType.ToLower() switch
            {
                "circular" => GenerateCircularGraph(nodeCount, minWeight, maxWeight, directed),
                "grid" => GenerateGridGraph(nodeCount, minWeight, maxWeight, directed),
                "complete" => GenerateCompleteGraph(nodeCount, minWeight, maxWeight, directed),
                "random" => GenerateRandomGraph(nodeCount, minWeight, maxWeight, directed),
                _ => GenerateRandomGraph(nodeCount, minWeight, maxWeight, directed)
            };
        }

        private GraphStructure GenerateCircularGraph(int nodeCount, int minWeight, int maxWeight, int directed)
        {
            var structure = new GraphStructure();
            const int centerX = 400;
            const int centerY = 250;
            const int radius = 150;

            // Create nodes in circular layout
            for (int i = 0; i < nodeCount; i++)
            {
                var angle = (i * 2 * Math.PI) / nodeCount;
                structure.Nodes.Add(new GraphNode
                {
                    Id = i.ToString(),
                    Value = i,
                    X = centerX + radius * Math.Cos(angle),
                    Y = centerY + radius * Math.Sin(angle)
                });
            }

            // Create circular connections
            for (int i = 0; i < nodeCount; i++)
            {
                var next = (i + 1) % nodeCount;
                structure.Edges.Add(new GraphEdge
                {
                    FromId = i.ToString(),
                    ToId = next.ToString(),
                    Weight = _random.Next(minWeight, maxWeight + 1)
                });
            }

            return structure;
        }

        private GraphStructure GenerateGridGraph(int nodeCount, int minWeight, int maxWeight, int directed)
        {
            var structure = new GraphStructure();
            var cols = (int)Math.Ceiling(Math.Sqrt(nodeCount));
            var rows = (int)Math.Ceiling((double)nodeCount / cols);
            var cellWidth = 250.0 / Math.Max(cols - 1, 1);
            var cellHeight = 250.0 / Math.Max(rows - 1, 1);

            // Create nodes in grid layout
            for (int i = 0; i < nodeCount; i++)
            {
                var row = i / cols;
                var col = i % cols;
                structure.Nodes.Add(new GraphNode
                {
                    Id = i.ToString(),
                    Value = i,
                    X = 200 + col * cellWidth,
                    Y = 150 + row * cellHeight
                });
            }

            // Create horizontal connections
            for (int i = 0; i < nodeCount; i++)
            {
                var row = i / cols;
                var col = i % cols;

                if (col < cols - 1 && i + 1 < nodeCount)
                {
                    structure.Edges.Add(new GraphEdge
                    {
                        FromId = i.ToString(),
                        ToId = (i + 1).ToString(),
                        Weight = _random.Next(minWeight, maxWeight + 1)
                    });
                }

                // Create vertical connections
                if (row < rows - 1 && i + cols < nodeCount)
                {
                    structure.Edges.Add(new GraphEdge
                    {
                        FromId = i.ToString(),
                        ToId = (i + cols).ToString(),
                        Weight = _random.Next(minWeight, maxWeight + 1)
                    });
                }
            }

            return structure;
        }

        private GraphStructure GenerateCompleteGraph(int nodeCount, int minWeight, int maxWeight, int directed)
        {
            var structure = new GraphStructure();
            const int centerX = 400;
            const int centerY = 250;
            const int radius = 150;

            // Create nodes in circular layout
            for (int i = 0; i < nodeCount; i++)
            {
                var angle = (i * 2 * Math.PI) / nodeCount;
                structure.Nodes.Add(new GraphNode
                {
                    Id = i.ToString(),
                    Value = i,
                    X = centerX + radius * Math.Cos(angle),
                    Y = centerY + radius * Math.Sin(angle)
                });
            }

            // Create complete connections (all nodes connected to all)
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = i + 1; j < nodeCount; j++)
                {
                    structure.Edges.Add(new GraphEdge
                    {
                        FromId = i.ToString(),
                        ToId = j.ToString(),
                        Weight = _random.Next(minWeight, maxWeight + 1)
                    });

                    if (directed == 1)
                    {
                        structure.Edges.Add(new GraphEdge
                        {
                            FromId = j.ToString(),
                            ToId = i.ToString(),
                            Weight = _random.Next(minWeight, maxWeight + 1)
                        });
                    }
                }
            }

            return structure;
        }

        private GraphStructure GenerateRandomGraph(int nodeCount, int minWeight, int maxWeight, int directed)
        {
            var structure = new GraphStructure();
            const int padding = 60;
            const double minDistance = 70;

            // Create nodes with collision detection
            for (int i = 0; i < nodeCount; i++)
            {
                GraphNode node;
                int attempts = 0;

                do
                {
                    var x = padding + _random.NextDouble() * (800 - 2 * padding);
                    var y = padding + _random.NextDouble() * (500 - 2 * padding);

                    node = new GraphNode
                    {
                        Id = i.ToString(),
                        Value = i,
                        X = x,
                        Y = y
                    };

                    attempts++;

                    if (attempts > 50)
                    {
                        node.X = 200 + _random.NextDouble() * 400;
                        node.Y = 150 + _random.NextDouble() * 200;
                    }

                    if (attempts > 100) break;

                } while (HasCollision(node, structure.Nodes, minDistance));

                structure.Nodes.Add(node);
            }

            // Apply force-directed layout for better positioning
            ApplyForceDirectedLayout(structure.Nodes, 30, 80);

            // Create minimum spanning tree for basic connectivity
            CreateMinimumSpanningTree(structure);

            // Add some additional short edges
            AddShortEdges(structure, minWeight, maxWeight, 120, directed);

            return structure;
        }

        private bool HasCollision(GraphNode newNode, List<GraphNode> existingNodes, double minDistance)
        {
            return existingNodes.Any(existing =>
            {
                var dx = newNode.X - existing.X;
                var dy = newNode.Y - existing.Y;
                return Math.Sqrt(dx * dx + dy * dy) < minDistance;
            });
        }

        private void ApplyForceDirectedLayout(List<GraphNode> nodes, int iterations, double repulsionForce)
        {
            const int padding = 60;

            for (int iter = 0; iter < iterations; iter++)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    double forceX = 0, forceY = 0;

                    // Repulsion from other nodes
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        if (i != j)
                        {
                            var dx = nodes[i].X - nodes[j].X;
                            var dy = nodes[i].Y - nodes[j].Y;
                            var distance = Math.Sqrt(dx * dx + dy * dy);

                            if (distance > 0 && distance < 150)
                            {
                                var force = repulsionForce / distance;
                                forceX += (dx / distance) * force;
                                forceY += (dy / distance) * force;
                            }
                        }
                    }

                    // Attraction to center
                    const double centerX = 400;
                    const double centerY = 250;
                    var toCenterX = centerX - nodes[i].X;
                    var toCenterY = centerY - nodes[i].Y;
                    var toCenterDist = Math.Sqrt(toCenterX * toCenterX + toCenterY * toCenterY);

                    if (toCenterDist > 200)
                    {
                        forceX += toCenterX * 0.1;
                        forceY += toCenterY * 0.1;
                    }

                    // Apply forces with limits
                    var forceMagnitude = Math.Sqrt(forceX * forceX + forceY * forceY);
                    if (forceMagnitude > 15)
                    {
                        forceX = (forceX / forceMagnitude) * 15;
                        forceY = (forceY / forceMagnitude) * 15;
                    }

                    nodes[i].X += forceX;
                    nodes[i].Y += forceY;

                    // Constrain to boundaries
                    nodes[i].X = Math.Max(padding, Math.Min(800 - padding, nodes[i].X));
                    nodes[i].Y = Math.Max(padding, Math.Min(500 - padding, nodes[i].Y));
                }
            }
        }

        private void CreateMinimumSpanningTree(GraphStructure structure)
        {
            var nodeCount = structure.Nodes.Count;
            var addedEdges = new HashSet<string>();
            var connectedNodes = new HashSet<string> { "0" };

            while (connectedNodes.Count < nodeCount)
            {
                GraphEdge bestEdge = null;
                double bestDistance = double.MaxValue;

                foreach (var connectedId in connectedNodes)
                {
                    var connectedNode = structure.Nodes.First(n => n.Id == connectedId);

                    foreach (var candidateNode in structure.Nodes)
                    {
                        if (!connectedNodes.Contains(candidateNode.Id))
                        {
                            var dx = connectedNode.X - candidateNode.X;
                            var dy = connectedNode.Y - candidateNode.Y;
                            var distance = Math.Sqrt(dx * dx + dy * dy);

                            if (distance < bestDistance)
                            {
                                bestDistance = distance;
                                bestEdge = new GraphEdge
                                {
                                    FromId = connectedId,
                                    ToId = candidateNode.Id,
                                    Weight = (int)(distance / 25) + 1
                                };
                            }
                        }
                    }
                }

                if (bestEdge != null)
                {
                    var edgeKey = $"{Math.Min(int.Parse(bestEdge.FromId), int.Parse(bestEdge.ToId))}-{Math.Max(int.Parse(bestEdge.FromId), int.Parse(bestEdge.ToId))}";
                    addedEdges.Add(edgeKey);
                    structure.Edges.Add(bestEdge);
                    connectedNodes.Add(bestEdge.ToId);
                }
                else
                {
                    break;
                }
            }
        }

        private void AddShortEdges(GraphStructure structure, int minWeight, int maxWeight, double maxDistance, int directed)
        {
            var nodeCount = structure.Nodes.Count;
            var addedEdges = new HashSet<string>();

            // Mark existing edges
            foreach (var edge in structure.Edges)
            {
                var edgeKey = $"{Math.Min(int.Parse(edge.FromId), int.Parse(edge.ToId))}-{Math.Max(int.Parse(edge.FromId), int.Parse(edge.ToId))}";
                addedEdges.Add(edgeKey);
            }

            var shortEdges = new List<(string from, string to, double distance)>();

            // Find potential short edges
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = i + 1; j < nodeCount; j++)
                {
                    var edgeKey = $"{i}-{j}";
                    if (!addedEdges.Contains(edgeKey))
                    {
                        var node1 = structure.Nodes[i];
                        var node2 = structure.Nodes[j];
                        var dx = node1.X - node2.X;
                        var dy = node1.Y - node2.Y;
                        var distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance < maxDistance)
                        {
                            shortEdges.Add((i.ToString(), j.ToString(), distance));
                        }
                    }
                }
            }

            // Sort by distance and add shortest edges
            shortEdges = shortEdges.OrderBy(e => e.distance).ToList();
            var maxExtraEdges = Math.Min(shortEdges.Count, Math.Max(2, (int)(nodeCount * 0.4)));

            for (int i = 0; i < maxExtraEdges; i++)
            {
                var edge = shortEdges[i];
                structure.Edges.Add(new GraphEdge
                {
                    FromId = edge.from,
                    ToId = edge.to,
                    Weight = _random.Next(minWeight, maxWeight + 1)
                });

                if (directed == 1)
                {
                    structure.Edges.Add(new GraphEdge
                    {
                        FromId = edge.to,
                        ToId = edge.from,
                        Weight = _random.Next(minWeight, maxWeight + 1)
                    });
                }
            }
        }

        public override Dictionary<string, object> GetDefaultParameters()
        {
            return new Dictionary<string, object>
            {
                { "nodeCount", 8 },
                { "type", "random" },
                { "minWeight", 1 },
                { "maxWeight", 10 },
                { "directed", 0 }
            };
        }
    }
}
