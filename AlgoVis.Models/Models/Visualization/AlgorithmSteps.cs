using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    // Sorting algorithms
    public class SortingStep : VisualizationStepBase
    {
        public int[] array { get; set; } = Array.Empty<int>();
        public int[]? comparing { get; set; }
        public int[]? swapping { get; set; }
        public int[]? sorted { get; set; }
        public int? pivotIndex { get; set; }
    }

    // Searching algorithms
    public class SearchingStep : VisualizationStepBase
    {
        public int[] array { get; set; } = Array.Empty<int>();
        public int? currentIndex { get; set; }
        public int? targetIndex { get; set; }
        public int? leftBound { get; set; }
        public int? rightBound { get; set; }
        public int? midIndex { get; set; }
    }

    // Graph algorithms
    public class GraphStep : VisualizationStepBase
    {
        public int[][] Graph { get; set; } = Array.Empty<int[]>();
        public int? CurrentNode { get; set; }
        public int[]? VisitedNodes { get; set; }
        public int[]? Path { get; set; }
        public int? FromNode { get; set; }
        public int? ToNode { get; set; }
    }

    // Pathfinding algorithms
    public class PathfindingStep : VisualizationStepBase
    {
        public int[][] Grid { get; set; } = Array.Empty<int[]>();
        public int[]? CurrentPosition { get; set; }
        public int[]? StartPosition { get; set; }
        public int[]? EndPosition { get; set; }
        public int[][]? OpenSet { get; set; }
        public int[][]? ClosedSet { get; set; }
        public int[][]? Path { get; set; }
    }
}
