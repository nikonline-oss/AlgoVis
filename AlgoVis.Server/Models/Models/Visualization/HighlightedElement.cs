using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    /// <summary>
    /// Представляет элемент, который должен быть выделен в визуализации.
    /// </summary>
    public class HighlightedElement
    {
        /// <summary>
        /// Уникальный идентификатор выделяемого элемента.
        /// </summary>
        public string ElementId { get; set; } = string.Empty;

        /// <summary>
        /// Тип выделения (например, "active", "visited", "path", "selected").
        /// </summary>
        public string HighlightType { get; set; } = string.Empty;

        /// <summary>
        /// Цвет выделения в формате CSS (например, "#FF0000", "red", "rgba(255,0,0,0.5)").
        /// </summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// Текстовая метка для отображения рядом с выделенным элементом.
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }
}