using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{

    public class GraphState
    {
        public List<GraphNode> Nodes { get; set; } = new();
        public List<GraphEdge> Edges { get; set; } = new();
        public string graphType { get; set; } = string.Empty;
    }
}
//interface GraphVisualizationProps
//{
//    nodes: GraphNode[];
//  edges: GraphEdge[];
//  highlightedNodes?: number[];
//  highlightedEdges?: Array<[number, number]>;
//  visitedNodes?: number[];
//  currentNode?: number;
//  directed?: boolean;
//  width?: number;
//  height?: number;
//  graphType?: 'circular' | 'grid' | 'complete' | 'random';
//}