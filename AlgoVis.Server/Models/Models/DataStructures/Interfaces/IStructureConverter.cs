using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.DataStructures.Interfaces
{
    /// <summary>
    /// Интерфейс конвертера между структурами данных и значениями переменных.
    /// </summary>
    /// <remarks>
    /// Предоставляет механизм для преобразования структур данных в формат,
    /// понятный системе оценки (Evaluator), и обратного преобразования.
    /// Это позволяет использовать структуры данных в качестве параметров алгоритмов
    /// и сохранять результаты работы алгоритмов обратно в структуры данных.
    /// </remarks>
    public interface IStructureConverter
    {
        /// <summary>
        /// Преобразует структуру данных в значение переменной для системы оценки.
        /// </summary>
        /// <param name="structure">Структура данных для преобразования.</param>
        /// <returns>Значение переменной, представляющее структуру данных.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если 
        /// <paramref name="structure"/> равен <c>null</c>.</exception>
        IVariableValue ConvertToVariableValue(IDataStructure structure);

        /// <summary>
        /// Преобразует значение переменной обратно в структуру данных.
        /// </summary>
        /// <param name="value">Значение переменной для преобразования.</param>
        /// <param name="structureType">Тип структуры данных, которую нужно создать
        /// (например, "Array", "LinkedList").</param>
        /// <returns>Созданная структура данных.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если 
        /// <paramref name="value"/> или <paramref name="structureType"/> равны <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если 
        /// <paramref name="structureType"/> не поддерживается конвертером.</exception>
        IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType);

        /// <summary>
        /// Проверяет, может ли конвертер работать с указанным типом структуры данных.
        /// </summary>
        /// <param name="structureType">Тип структуры данных для проверки.</param>
        /// <returns><c>true</c>, если конвертер поддерживает указанный тип структуры;
        /// в противном случае — <c>false</c>.</returns>
        bool CanConvert(string structureType);
    }
}