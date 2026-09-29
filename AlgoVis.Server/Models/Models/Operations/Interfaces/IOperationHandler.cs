using AlgoVis.Models.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext  = AlgoVis.Models.Models.DataStructures.ExecutionContext;


/// <summary>
/// Пространство имен для операций визуализации алгоритмов.
/// Содержит интерфейсы и классы для выполнения операций над шагами алгоритмов.
/// </summary>
namespace AlgoVis.Models.Models.Operations.Interfaces
{
    /// <summary>
    /// Интерфейс обработчика операций.
    /// Определяет контракт для обработки конкретных типов операций.
    /// </summary>
    /// <remarks>
    /// Каждая реализация этого интерфейса отвечает за обработку конкретного типа операции,
    /// такого как присваивание, сравнение, обмен значениями и т.д.
    /// </remarks>
    public interface IOperationHandler
    {
        /// <summary>
        /// Выполняет обработку указанного шага алгоритма в заданном контексте выполнения.
        /// </summary>
        /// <param name="step">Шаг алгоритма для обработки. Содержит данные операции.</param>
        /// <param name="context">Контекст выполнения алгоритма для модификации состояния.</param>
        void Execute(AlgorithmStep step, ExecutionContext context);
    }
}
