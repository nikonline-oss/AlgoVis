using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;

namespace AlgoVis.Models.Models.Steps.Interfaces
{
    /// <summary>
    /// Интерфейс исполнителя шагов алгоритма.
    /// </summary>
    /// <remarks>
    /// Определяет контракт для выполнения отдельных шагов алгоритма в заданном контексте выполнения.
    /// </remarks>
    public interface IStepExecutor
    {
        /// <summary>
        /// Выполняет шаг алгоритма по его идентификатору.
        /// </summary>
        /// <param name="stepId">Идентификатор шага для выполнения.</param>
        /// <param name="context">Контекст выполнения, содержащий состояние алгоритма и данные.</param>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="stepId"/> равен null или пустой строке,
        /// или если <paramref name="context"/> равен null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если шаг с указанным идентификатором не найден
        /// или произошла ошибка при выполнении шага.
        /// </exception>
        void Execute(string stepId, ExecutionContext context);
    }
}