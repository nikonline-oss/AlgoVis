using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.Suport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    /// <summary>
    /// Генератор случайных бинарных деревьев.
    /// </summary>
    /// <remarks>
    /// Этот класс генерирует бинарные деревья различных типов: полные, сбалансированные, случайные.
    /// Поддерживает настройку количества узлов и диапазона значений.
    /// </remarks>
    public class BinaryTreeRandomGenerator : RandomGeneratorBase<BinaryTreeStructure>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BinaryTreeRandomGenerator"/> с указанным начальным значением.
        /// </summary>
        /// <param name="seed">Начальное значение для генератора случайных чисел.</param>
        public BinaryTreeRandomGenerator(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BinaryTreeRandomGenerator"/>.
        /// </summary>
        public BinaryTreeRandomGenerator()
        {
        }

        /// <summary>
        /// Получает тип структуры данных - "binarytree".
        /// </summary>
        public override string StructureType => "binarytree";

        /// <summary>
        /// Генерирует бинарное дерево с указанными параметрами.
        /// </summary>
        /// <param name="parameters">Параметры для генерации дерева.</param>
        /// <returns>Сгенерированное бинарное дерево.</returns>
        /// <remarks>
        /// Поддерживаемые типы деревьев: "complete", "balanced", "random".
        /// </remarks>
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

        /// <summary>
        /// Генерирует полное бинарное дерево.
        /// </summary>
        /// <param name="nodeCount">Количество узлов.</param>
        /// <param name="minValue">Минимальное значение узла.</param>
        /// <param name="maxValue">Максимальное значение узла.</param>
        /// <returns>Полное бинарное дерево.</returns>
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

        /// <summary>
        /// Генерирует сбалансированное бинарное дерево.
        /// </summary>
        /// <param name="nodeCount">Количество узлов.</param>
        /// <param name="minValue">Минимальное значение узла.</param>
        /// <param name="maxValue">Максимальное значение узла.</param>
        /// <returns>Сбалансированное бинарное дерево.</returns>
        private BinaryTreeStructure GenerateBalancedTree(int nodeCount, int minValue, int maxValue)
        {
            if (nodeCount <= 0) return new BinaryTreeStructure();
            return new BinaryTreeStructure(BuildBalancedTree(0, nodeCount - 1, minValue, maxValue));
        }

        /// <summary>
        /// Рекурсивно строит сбалансированное дерево.
        /// </summary>
        /// <param name="start">Начальный индекс.</param>
        /// <param name="end">Конечный индекс.</param>
        /// <param name="minValue">Минимальное значение узла.</param>
        /// <param name="maxValue">Максимальное значение узла.</param>
        /// <returns>Корень сбалансированного дерева.</returns>
        private TreeNode BuildBalancedTree(int start, int end, int minValue, int maxValue)
        {
            if (start > end) return null;

            int mid = (start + end) / 2;

            var node = new TreeNode
            {
                Value = _random.Next(minValue, maxValue + 1),
                Left = BuildBalancedTree(mid + 1, end, minValue, maxValue),
                Right = BuildBalancedTree(start, mid - 1, minValue, maxValue)
            };

            return node;
        }

        /// <summary>
        /// Генерирует случайное бинарное дерево.
        /// </summary>
        /// <param name="nodeCount">Количество узлов.</param>
        /// <param name="minValue">Минимальное значение узла.</param>
        /// <param name="maxValue">Максимальное значение узла.</param>
        /// <returns>Случайное бинарное дерево.</returns>
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

        /// <summary>
        /// Вставляет новый узел в случайное место дерева.
        /// </summary>
        /// <param name="root">Корень дерева.</param>
        /// <param name="newNode">Новый узел.</param>
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

        /// <summary>
        /// Получает параметры по умолчанию для генерации бинарного дерева.
        /// </summary>
        /// <returns>Словарь параметров по умолчанию.</returns>
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