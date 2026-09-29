using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    /// <summary>
    /// Представляет шаг визуализации алгоритма сортировки.
    /// Наследует базовый класс <see cref="VisualizationStepBase"/>.
    /// </summary>
    public class SortingStep : VisualizationStepBase
    {
        /// <summary>
        /// Текущее состояние массива на данном шаге сортировки.
        /// </summary>
        public int[] array { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Индексы элементов, которые в данный момент сравниваются.
        /// Может быть null, если на этом шаге сравнения не происходит.
        /// </summary>
        public int[]? comparing { get; set; }

        /// <summary>
        /// Индексы элементов, которые в данный момент меняются местами.
        /// Может быть null, если на этом шаге обмена не происходит.
        /// </summary>
        public int[]? swapping { get; set; }

        /// <summary>
        /// Индексы элементов, которые уже находятся на своих окончательных позициях.
        /// Может быть null, если на этом шаге нет отсортированных элементов.
        /// </summary>
        public int[]? sorted { get; set; }

        /// <summary>
        /// Индекс опорного элемента (пивота) для алгоритмов быстрой сортировки.
        /// Может быть null, если алгоритм не использует опорный элемент.
        /// </summary>
        public int? pivotIndex { get; set; }
    }
}