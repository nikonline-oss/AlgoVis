using AlgoVis.Models.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;

/// <summary>
/// Пространство имен для операций визуализации алгоритмов.
/// Содержит интерфейсы и классы для выполнения операций над шагами алгоритмов.
/// </summary>
namespace AlgoVis.Models.Models.Operations.Interfaces
{
    /// <summary>
    /// Интерфейс исполнителя операций.
    /// Определяет контракт для выполнения шагов алгоритма в заданном контексте выполнения.
    /// </summary>
    /// <remarks>
    /// Реализации этого интерфейса отвечают за координацию выполнения операций,
    /// используя соответствующие обработчики для разных типов операций.
    /// </remarks>
    public interface IOperationExecutor
    {
        /// <summary>
        /// Выполняет указанный шаг алгоритма в заданном контексте выполнения.
        /// </summary>
        /// <param name="step">Шаг алгоритма для выполнения. Содержит тип операции и данные.</param>
        /// <param name="context">Контекст выполнения алгоритма. Содержит состояние выполнения, переменные и другие данные.</param>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, когда тип операции не поддерживается исполнителем.
        /// </exception>
        void Execute(AlgorithmStep step, ExecutionContext context);
    }
}

