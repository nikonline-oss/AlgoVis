using AlgoVis.Core.Core.GenerateState;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core
{
    /// <summary>
    /// Фабрика для генерации случайных структур данных.
    /// </summary>
    /// <remarks>
    /// Этот класс предоставляет методы для генерации случайных структур данных различных типов
    /// с использованием зарегистрированных генераторов. Поддерживает регистрацию пользовательских генераторов.
    /// </remarks>
    public class RandomStructureFactory
    {
        private readonly Dictionary<string, IRandomStructureGenerator> _generators;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RandomStructureFactory"/>.
        /// </summary>
        /// <remarks>
        /// Регистрирует генераторы для стандартных типов структур данных: array, graph, binarytree.
        /// </remarks>
        public RandomStructureFactory()
        {
            _generators = new Dictionary<string, IRandomStructureGenerator>
            {
                { "array", new ArrayRandomGenerator() },
                { "graph", new GraphRandomGenerator() },
                { "binarytree", new BinaryTreeRandomGenerator() }
            };
        }

        /// <summary>
        /// Регистрирует новый генератор для указанного типа структуры данных.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <param name="generator">Генератор для регистрации.</param>
        public void RegisterGenerator(string structureType, IRandomStructureGenerator generator)
        {
            _generators[structureType] = generator;
        }

        /// <summary>
        /// Генерирует структуру данных указанного типа.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <param name="parameters">Параметры для генерации структуры.</param>
        /// <returns>Сгенерированная структура данных.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если для указанного типа структуры не найден генератор.
        /// </exception>
        public IDataStructure GenerateStructure(string structureType, Dictionary<string, object> parameters = null)
        {
            if (_generators.TryGetValue(structureType.ToLower(), out var generator))
            {
                var actualParameters = parameters ?? generator.GetDefaultParameters();
                return generator.Generate(actualParameters);
            }

            throw new ArgumentException($"No generator found for structure type: {structureType}");
        }

        /// <summary>
        /// Получает параметры по умолчанию для генерации структуры указанного типа.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>Параметры по умолчанию.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если для указанного типа структуры не найден генератор.
        /// </exception>
        public Dictionary<string, object> GetDefaultParameters(string structureType)
        {
            if (_generators.TryGetValue(structureType.ToLower(), out var generator))
            {
                return generator.GetDefaultParameters();
            }

            throw new ArgumentException($"No generator found for structure type: {structureType}");
        }

        /// <summary>
        /// Получает список доступных типов структур данных.
        /// </summary>
        /// <returns>Список типов структур данных.</returns>
        public List<string> GetAvailableStructures()
        {
            return _generators.Keys.ToList();
        }
    }
}