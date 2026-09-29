using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Types
{
    /// <summary>
    /// Перечисление поддерживаемых типов переменных.
    /// 
    /// Определяет все типы данных, которые могут быть использованы
    /// в системе визуализации алгоритмов.
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// Целочисленный тип (int).
        /// 
        /// 32-битное целое число со знаком.
        /// </summary>
        Int,

        /// <summary>
        /// Вещественный тип с двойной точностью (double).
        /// 
        /// 64-битное число с плавающей точкой.
        /// </summary>
        Double,

        /// <summary>
        /// Логический тип (bool).
        /// 
        /// Принимает значения true или false.
        /// </summary>
        Bool,

        /// <summary>
        /// Строковый тип (string).
        /// 
        /// Последовательность символов Unicode.
        /// </summary>
        String,

        /// <summary>
        /// Массив.
        /// 
        /// Коллекция элементов одного типа с доступом по индексу.
        /// </summary>
        Array,

        /// <summary>
        /// Объект (ссылочный тип).
        /// 
        /// Произвольный объект с набором свойств и методов.
        /// </summary>
        Object,

        /// <summary>
        /// Нулевое значение (null).
        /// 
        /// Специальное значение, указывающее на отсутствие объекта.
        /// </summary>
        Null
    }
}