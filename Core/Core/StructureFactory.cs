using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Suport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core
{
    public static class StructureFactory
    {
        public static IDataStructure CreateStructure(string type, object data)
        {
            return type.ToLower() switch
            {
                "array" => new ArrayStructure((int[])data),
                "linkedlist" => new LinkedListStructure { Head = (ListNode)data },
                "binarytree" => new BinaryTreeStructure { Root = (TreeNode)data },
                "graph" => new GraphStructure
                {
                    Nodes = ((GraphState)data).Nodes,
                    Edges = ((GraphState)data).Edges
                },
                _ => throw new ArgumentException($"Unsupported structure type: {type}")
            };
        }

        // Обобщенная версия для типобезопасного использования
        public static IDataStructure<T> CreateStructure<T>(string type, object data)
        {
            return type.ToLower() switch
            {
                "array" when typeof(T) == typeof(int[]) =>
                    new ArrayStructure((int[])data) as IDataStructure<T>,
                "linkedlist" when typeof(T) == typeof(ListNode) =>
                    new LinkedListStructure { Head = (ListNode)data } as IDataStructure<T>,
                "binarytree" when typeof(T) == typeof(TreeNode) =>
                    new BinaryTreeStructure { Root = (TreeNode)data } as IDataStructure<T>,
                "graph" when typeof(T) == typeof(GraphState) =>
                    new GraphStructure
                    {
                        Nodes = ((GraphState)data).Nodes,
                        Edges = ((GraphState)data).Edges
                    } as IDataStructure<T>,
                _ => throw new ArgumentException($"Unsupported structure type: {type}")
            } ?? throw new InvalidOperationException("Failed to create structure");
        }

    }
}
