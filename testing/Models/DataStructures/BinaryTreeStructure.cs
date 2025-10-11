using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Visualization;
using testing.Support;

namespace testing.Models.DataStructures
{
    public class BinaryTreeStructure : IDataStructure<TreeNode>
    {
        public string Type => "binarytree";
        public string Id { get; } = Guid.NewGuid().ToString();
        public TreeNode Root { get; set; }

        public TreeNode GetState() => CloneTree(Root);

        public void ApplyState(TreeNode state) => Root = CloneTree(state);

        public VisualizationData ToVisualizationData()
        {
            var data = new VisualizationData { StructureType = "binarytree" };
            BuildVisualizationData(Root, data, null);
            return data;
        }

        private void BuildVisualizationData(TreeNode node, VisualizationData data, string parentId)
        {
            if (node == null) return;

            data.Elements[node.Id] = new
            {
                value = node.Value,
                label = $"Node: {node.Value}"
            };

            if (parentId != null)
            {
                data.Connections.Add(new Connection
                {
                    FromId = parentId,
                    ToId = node.Id,
                    Type = "parent"
                });
            }

            if (node.Left != null)
            {
                data.Connections.Add(new Connection
                {
                    FromId = node.Id,
                    ToId = node.Left.Id,
                    Type = "left"
                });
                BuildVisualizationData(node.Left, data, node.Id);
            }

            if (node.Right != null)
            {
                data.Connections.Add(new Connection
                {
                    FromId = node.Id,
                    ToId = node.Right.Id,
                    Type = "right"
                });
                BuildVisualizationData(node.Right, data, node.Id);
            }
        }

        private TreeNode CloneTree(TreeNode root)
        {
            if (root == null) return null;

            return new TreeNode
            {
                Value = root.Value,
                Left = CloneTree(root.Left),
                Right = CloneTree(root.Right)
            };
        }

    }
}
