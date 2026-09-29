using AlgoVis.Models.Models.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    /// <summary>
    /// Генератор случайных массивов.
    /// </summary>
    /// <remarks>
    /// Этот класс генерирует массивы целых чисел с возможностью настройки размера,
    /// диапазона значений и сортировки.
    /// </remarks>
    public class ArrayRandomGenerator : RandomGeneratorBase<ArrayStructure>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ArrayRandomGenerator"/>.
        /// </summary>
        public ArrayRandomGenerator()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ArrayRandomGenerator"/> с указанным начальным значением.
        /// </summary>
        /// <param name="seed">Начальное значение для генератора случайных чисел.</param>
        public ArrayRandomGenerator(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Получает тип структуры данных - "array".
        /// </summary>
        public override string StructureType => "array";

        /// <summary>
        /// Генерирует массив с указанными параметрами.
        /// </summary>
        /// <param name="parameters">Параметры для генерации массива.</param>
        /// <returns>Сгенерированный массив.</returns>
        /// <remarks>
        /// Поддерживает генерацию отсортированных и несортированных массивов.
        /// </remarks>
        public override ArrayStructure Generate(Dictionary<string, object> parameters)
        {
            var size = GetParameterValue(parameters, "size", 10);
            var minValue = GetParameterValue(parameters, "minValue", 0);
            var maxValue = GetParameterValue(parameters, "maxValue", 100);
            var sorted = GetParameterValue(parameters, "sorted", 0);

            var data = new int[size];
            for (var i = 0; i < size; i++)
            {
                data[i] = _random.Next(minValue, maxValue + 1);
            }

            if (sorted == 1)
            {
                Array.Sort(data);
            }

            return new ArrayStructure(data);
        }

        /// <summary>
        /// Получает параметры по умолчанию для генерации массива.
        /// </summary>
        /// <returns>Словарь параметров по умолчанию.</returns>
        public override Dictionary<string, object> GetDefaultParameters()
        {
            return new Dictionary<string, object>
            {
                { "size", 10 },
                { "minValue", 0 },
                { "maxValue", 100 },
                { "sorted", 0 }
            };
        }
    }
}