using AlgoVis.Core.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.DataStructures
{
    /// <summary>
    /// Представляет структуру данных "Массив" для визуализации алгоритмов.
    /// Реализует интерфейс IDataStructure<int[]> для работы с целочисленными массивами.
    /// </summary>
    /// <remarks>
    /// Класс поддерживает сохранение исходного состояния массива и его восстановление.
    /// </remarks>
    public class ArrayStructure : IDataStructure<int[]>
    {
        /// <summary>Тип структуры данных (константа "array")</summary>
        public string Type => "array";

        /// <summary>Уникальный идентификатор экземпляра массива</summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        private int[] _data;
        private readonly int[] _originData;

        /// <summary>
        /// Инициализирует новый экземпляр класса ArrayStructure с указанными данными.
        /// </summary>
        /// <param name="data">Исходный массив целых чисел.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если data равен null.</exception>
        public ArrayStructure(int[] data)
        {
            _data = data;
            _originData = (int[])_data.Clone();
        }

        /// <summary>
        /// Получает текущее состояние массива (глубокая копия).
        /// </summary>
        /// <returns>Копия текущего массива.</returns>
        public int[] GetState() => (int[])_data.Clone();

        /// <summary>
        /// Применяет новое состояние массива.
        /// </summary>
        /// <param name="state">Новое состояние массива.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если state равен null.</exception>
        public void ApplyState(int[] state) => _data = (int[])state.Clone();

        /// <summary>
        /// Преобразует текущее состояние массива в данные для визуализации.
        /// </summary>
        /// <returns>Объект VisualizationData с элементами массива.</returns>
        public VisualizationData ToVisualizationData()
        {
            return new VisualizationData
            {
                structureType = "array",
                elements = _data.Select((value, index) =>
                    new KeyValuePair<string, object>(index.ToString(), new
                    {
                        value,
                        index,
                        label = $"arr[{index}]"
                    })).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        /// <summary>
        /// Получает исходное состояние массива (состояние при создании).
        /// </summary>
        /// <returns>Копия исходного массива.</returns>
        public int[] GetOriginState() => _originData;
    }
}