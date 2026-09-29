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
    /// <summary>
    /// Представляет структуру данных "Бинарное дерево" для визуализации алгоритмов.
    /// Реализует интерфейс IDataStructure<TreeNode>.
    /// </summary>
    public class BinaryTreeStructure : IDataStructure<TreeNode>
    {
        /// <summary>Тип структуры данных (константа "binarytree")</summary>
        public string Type => "binarytree";

        /// <summary>Уникальный идентификатор экземпляра дерева</summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>Корневой узел бинарного дерева</summary>
        public TreeNode Root { get; set; }

        private readonly TreeNode _originRoot;

        /// <summary>
        /// Инициализирует новый экземпляр класса BinaryTreeStructure с указанным корнем.
        /// </summary>
        /// <param name="root">Корневой узел дерева.</param>
        public BinaryTreeStructure(TreeNode root)
        {
            Root = root;
            _originRoot = CloneTree(root);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса BinaryTreeStructure с пустым деревом.
        /// </summary>
        public BinaryTreeStructure()
        {
        }

        /// <summary>
        /// Получает текущее состояние дерева (глубокая копия).
        /// </summary>
        /// <returns>Копия текущего дерева.</returns>
        public TreeNode GetState() => CloneTree(Root);

        /// <summary>
        /// Применяет новое состояние дерева.
        /// </summary>
        /// <param name="state">Новое состояние дерева.</param>
        public void ApplyState(TreeNode state) => Root = CloneTree(state);

        /// <summary>
        /// Преобразует текущее состояние дерева в данные для визуализации.
        /// </summary>
        /// <returns>Объект VisualizationData с элементами дерева и связями.</returns>
        public VisualizationData ToVisualizationData()
        {
            var data = new VisualizationData { structureType = "binarytree" };
            BuildVisualizationData(Root, data, null);
            return data;
        }

        /// <summary>
        /// Рекурсивно строит данные визуализации для узла дерева и его потомков.
        /// </summary>
        /// <param name="node">Текущий узел.</param>
        /// <param name="data">Объект VisualizationData для заполнения.</param>
        /// <param name="parentId">Идентификатор родительского узла.</param>
        private void BuildVisualizationData(TreeNode node, VisualizationData data, string parentId)
        {
            if (node == null) return;

            data.elements[node.Id] = new
            {
                value = node.Value,
                label = $"Node: {node.Value}"
            };

            if (parentId != null)
            {
                data.connections.Add(new Connection
                {
                    FromId = parentId,
                    ToId = node.Id,
                    Type = "parent"
                });
            }

            if (node.Left != null)
            {
                data.connections.Add(new Connection
                {
                    FromId = node.Id,
                    ToId = node.Left.Id,
                    Type = "left"
                });
                BuildVisualizationData(node.Left, data, node.Id);
            }

            if (node.Right != null)
            {
                data.connections.Add(new Connection
                {
                    FromId = node.Id,
                    ToId = node.Right.Id,
                    Type = "right"
                });
                BuildVisualizationData(node.Right, data, node.Id);
            }
        }

        /// <summary>
        /// Рекурсивно создает глубокую копию дерева.
        /// </summary>
        /// <param name="root">Корневой узел копируемого дерева.</param>
        /// <returns>Копия дерева или null, если root равен null.</returns>
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

        /// <summary>
        /// Получает исходное состояние дерева (состояние при создании).
        /// </summary>
        /// <returns>Копия исходного дерева.</returns>
        public TreeNode GetOriginState() => CloneTree(_originRoot ?? Root);
    }
}