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

namespace testing.Algorithms.Tree
{
    public class TreeInOrderAlgorithm : BaseAlgorithm<BinaryTreeStructure, TreeNode>
    {
        public override string Name => "TreeInOrder";

        protected override void ExecuteAlgorithm(AlgorithmConfig config, BinaryTreeStructure structure)
        {
            InOrderTraversal(structure.Root, structure);
        }

        private void InOrderTraversal(TreeNode node, BinaryTreeStructure structure)
        {
            if (node == null) return;

            RecordRecursiveCall();

            InOrderTraversal(node.Left, structure);

            AddStep("visit", $"Посещение узла со значением {node.Value}", structure,
                highlights: new List<HighlightedElement>
                {
                    new() { ElementId = node.Id, HighlightType = "current", Color = "blue" }
                });

            InOrderTraversal(node.Right, structure);
        }

        protected override Dictionary<string, object> GetOutputData(BinaryTreeStructure structure)
        {
            var visitedNodes = Steps
                .Where(s => s.Operation == "visit")
                .Select(s => s.Description)
                .ToList();

            return new Dictionary<string, object>
            {
                ["inorder_traversal"] = visitedNodes,
                ["visited_count"] = visitedNodes.Count
            };
        }
    }
}
