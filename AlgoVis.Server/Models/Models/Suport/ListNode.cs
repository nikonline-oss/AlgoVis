
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Suport
{
    /// <summary>
    /// Представляет узел связного списка (односвязного или двусвязного).
    /// </summary>
    /// <remarks>
    /// Базовый элемент для реализации структур данных типа LinkedList.
    /// </remarks>
    public class ListNode
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
        /// Ссылка на следующий узел в списке.
        /// </summary>
        /// <value>null, если это последний узел.</value>
        public ListNode? Next { get; set; }

        /// <summary>
        /// Ссылка на предыдущий узел в списке.
        /// </summary>
        /// <value>null, если это первый узел (для двусвязных списков).</value>
        public ListNode? Previous { get; set; }
    }
}
