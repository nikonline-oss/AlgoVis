using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgoVis.Models.Models.Visualization;

namespace AlgoVis.Models.Models.DataStructures.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для всех структур данных в системе визуализации алгоритмов.
    /// </summary>
    /// <remarks>
    /// Определяет общий контракт для сериализации состояния структуры данных,
    /// восстановления состояния и преобразования в формат, пригодный для визуализации.
    /// Все специализированные структуры данных должны реализовывать этот интерфейс.
    /// </remarks>
    public interface IDataStructure
    {
        /// <summary>
        /// Получает тип структуры данных (например, "Array", "LinkedList", "BinaryTree").
        /// </summary>
        /// <value>Строковый идентификатор типа структуры данных.</value>
        string Type { get; }

        /// <summary>
        /// Получает уникальный идентификатор экземпляра структуры данных.
        /// </summary>
        /// <value>Уникальный идентификатор (GUID или другой уникальный ключ).</value>
        string Id { get; }

        /// <summary>
        /// Преобразует текущее состояние структуры данных в формат для визуализации.
        /// </summary>
        /// <returns>Данные визуализации, содержащие информацию о структуре и её элементах.</returns>
        VisualizationData ToVisualizationData();

        /// <summary>
        /// Получает текущее состояние структуры данных в виде объекта.
        /// </summary>
        /// <returns>Текущее состояние структуры данных.</returns>
        object GetState();

        /// <summary>
        /// Получает исходное (начальное) состояние структуры данных в виде объекта.
        /// </summary>
        /// <returns>Исходное состояние структуры данных до любых модификаций.</returns>
        object GetOriginState();

        /// <summary>
        /// Применяет сохранённое состояние к структуре данных.
        /// </summary>
        /// <param name="state">Состояние, которое нужно применить (должно быть совместимого типа).</param>
        /// <exception cref="InvalidCastException">Выбрасывается, если переданный объект
        /// состояния не совместим с типом структуры данных.</exception>
        void ApplyState(object state);
    }

    /// <summary>
    /// Обобщённый интерфейс структуры данных с типизированным состоянием.
    /// </summary>
    /// <typeparam name="T">Тип, используемый для представления состояния структуры данных.</typeparam>
    /// <remarks>
    /// Наследует от <see cref="IDataStructure"/> и добавляет типизированные методы
    /// для работы с состоянием, избегая операций приведения типов.
    /// Реализации по умолчанию обеспечивают совместимость с базовым интерфейсом.
    /// </remarks>
    public interface IDataStructure<T> : IDataStructure
    {
        /// <summary>
        /// Получает текущее состояние структуры данных в типизированном виде.
        /// </summary>
        /// <returns>Текущее состояние структуры данных типа <typeparamref name="T"/>.</returns>
        new T GetState();

        /// <summary>
        /// Получает исходное (начальное) состояние структуры данных в типизированном виде.
        /// </summary>
        /// <returns>Исходное состояние структуры данных типа <typeparamref name="T"/>.</returns>
        new T GetOriginState();

        /// <summary>
        /// Применяет сохранённое типизированное состояние к структуре данных.
        /// </summary>
        /// <param name="state">Состояние типа <typeparamref name="T"/> для применения.</param>
        void ApplyState(T state);

        /// <summary>
        /// Явная реализация базового метода <see cref="IDataStructure.GetState"/>.
        /// </summary>
        /// <inheritdoc cref="IDataStructure.GetState"/>
        object IDataStructure.GetState() => GetState()!;

        /// <summary>
        /// Явная реализация базового метода <see cref="IDataStructure.GetOriginState"/>.
        /// </summary>
        /// <inheritdoc cref="IDataStructure.GetOriginState"/>
        object IDataStructure.GetOriginState() => GetOriginState()!;

        /// <summary>
        /// Явная реализация базового метода <see cref="IDataStructure.ApplyState(object)"/>.
        /// </summary>
        /// <inheritdoc cref="IDataStructure.ApplyState(object)"/>
        /// <exception cref="InvalidCastException">Выбрасывается, если переданный объект
        /// не может быть приведён к типу <typeparamref name="T"/>.</exception>
        void IDataStructure.ApplyState(object state) => ApplyState((T)state);
    }
}