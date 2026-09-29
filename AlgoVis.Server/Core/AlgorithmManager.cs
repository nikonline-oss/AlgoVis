//using AlgoVis.Core.Core.Algorithms.Tree;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Suport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core
{
    /// <summary>
    /// Менеджер для управления выполнением алгоритмов.
    /// </summary>
    /// <remarks>
    /// Этот класс предоставляет методы для выполнения как стандартных, так и пользовательских алгоритмов
    /// на различных структурах данных. Служит фасадом для более сложной логики выполнения алгоритмов.
    /// </remarks>
    public class AlgorithmManager
    {
        private readonly AlgorithmInterpreter algorithmInterpreter;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AlgorithmManager"/>.
        /// </summary>
        /// <remarks>
        /// Создает экземпляр интерпретатора алгоритмов для выполнения пользовательских алгоритмов.
        /// </remarks>
        public AlgorithmManager()
        {
            algorithmInterpreter = new AlgorithmInterpreter();
        }

        /// <summary>
        /// Выполняет пользовательский алгоритм.
        /// </summary>
        /// <param name="request">Запрос на выполнение алгоритма, содержащий описание алгоритма.</param>
        /// <param name="structure">Структура данных, на которой выполняется алгоритм.</param>
        /// <returns>Результат выполнения алгоритма.</returns>
        public CustomAlgorithmResult ExecuteCustomAlgorithm(CustomAlgorithmRequest request, IDataStructure structure)
        {
            return algorithmInterpreter.Execute(request, structure);
        }
    }
}