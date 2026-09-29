using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    /// <summary>
    /// Интерфейс для генераторов случайных структур данных.
    /// </summary>
    /// <remarks>
    /// Определяет базовый контракт для генерации структур данных различных типов.
    /// </remarks>
    public interface IRandomStructureGenerator
    {
        /// <summary>
        /// Получает тип структуры данных, которую генерирует этот генератор.
        /// </summary>
        string StructureType { get; }

        /// <summary>
        /// Генерирует структуру данных с указанными параметрами.
        /// </summary>
        /// <param name="parameters">Параметры для генерации структуры.</param>
        /// <returns>Сгенерированная структура данных.</returns>
        IDataStructure Generate(Dictionary<string, object> parameters);

        /// <summary>
        /// Получает параметры по умолчанию для генерации структуры.
        /// </summary>
        /// <returns>Словарь параметров по умолчанию.</returns>
        Dictionary<string, object> GetDefaultParameters();
    }

    /// <summary>
    /// Обобщенный интерфейс для генераторов случайных структур данных.
    /// </summary>
    /// <typeparam name="T">Тип структуры данных, реализующий интерфейс IDataStructure.</typeparam>
    /// <remarks>
    /// Наследует от необобщенного интерфейса и добавляет типобезопасные методы.
    /// </remarks>
    public interface IRandomStructureGenerator<T> : IRandomStructureGenerator where T : IDataStructure
    {
        /// <summary>
        /// Генерирует структуру данных указанного типа с указанными параметрами.
        /// </summary>
        /// <param name="parameters">Параметры для генерации структуры.</param>
        /// <returns>Сгенерированная структура данных указанного типа.</returns>
        new T Generate(Dictionary<string, object> parameters);
    }
}