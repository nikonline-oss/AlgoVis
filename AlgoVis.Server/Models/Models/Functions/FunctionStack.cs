using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Functions
{
    /// <summary>
    /// Представляет стек вызовов функций пользовательского алгоритма.
    /// </summary>
    /// <remarks>
    /// Обеспечивает управление контекстами выполнения функций в виде стека (LIFO),
    /// что позволяет обрабатывать вложенные вызовы функций.
    /// </remarks>
    public class FunctionStack
    {
        private readonly Stack<FunctionContext> _stack = new Stack<FunctionContext>();

        /// <summary>
        /// Получает текущую глубину стека вызовов функций.
        /// </summary>
        /// <value>Количество контекстов функций в стеке.</value>
        public int CurrentDepth => _stack.Count;

        /// <summary>
        /// Получает текущий контекст функции (верхний элемент стека).
        /// </summary>
        /// <value>
        /// Текущий <see cref="FunctionContext"/> или <c>null</c>, если стек пуст.
        /// </value>
        public FunctionContext Current => _stack.Count > 0 ? _stack.Peek() : null;

        /// <summary>
        /// Добавляет контекст функции в стек.
        /// </summary>
        /// <param name="context">Контекст функции для добавления в стек.</param>
        public void Push(FunctionContext context) => _stack.Push(context);

        /// <summary>
        /// Извлекает и возвращает контекст функции из вершины стека.
        /// </summary>
        /// <returns>
        /// Извлеченный <see cref="FunctionContext"/> или <c>null</c>, если стек пуст.
        /// </returns>
        public FunctionContext Pop() => _stack.Count > 0 ? _stack.Pop() : null;

        /// <summary>
        /// Очищает стек вызовов функций.
        /// </summary>
        public void Clear() => _stack.Clear();
    }
}