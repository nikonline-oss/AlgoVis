using AlgoVis.Evaluator.Evaluator.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Visualization
{
    /// <summary>
    /// Базовый интерфейс для всех шагов визуализации алгоритмов.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// Текстовое описание того, что происходит на этом шаге алгоритма.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Дополнительные метаданные шага (расширяемые данные).
        /// </summary>
        public Dictionary<string, object> metadata { get; set; }

        /// <summary>
        /// Тип операции, выполняемой на этом шаге (например, "compare", "swap", "visit", "insert").
        /// </summary>
        public string operation { get; set; }

        /// <summary>
        /// Порядковый номер шага в последовательности визуализации.
        /// </summary>
        public int stepNumber { get; set; }
    }

    /// <summary>
    /// Абстрактный базовый класс для шагов визуализации с данными визуализации.
    /// </summary>
    public abstract class VisualizationStepBase : IStep
    {
        /// <inheritdoc/>
        public int stepNumber { get; set; }

        /// <inheritdoc/>
        public string operation { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string description { get; set; } = string.Empty;

        /// <inheritdoc/>
        public Dictionary<string, object> metadata { get; set; } = new();

        /// <summary>
        /// Данные для визуализации состояния структуры данных на этом шаге.
        /// </summary>
        public VisualizationData visualizationData { get; set; } = new();
    }

    /// <summary>
    /// Конкретная реализация шага визуализации с поддержкой переменных.
    /// </summary>
    public class VisualizationStep : IStep
    {
        /// <inheritdoc/>
        public int stepNumber { get; set; }

        /// <inheritdoc/>
        public string operation { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string description { get; set; } = string.Empty;

        /// <inheritdoc/>
        public Dictionary<string, object> metadata { get; set; } = new();

        /// <summary>
        /// Словарь переменных и их значений, актуальных на этом шаге выполнения алгоритма.
        /// Может быть null, если переменные не отслеживаются.
        /// </summary>
        public Dictionary<string, object> variables { get; set; } = null;
    }
}