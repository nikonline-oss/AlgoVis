
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{
    /// <summary>
    /// Представляет узел бинарного дерева.
    /// </summary>
    /// <remarks>
    /// Используется для реализации деревьев (BST, AVL, красно-черных и т.д.)
    /// в алгоритмах визуализации.
    /// </remarks>
    public class TreeNode
    {
        /// <summary>
        /// Уникальный идентификатор узла. Генерируется автоматически при создании.
        /// </summary>
        /// <value>GUID в строковом формате.</value>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Значение, хранящееся в узле.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Ссылка на левого потомка.
        /// </summary>
        /// <value>null, если левого потомка нет.</value>
        public TreeNode? Left { get; set; }

        /// <summary>
        /// Ссылка на правого потомка.
        /// </summary>
        /// <value>null, если правого потомка нет.</value>
        public TreeNode? Right { get; set; }

        /// <summary>
        /// Ссылка на родительский узел.
        /// </summary>
        /// <value>null, если это корневой узел.</value>
        public TreeNode? Parent { get; set; }
    }
}