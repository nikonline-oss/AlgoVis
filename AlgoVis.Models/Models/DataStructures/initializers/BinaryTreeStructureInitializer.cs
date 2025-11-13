using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Suport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.DataStructures.initializers
{
    public class BinaryTreeStructureInitializer : IStructureInitializer
    {
        public string StructureType => "BinaryTree";
        public Dictionary<string, object> GetStructureProperties(IDataStructure structure)
        {
            var treeState = structure.GetState() as TreeNode ?? null;
            return new Dictionary<string, object>
            {
                ["value"] = treeState?.Value ?? 0,
                ["hasLeft"] = treeState?.Left != null,
                ["hasRight"] = treeState?.Right != null,
                ["isLeaf"] = treeState?.Left == null && treeState?.Right == null,
                ["height"] = CalculateTreeHeight(treeState),
                ["nodeCount"] = CountTreeNodes(treeState)
            };
        }

        private int CalculateTreeHeight(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(CalculateTreeHeight(node.Left), CalculateTreeHeight(node.Right));
        }

        private int CountTreeNodes(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + CountTreeNodes(node.Left) + CountTreeNodes(node.Right);
        }

    }
}
