
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{
    /// <summary>
    /// Представляет узел (вершину) графа в алгоритмах визуализации.
    /// </summary>
    /// <remarks>
    /// Содержит уникальный идентификатор, числовое значение и координаты для визуального представления.
    /// </remarks>
    public class GraphNode
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
        /// X-координата узла в системе координат визуализации.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-координата узла в системе координат визуализации.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Опциональная текстовая метка для отображения на узле.
        /// </summary>
        public string? Label { get; set; }
    }
}
