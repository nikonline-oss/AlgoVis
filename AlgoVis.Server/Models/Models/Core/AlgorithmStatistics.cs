
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Core
{
    /// <summary>
    /// Содержит статистические данные о выполнении алгоритма.
    /// </summary>
    /// <remarks>
    /// Используется для отслеживания метрик производительности, сложности и других характеристик алгоритма.
    /// </remarks>
    public class AlgorithmStatistics
    {
        /// <summary>
        /// Количество сравнений, выполненных алгоритмом.
        /// </summary>
        public int Comparisons { get; set; }

        /// <summary>
        /// Количество обменов (перестановок) элементов.
        /// </summary>
        public int Swaps { get; set; }

        /// <summary>
        /// Общее количество шагов выполнения.
        /// </summary>
        public int Steps { get; set; }

        /// <summary>
        /// Количество рекурсивных вызовов (для рекурсивных алгоритмов).
        /// </summary>
        public int RecursiveCalls { get; set; }

        /// <summary>
        /// Количество операций с памятью (выделение/освобождение).
        /// </summary>
        public int MemoryOperations { get; set; }

        /// <summary>
        /// Теоретическая временная сложность алгоритма.
        /// </summary>
        public double TimeComplexity { get; set; }

        /// <summary>
        /// Теоретическая пространственная сложность алгоритма.
        /// </summary>
        public double SpaceComplexity { get; set; }

        /// <summary>
        /// Дополнительные пользовательские метрики алгоритма.
        /// </summary>
        public Dictionary<string, double> CustomMetrics { get; set; } = new();

        /// <summary>
        /// Сбрасывает все статистические данные к значениям по умолчанию.
        /// </summary>
        public void Reset()
        {
            Comparisons = 0;
            Swaps = 0;
            Steps = 0;
            RecursiveCalls = 0;
            MemoryOperations = 0;
            CustomMetrics.Clear();
        }

        /// <summary>
        /// Создает глубокую копию объекта статистики.
        /// </summary>
        /// <returns>Новый объект <see cref="AlgorithmStatistics"/> с теми же значениями.</returns>
        public AlgorithmStatistics Clone()
        {
            return new AlgorithmStatistics
            {
                Comparisons = Comparisons,
                Swaps = Swaps,
                Steps = Steps,
                RecursiveCalls = RecursiveCalls,
                MemoryOperations = MemoryOperations,
                TimeComplexity = TimeComplexity,
                SpaceComplexity = SpaceComplexity,
                CustomMetrics = new Dictionary<string, double>(CustomMetrics)
            };
        }
    }
}