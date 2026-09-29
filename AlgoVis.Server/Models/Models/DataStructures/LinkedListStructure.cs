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
    /// Представляет структуру данных "Связный список" для визуализации алгоритмов.
    /// Реализует интерфейс IDataStructure<ListNode>.
    /// </summary>
    public class LinkedListStructure : IDataStructure<ListNode>
    {
        /// <summary>Тип структуры данных (константа "linkedlist")</summary>
        public string Type => "linkedlist";

        /// <summary>Уникальный идентификатор экземпляра списка</summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>Головной элемент связного списка</summary>
        public ListNode Head { get; set; }

        /// <summary>Хвостовой элемент связного списка</summary>
        public ListNode Tail { get; set; }

        /// <summary>
        /// Получает текущее состояние списка (глубокая копия).
        /// </summary>
        /// <returns>Копия текущего списка.</returns>
        public ListNode GetState() => CloneList(Head);

        /// <summary>
        /// Применяет новое состояние списка.
        /// </summary>
        /// <param name="state">Новое состояние списка.</param>
        public void ApplyState(ListNode state) => Head = CloneList(state);

        /// <summary>
        /// Преобразует текущее состояние списка в данные для визуализации.
        /// </summary>
        /// <returns>Объект VisualizationData с элементами списка и связями.</returns>
        public VisualizationData ToVisualizationData()
        {
            var data = new VisualizationData { structureType = "linkedlist" };
            var visited = new HashSet<string>();
            var current = Head;

            while (current != null && !visited.Contains(current.Id))
            {
                data.elements[current.Id] = new
                {
                    value = current.Value,
                    label = $"Node: {current.Value}"
                };

                if (current.Next != null)
                {
                    data.connections.Add(new Connection
                    {
                        FromId = current.Id,
                        ToId = current.Next.Id,
                        Type = "next"
                    });
                }

                visited.Add(current.Id);
                current = current.Next;
            }

            return data;
        }

        /// <summary>
        /// Создает глубокую копию связного списка.
        /// </summary>
        /// <param name="head">Головной элемент копируемого списка.</param>
        /// <returns>Копия списка или null, если head равен null.</returns>
        private ListNode CloneList(ListNode head)
        {
            if (head == null) return null;

            var newHead = new ListNode { Value = head.Value };
            var currentOriginal = head.Next;
            var currentNew = newHead;

            while (currentOriginal != null)
            {
                currentNew.Next = new ListNode { Value = currentOriginal.Value };
                currentNew = currentNew.Next;
                currentOriginal = currentOriginal.Next;
            }

            return newHead;
        }

        /// <summary>
        /// Получает исходное состояние списка.
        /// </summary>
        /// <returns>Исходное состояние списка.</returns>
        /// <exception cref="NotImplementedException">Метод пока не реализован.</exception>
        public ListNode GetOriginState()
        {
            throw new NotImplementedException();
        }
    }
}