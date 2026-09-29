using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для интерпретаторов пользовательских алгоритмов.
    /// </summary>
    /// <remarks>
    /// Определяет контракт для выполнения пользовательских алгоритмов на структурах данных.
    /// </remarks>
    public interface ICustomAlgorithmInterpreter
    {
        /// <summary>
        /// Выполняет пользовательский алгоритм.
        /// </summary>
        /// <param name="request">Запрос на выполнение алгоритма, содержащий описание алгоритма.</param>
        /// <param name="structure">Структура данных, на которой выполняется алгоритм.</param>
        /// <returns>Результат выполнения алгоритма.</returns>
        CustomAlgorithmResult Execute(CustomAlgorithmRequest request, IDataStructure structure);
    }
}