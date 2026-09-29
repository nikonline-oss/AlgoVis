using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    /// <summary>
    /// Базовый абстрактный класс для генераторов случайных структур данных.
    /// </summary>
    /// <typeparam name="T">Тип структуры данных, реализующий интерфейс IDataStructure.</typeparam>
    /// <remarks>
    /// Этот класс предоставляет базовую функциональность для генерации случайных структур данных,
    /// включая работу с параметрами и генерацию случайных чисел.
    /// </remarks>
    public abstract class RandomGeneratorBase<T> : IRandomStructureGenerator<T> where T : IDataStructure
    {
        /// <summary>
        /// Генератор случайных чисел.
        /// </summary>
        protected Random _random;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RandomGeneratorBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Использует случайное начальное значение для генератора случайных чисел.
        /// </remarks>
        public RandomGeneratorBase()
        {
            _random = new Random();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RandomGeneratorBase{T}"/> с указанным начальным значением.
        /// </summary>
        /// <param name="seed">Начальное значение для генератора случайных чисел.</param>
        public RandomGeneratorBase(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Получает тип структуры данных, которую генерирует этот класс.
        /// </summary>
        public abstract string StructureType { get; }

        /// <summary>
        /// Генерирует структуру данных с указанными параметрами.
        /// </summary>
        /// <param name="parameters">Параметры для генерации структуры.</param>
        /// <returns>Сгенерированная структура данных.</returns>
        public abstract T Generate(Dictionary<string, object> parameters);

        /// <summary>
        /// Получает параметры по умолчанию для генерации структуры.
        /// </summary>
        /// <returns>Словарь параметров по умолчанию.</returns>
        public abstract Dictionary<string, object> GetDefaultParameters();

        /// <summary>
        /// Генерирует структуру данных (необобщенная версия).
        /// </summary>
        /// <param name="parameters">Параметры для генерации структуры.</param>
        /// <returns>Сгенерированная структура данных.</returns>
        IDataStructure IRandomStructureGenerator.Generate(Dictionary<string, object> parameters) => Generate(parameters);

        /// <summary>
        /// Получает значение целочисленного параметра из словаря параметров.
        /// </summary>
        /// <param name="parameters">Словарь параметров.</param>
        /// <param name="key">Ключ параметра.</param>
        /// <param name="defaultValue">Значение по умолчанию, если параметр не найден.</param>
        /// <returns>Значение параметра или значение по умолчанию.</returns>
        /// <remarks>
        /// Поддерживает получение значения как из нативного int, так и из JsonElement.
        /// </remarks>
        protected int GetParameterValue(Dictionary<string, object> parameters, string key, int defaultValue)
        {
            if (parameters != null && parameters.ContainsKey(key))
            {
                if (parameters[key].GetType().ToString().ToLower() == "system.int32")
                {
                    return (int)parameters[key];
                }
                else
                {
                    var param = (JsonElement)parameters[key];
                    return param.GetInt32();
                }
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Получает значение строкового параметра из словаря параметров.
        /// </summary>
        /// <param name="parameters">Словарь параметров.</param>
        /// <param name="key">Ключ параметра.</param>
        /// <param name="defaultValue">Значение по умолчанию, если параметр не найден.</param>
        /// <returns>Значение параметра или значение по умолчанию.</returns>
        protected string GetParameterValue(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            return parameters != null && parameters.ContainsKey(key)
                ? parameters[key]?.ToString()
                : defaultValue;
        }
    }
}