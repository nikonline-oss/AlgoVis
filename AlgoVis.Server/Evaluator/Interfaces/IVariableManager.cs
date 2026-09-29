/// <summary>
/// Определяет интерфейс для управления областями видимости переменных.
/// </summary>
/// <remarks>
/// Этот интерфейс предоставляет методы для создания новых областей видимости переменных,
/// которые могут использоваться для изоляции контекстов выполнения в системе оценки алгоритмов.
/// </remarks>
using AlgoVis.Evaluator.Evaluator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Interfaces
{
    /// <summary>
    /// Менеджер для создания областей видимости переменных.
    /// </summary>
    /// <remarks>
    /// Реализует механизм управления областями видимости, позволяя создавать корневые
    /// и дочерние области для организации иерархического хранения переменных.
    /// </remarks>
    public interface IVariableManager
    {
        /// <summary>
        /// Создает новую корневую область видимости переменных.
        /// </summary>
        /// <returns>Новая область видимости переменных.</returns>
        /// <remarks>
        /// Созданная область не имеет родительской области и представляет собой
        /// самостоятельный контекст для хранения переменных.
        /// </remarks>
        IVariableScope CreateScope();

        /// <summary>
        /// Создает дочернюю область видимости переменных.
        /// </summary>
        /// <param name="parent">Родительская область видимости.</param>
        /// <returns>Новая дочерняя область видимости.</returns>
        /// <remarks>
        /// Дочерняя область наследует переменные из родительской области
        /// и может переопределять их значения.
        /// </remarks>
        IVariableScope CreateChildScope(VariableScope parent);
    }
}