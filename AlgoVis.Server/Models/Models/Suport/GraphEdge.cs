using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{
    /// <summary>
    /// Представляет ребро графа в алгоритмах визуализации.
    /// </summary>
    /// <remarks>
    /// Этот класс используется для хранения информации о связи между двумя узлами графа,
    /// включая вес соединения и опциональную метку.
    /// </remarks>
    public class GraphEdge
    {
        /// <summary>
        /// Идентификатор или имя начального узла ребра.
        /// </summary>
        public string from { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор или имя конечного узла ребра.
        /// </summary>
        public string to { get; set; } = string.Empty;

        /// <summary>
        /// Вес ребра. Используется во взвешенных графах.
        /// </summary>
        public double weight { get; set; }

        /// <summary>
        /// Опциональная текстовая метка для отображения на ребре.
        /// </summary>
        public string? Label { get; set; }
    }
}