using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Models.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Functions.Interfaces
{
    /// <summary>
    /// Менеджер функций, предоставляющий доступ к функциям и управляющий их контекстами выполнения.
    /// </summary>
    /// <remarks>
    /// Этот интерфейс определяет методы для создания контекстов выполнения функций
    /// и получения информации о доступных функциях в рамках алгоритма.
    /// </remarks>
    public interface IFunctionManager
    {
        /// <summary>
        /// Создает контекст выполнения для указанной функции.
        /// </summary>
        /// <param name="functionName">Имя функции, для которой создается контекст.</param>
        /// <param name="returnStepId">Идентификатор шага, в который должно вернуться управление после выполнения функции.</param>
        /// <param name="parentScope">Родительская область видимости переменных, от которой наследуется контекст.</param>
        /// <returns>Контекст выполнения функции, содержащий информацию о выполнении.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="functionName"/> имеет значение null или пусто.</exception>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="parentScope"/> имеет значение null.</exception>
        /// <example>
        /// Пример использования:
        /// <code>
        /// var context = functionManager.CreateContext("CalculateSum", "step_123", parentScope);
        /// </code>
        /// </example>
        FunctionContext CreateContext(string functionName, string returnStepId, IVariableScope parentScope);

        /// <summary>
        /// Получает группу функций по имени для указанного запроса алгоритма.
        /// </summary>
        /// <param name="name">Имя функции или группы функций для поиска.</param>
        /// <param name="request">Запрос пользовательского алгоритма, содержащий параметры выполнения.</param>
        /// <returns>Группа функций, соответствующая указанному имени и запросу.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="name"/> или <paramref name="request"/> имеют значение null.</exception>
        /// <exception cref="KeyNotFoundException">Выбрасывается, если функция с указанным именем не найдена.</exception>
        /// <remarks>
        /// Метод выполняет поиск функции по имени с учетом контекста запроса алгоритма.
        /// Возвращаемая группа может содержать несколько вариантов реализации функции.
        /// </remarks>
        FunctionGroup GetFunction(string name, CustomAlgorithmRequest request);
    }
}