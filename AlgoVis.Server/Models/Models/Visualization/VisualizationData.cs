using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    /// <summary>
    /// Основной контейнер данных для визуализации алгоритмов.
    /// Содержит информацию о структуре данных, элементах, соединениях и выделениях.
    /// </summary>
    public class VisualizationData
    {
        /// <summary>
        /// Тип визуализируемой структуры данных (например, "array", "linked-list", "tree", "graph").
        /// </summary>
        public string structureType { get; set; } = string.Empty;

        /// <summary>
        /// Словарь элементов структуры данных, где ключ - идентификатор элемента.
        /// </summary>
        public Dictionary<string, object> elements { get; set; } = new();

        /// <summary>
        /// Список элементов, которые должны быть выделены в текущем шаге визуализации.
        /// </summary>
        public List<HighlightedElement> highlights { get; set; } = new();

        /// <summary>
        /// Список соединений между элементами структуры данных.
        /// </summary>
        public List<Connection> connections { get; set; } = new();
    }
}