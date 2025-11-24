using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.Suport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    public class BinaryTreeRandomGenerator : RandomGeneratorBase<BinaryTreeStructure>
    {
        public BinaryTreeRandomGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public BinaryTreeRandomGenerator()
        {
        }

        public override string StructureType => "binarytree";

        public override BinaryTreeStructure Generate(Dictionary<string, object> parameters)
        {
            var nodeCount = GetParameterValue(parameters, "nodeCount", 7);
            var minValue = GetParameterValue(parameters, "minValue", 1);
            var maxValue = GetParameterValue(parameters, "maxValue", 100);
            var treeType = GetParameterValue(parameters, "type", "balanced");

            return treeType.ToLower() switch
            {
                "complete" => GenerateCompleteTree(nodeCount, minValue, maxValue),
                "balanced" => GenerateBalancedTree(nodeCount, minValue, maxValue),
                "random" => GenerateRandomTree(nodeCount, minValue, maxValue),
                _ => GenerateBalancedTree(nodeCount, minValue, maxValue)
            };
        }

        private BinaryTreeStructure GenerateCompleteTree(int nodeCount, int minValue, int maxValue)
        {
            if (nodeCount <= 0) return new BinaryTreeStructure();

            var nodes = new List<TreeNode>();

            // Create complete binary tree
            for (int i = 0; i < nodeCount; i++)
            {
                nodes.Add(new TreeNode
                {
                    Value = _random.Next(minValue, maxValue + 1)
                });
            }

            // Build complete tree structure
            for (int i = 0; i < nodeCount; i++)
            {
                var leftIndex = 2 * i + 1;
                var rightIndex = 2 * i + 2;

                if (leftIndex < nodeCount)
                    nodes[i].Left = nodes[leftIndex];

                if (rightIndex < nodeCount)
                    nodes[i].Right = nodes[rightIndex];
            }

            return new BinaryTreeStructure(nodes[0]);
        }

        private BinaryTreeStructure GenerateBalancedTree(int nodeCount, int minValue, int maxValue)
        {
            if (nodeCount <= 0) return new BinaryTreeStructure();
            return new BinaryTreeStructure(BuildBalancedTree(0, nodeCount - 1, minValue, maxValue));
        }

        private TreeNode BuildBalancedTree(int start, int end, int minValue, int maxValue)
        {
            if (start > end) return null;

            int mid = (start + end) / 2;

            var node = new TreeNode
            {
                Value = _random.Next(minValue, maxValue + 1),
                Left = BuildBalancedTree(start, mid - 1, minValue, maxValue),
                Right = BuildBalancedTree(mid + 1, end, minValue, maxValue)
            };

            return node;
        }

        private BinaryTreeStructure GenerateRandomTree(int nodeCount, int minValue, int maxValue)
        {
            if (nodeCount <= 0) return new BinaryTreeStructure();

            var root = new TreeNode { Value = _random.Next(minValue, maxValue + 1) };
            var nodes = new List<TreeNode> { root };

            for (int i = 1; i < nodeCount; i++)
            {
                var newNode = new TreeNode { Value = _random.Next(minValue, maxValue + 1) };
                InsertRandomNode(root, newNode);
                nodes.Add(newNode);
            }

            return new BinaryTreeStructure(root);
        }

        private void InsertRandomNode(TreeNode root, TreeNode newNode)
        {
            while (true)
            {
                if (_random.Next(2) == 0) // Go left
                {
                    if (root.Left == null)
                    {
                        root.Left = newNode;
                        return;
                    }
                    root = root.Left;
                }
                else // Go right
                {
                    if (root.Right == null)
                    {
                        root.Right = newNode;
                        return;
                    }
                    root = root.Right;
                }
            }
        }

        public override Dictionary<string, object> GetDefaultParameters()
        {
            return new Dictionary<string, object>
            {
                { "nodeCount", 7 },
                { "minValue", 1 },
                { "maxValue", 100 },
                { "type", "balanced" }
            };
        }
    }
}
