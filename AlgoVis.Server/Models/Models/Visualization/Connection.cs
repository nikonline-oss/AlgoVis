using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    /// <summary>
    /// Представляет соединение (ребро) между двумя элементами в структуре данных.
    /// Используется для визуализации графов, деревьев и других связанных структур.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Идентификатор элемента, от которого исходит соединение.
        /// </summary>
        public string FromId { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор элемента, к которому направлено соединение.
        /// </summary>
        public string ToId { get; set; } = string.Empty;

        /// <summary>
        /// Тип соединения (например, "directed", "undirected", "parent-child", "sibling").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Вес соединения (для взвешенных графов).
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Флаг, указывающий, выделено ли соединение в текущем шаге визуализации.
        /// </summary>
        public bool IsHighlighted { get; set; }
    }
}