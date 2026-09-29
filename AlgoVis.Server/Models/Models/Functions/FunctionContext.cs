using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Functions
{
    /// <summary>
    /// Представляет контекст выполнения функции пользовательского алгоритма.
    /// </summary>
    /// <remarks>
    /// Содержит информацию о состоянии функции во время выполнения, включая её имя,
    /// область видимости переменных и идентификатор шага возврата.
    /// </remarks>
    public class FunctionContext
    {
        /// <summary>
        /// Получает или задает имя функции.
        /// </summary>
        /// <value>Строка, содержащая имя функции.</value>
        public string FunctionName { get; set; } = string.Empty;

        /// <summary>
        /// Получает или задает идентификатор шага, к которому следует вернуться после завершения функции.
        /// </summary>
        /// <value>Строка, содержащая идентификатор шага возврата.</value>
        public string ReturnStepId { get; set; } = string.Empty;

        /// <summary>
        /// Получает или задает область видимости переменных, связанную с этим контекстом функции.
        /// </summary>
        /// <value>Экземпляр <see cref="IVariableScope"/>, представляющий область видимости переменных.</value>
        public IVariableScope Variables { get; set; }

        /// <summary>
        /// Получает или задает глубину вложенности вызова функции.
        /// </summary>
        /// <value>Целое число, представляющее глубину вызова функции (0 для корневого уровня).</value>
        public int Depth { get; set; }
    }
}