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
    /// Представляет структуру данных "Граф" для визуализации алгоритмов.
    /// Реализует интерфейс IDataStructure<GraphState>.
    /// </summary>
    public class GraphStructure : IDataStructure<GraphState>
    {
        /// <summary>Тип структуры данных (константа "graph")</summary>
        public string Type => "graph";

        /// <summary>Уникальный идентификатор экземпляра графа</summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>Список узлов графа</summary>
        public List<GraphNode> Nodes { get; set; } = new();

        /// <summary>Список рёбер графа</summary>
        public List<GraphEdge> Edges { get; set; } = new();

        /// <summary>
        /// Получает текущее состояние графа.
        /// </summary>
        /// <returns>Объект GraphState с копиями узлов и рёбер.</returns>
        public GraphState GetState() => new GraphState
        {
            Nodes = Nodes.Select(n => new GraphNode
            {
                Id = n.Id,
                Value = n.Value,
                X = n.X,
                Y = n.Y
            }).ToList(),
            Edges = Edges.Select(e => new GraphEdge
            {
                from = e.from,
                to = e.to,
                weight = e.weight
            }).ToList()
        };

        /// <summary>
        /// Применяет новое состояние графа.
        /// </summary>
        /// <param name="state">Новое состояние графа.</param>
        public void ApplyState(GraphState state)
        {
            Nodes = state.Nodes;
            Edges = state.Edges;
        }

        /// <summary>
        /// Преобразует текущее состояние графа в данные для визуализации.
        /// </summary>
        /// <returns>Объект VisualizationData с элементами графа и связями.</returns>
        public VisualizationData ToVisualizationData()
        {
            var data = new VisualizationData { structureType = "graph" };

            foreach (var node in Nodes)
            {
                data.elements[node.Id] = new
                {
                    value = node.Value,
                    label = $"Node: {node.Value}",
                    x = node.X,
                    y = node.Y
                };
            }

            foreach (var edge in Edges)
            {
                data.connections.Add(new Connection
                {
                    FromId = edge.from,
                    ToId = edge.to,
                    Type = "edge",
                    Weight = edge.weight
                });
            }

            return data;
        }

        /// <summary>
        /// Получает исходное состояние графа.
        /// </summary>
        /// <returns>Исходное состояние графа.</returns>
        /// <exception cref="NotImplementedException">Метод пока не реализован.</exception>
        public GraphState GetOriginState()
        {
            throw new NotImplementedException();
        }
    }
}