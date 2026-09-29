using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgoVis.Models.Models.Visualization;

namespace AlgoVis.Models.Models.Core
{
    /// <summary>
    /// Представляет результат выполнения алгоритма с конкретным типом шагов визуализации.
    /// </summary>
    /// <remarks>
    /// Этот класс содержит всю информацию о выполнении алгоритма, включая шаги визуализации,
    /// статистику, время выполнения и выходные данные.
    /// </remarks>
    public class AlgorithmResult
    {
        /// <summary>
        /// Название выполненного алгоритма.
        /// </summary>
        public string AlgorithmName { get; set; } = string.Empty;

        /// <summary>
        /// Уникальный идентификатор сессии выполнения.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Тип структуры данных, используемой в алгоритме.
        /// </summary>
        public string StructureType { get; set; } = string.Empty;

        /// <summary>
        /// Список шагов визуализации, выполненных алгоритмом.
        /// </summary>
        public List<VisualizationStep> steps { get; set; } = new();

        /// <summary>
        /// Статистические данные о выполнении алгоритма.
        /// </summary>
        public AlgorithmStatistics Statistics { get; set; } = new();

        /// <summary>
        /// Время выполнения алгоритма.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Выходные данные алгоритма в виде пар ключ-значение.
        /// </summary>
        public Dictionary<string, object> OutputData { get; set; } = new();
    }

    /// <summary>
    /// Универсальный результат выполнения алгоритма с пользовательским типом шагов.
    /// </summary>
    /// <typeparam name="TStep">Тип шагов визуализации (должен быть ссылочным типом).</typeparam>
    /// <remarks>
    /// Предоставляет обобщенную версию <see cref="AlgorithmResult"/> для работы с любым типом шагов.
    /// </remarks>
    public class AlgorithmResult<TStep> where TStep : class
    {
        /// <summary>
        /// Название выполненного алгоритма.
        /// </summary>
        public string AlgorithmName { get; set; } = string.Empty;

        /// <summary>
        /// Уникальный идентификатор сессии выполнения.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Тип структуры данных, используемой в алгоритме.
        /// </summary>
        public string StructureType { get; set; } = string.Empty;

        /// <summary>
        /// Список шагов визуализации пользовательского типа.
        /// </summary>
        public List<TStep> steps { get; set; } = new();

        /// <summary>
        /// Статистические данные о выполнении алгоритма.
        /// </summary>
        public AlgorithmStatistics Statistics { get; set; } = new();

        /// <summary>
        /// Время выполнения алгоритма.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Выходные данные алгоритма в виде пар ключ-значение.
        /// </summary>
        public Dictionary<string, object> OutputData { get; set; } = new();
    }
}