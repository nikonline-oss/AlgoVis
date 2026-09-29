
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{
    /// <summary>
    /// Представляет полное состояние графа для алгоритмов визуализации.
    /// </summary>
    /// <remarks>
    /// Этот класс объединяет все узлы и ребра графа, а также указывает тип графа.
    /// Используется для сериализации/десериализации состояния визуализации.
    /// </remarks>
    public class GraphState
    {
        /// <summary>
        /// Список всех узлов в графе.
        /// </summary>
        public List<GraphNode> Nodes { get; set; } = new();

        /// <summary>
        /// Список всех ребер в графе.
        /// </summary>
        public List<GraphEdge> Edges { get; set; } = new();

        /// <summary>
        /// Тип графа (например, "ориентированный", "неориентированный", "взвешенный").
        /// </summary>
        public string graphType { get; set; } = string.Empty;
    }
}