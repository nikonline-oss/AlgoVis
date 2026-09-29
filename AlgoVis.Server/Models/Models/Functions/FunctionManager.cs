using AlgoVis.Evaluator.Evaluator.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.Functions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Functions
{
    /// <summary>
    /// Менеджер для управления функциями пользовательских алгоритмов.
    /// </summary>
    /// <remarks>
    /// Реализует интерфейс <see cref="IFunctionManager"/> и предоставляет функциональность
    /// для создания контекстов выполнения функций и получения информации о функциях.
    /// </remarks>
    public class FunctionManager : IFunctionManager
    {
        /// <summary>
        /// Создает новый контекст выполнения функции.
        /// </summary>
        /// <param name="functionName">Имя создаваемой функции.</param>
        /// <param name="returnStepId">Идентификатор шага, к которому следует вернуться после выполнения функции.</param>
        /// <param name="parentScope">Родительская область видимости переменных.</param>
        /// <returns>Новый экземпляр <see cref="FunctionContext"/> с указанными параметрами.</returns>
        /// <remarks>
        /// Создаваемая область видимости переменных наследует переменные из родительской области.
        /// Глубина вложенности устанавливается равной 0.
        /// </remarks>
        public FunctionContext CreateContext(string functionName, string returnStepId, IVariableScope parentScope)
        {
            return new FunctionContext
            {
                FunctionName = functionName,
                ReturnStepId = returnStepId,
                Variables = new VariableScope(parentScope),
                Depth = 0
            };
        }

        /// <summary>
        /// Получает информацию о функции по её имени из запроса пользовательского алгоритма.
        /// </summary>
        /// <param name="name">Имя искомой функции.</param>
        /// <param name="request">Запрос пользовательского алгоритма, содержащий список функций.</param>
        /// <returns>
        /// Экземпляр <see cref="FunctionGroup"/>, представляющий найденную функцию, 
        /// или <c>null</c>, если функция с указанным именем не найдена.
        /// </returns>
        public FunctionGroup GetFunction(string name, CustomAlgorithmRequest request)
        {
            return request.functions?.FirstOrDefault(f => f.name == name);
        }
    }
}