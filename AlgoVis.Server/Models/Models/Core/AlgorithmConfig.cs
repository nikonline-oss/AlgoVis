using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Core
{
    /// <summary>
    /// Конфигурация алгоритма визуализации.
    /// </summary>
    /// <remarks>
    /// Этот класс содержит все параметры, необходимые для настройки и выполнения
    /// алгоритма визуализации. Используется для передачи настроек между клиентом
    /// и сервером или между различными компонентами системы.
    /// </remarks>
    public class AlgorithmConfig
    {
        /// <summary>
        /// Название алгоритма.
        /// </summary>
        /// <value>
        /// Строка, содержащая уникальное имя алгоритма (например, "BubbleSort", "QuickSort").
        /// По умолчанию - пустая строка.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Длина массива данных для алгоритма.
        /// </summary>
        /// <value>
        /// Целое число, определяющее размер массива или количество элементов,
        /// с которыми будет работать алгоритм. По умолчанию - 10.
        /// </value>
        public int Length { get; set; } = 10;

        /// <summary>
        /// Флаг, указывающий, используются ли дополнительные аргументы.
        /// </summary>
        /// <value>
        /// <c>true</c> если алгоритм использует массив <see cref="Args"/>;
        /// <c>false</c> в противном случае. По умолчанию - <c>false</c>.
        /// </value>
        public bool IsArgs { get; set; } = false;

        /// <summary>
        /// Дополнительные аргументы для алгоритма.
        /// </summary>
        /// <value>
        /// Массив целых чисел, содержащий дополнительные параметры для алгоритма.
        /// Используется только если <see cref="IsArgs"/> равно <c>true</c>.
        /// По умолчанию - пустой массив.
        /// </value>
        public int[] Args { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Идентификатор сессии.
        /// </summary>
        /// <value>
        /// Уникальный идентификатор сессии визуализации. Используется для
        /// связи с конкретным экземпляром визуализации на клиенте.
        /// По умолчанию - пустая строка.
        /// </value>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Дополнительные параметры алгоритма.
        /// </summary>
        /// <value>
        /// Словарь, содержащий произвольные параметры алгоритма в формате
        /// ключ-значение. Может быть <c>null</c>. По умолчанию - новый пустой словарь.
        /// </value>
        /// <example>
        /// Пример использования:
        /// <code>
        /// config.Parameters = new Dictionary&lt;string, object&gt;()
        /// {
        ///     { "delay", 100 },
        ///     { "colorScheme", "rainbow" },
        ///     { "showComparison", true }
        /// };
        /// </code>
        /// </example>
        public Dictionary<string, object>? Parameters { get; set; } = new Dictionary<string, object>();
    }
}