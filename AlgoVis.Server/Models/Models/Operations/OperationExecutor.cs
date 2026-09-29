using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.Operations.Handlers;
using AlgoVis.Models.Models.Operations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;


/// <summary>
/// Пространство имен для операций визуализации алгоритмов.
/// Содержит реализации интерфейсов операций.
/// </summary>
namespace AlgoVis.Models.Models.Operations
{
    /// <summary>
    /// Реализация исполнителя операций для визуализации алгоритмов.
    /// Координирует выполнение операций, используя зарегистрированные обработчики.
    /// </summary>
    /// <remarks>
    /// Этот класс использует паттерн "Цепочка ответственности" через словарь обработчиков.
    /// Каждый тип операции связан с конкретным обработчиком.
    /// </remarks>
    public class OperationExecutor : IOperationExecutor
    {
        /// <summary>
        /// Словарь обработчиков операций, где ключ - тип операции, значение - обработчик.
        /// </summary>
        /// <remarks>
        /// Использует сравнение строк без учета регистра (StringComparer.OrdinalIgnoreCase).
        /// </remarks>
        private readonly Dictionary<string, IOperationHandler> _handlers;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="OperationExecutor"/>.
        /// Регистрирует все доступные обработчики операций.
        /// </summary>
        /// <remarks>
        /// Поддерживаемые типы операций:
        /// <list type="bullet">
        /// <item><description>assign - присваивание значений</description></item>
        /// <item><description>condition - условные операции</description></item>
        /// <item><description>compare - операции сравнения</description></item>
        /// <item><description>swap - обмен значениями</description></item>
        /// <item><description>call_function - вызов функций</description></item>
        /// <item><description>return - возврат значений</description></item>
        /// <item><description>generic - универсальные операции</description></item>
        /// </list>
        /// </remarks>
        public OperationExecutor()
        {
            _handlers = new Dictionary<string, IOperationHandler>(StringComparer.OrdinalIgnoreCase)
            {
                ["assign"] = new AssignOperationHandler(),
                ["condition"] = new ConditionOperationHandler(),
                ["compare"] = new CompareOperationHandler(),
                ["swap"] = new SwapOperationHandler(),
                ["call_function"] = new FunctionCallOperationHandler(),
                ["return"] = new ReturnOperationHandler(),
                ["generic"] = new GenericOperationHandler()
            };
        }

        /// <summary>
        /// Выполняет указанный шаг алгоритма, используя соответствующий обработчик.
        /// </summary>
        /// <param name="step">Шаг алгоритма для выполнения. Должен содержать поле <c>type</c> с типом операции.</param>
        /// <param name="context">Контекст выполнения для передачи обработчику операции.</param>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если тип операции из <paramref name="step.type"/> не найден в словаре обработчиков.
        /// </exception>
        /// <example>
        /// <code>
        /// var executor = new OperationExecutor();
        /// var step = new AlgorithmStep { type = "assign", ... };
        /// var context = new ExecutionContext();
        /// executor.Execute(step, context); // Будет вызван AssignOperationHandler
        /// </code>
        /// </example>
        public void Execute(AlgorithmStep step, ExecutionContext context)
        {
            if (_handlers.TryGetValue(step.type, out var handler))
            {
                handler.Execute(step, context);
            }
            else
            {
                throw new InvalidOperationException($"Неизвестный тип операции: {step.type}");
            }
        }
    }
}