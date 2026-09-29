using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Parsing
{
    /// <summary>
    /// Исключение, возникающее при ошибках синтаксического анализа.
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary>
        /// Получает позицию в строке, где произошла ошибка.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса ParseException.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        /// <param name="position">Позиция в строке, где произошла ошибка.</param>
        public ParseException(string message, int position) : base(message)
        {
            Position = position;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса ParseException.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        /// <param name="position">Позиция в строке, где произошла ошибка.</param>
        /// <param name="innerException">Внутреннее исключение.</param>
        public ParseException(string message, int position, Exception innerException)
            : base(message, innerException)
        {
            Position = position;
        }
    }
}