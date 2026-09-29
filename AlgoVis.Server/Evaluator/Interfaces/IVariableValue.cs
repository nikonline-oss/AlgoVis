using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Interfaces
{
    /// <summary>
    /// Представляет значение переменной в системе оценки.
    /// </summary>
    /// <remarks>
    /// Этот интерфейс обеспечивает единый способ работы с различными типами данных,
    /// предоставляя методы для доступа к свойствам, вызова методов и преобразования типов.
    /// </remarks>
    public interface IVariableValue
    {
        /// <summary>
        /// Получает тип переменной.
        /// </summary>
        /// <value>Тип переменной из перечисления <see cref="VariableType"/>.</value>
        VariableType Type { get; }

        /// <summary>
        /// Получает необработанное значение переменной.
        /// </summary>
        /// <value>
        /// Базовое значение переменной в виде объекта <see cref="object"/>.
        /// </value>
        object RawValue { get; }

        /// <summary>
        /// Получает значение свойства переменной по имени.
        /// </summary>
        /// <param name="name">Имя свойства.</param>
        /// <returns>Значение свойства.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если свойство с указанным именем не существует.
        /// </exception>
        IVariableValue GetProperty(string name);

        /// <summary>
        /// Устанавливает значение свойства переменной по имени.
        /// </summary>
        /// <param name="name">Имя свойства.</param>
        /// <param name="value">Новое значение свойства.</param>
        /// <remarks>
        /// Если свойство с указанным именем не существует и объект поддерживает
        /// динамическое добавление свойств, оно будет создано.
        /// </remarks>
        void SetProperty(string name, IVariableValue value);

        /// <summary>
        /// Вызывает метод переменной с указанными аргументами.
        /// </summary>
        /// <param name="methodName">Имя вызываемого метода.</param>
        /// <param name="args">Аргументы метода.</param>
        /// <returns>Результат выполнения метода.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если метод с указанным именем не существует или 
        /// не может быть вызван с данными аргументами.
        /// </exception>
        IVariableValue CallMethod(string methodName, IVariableValue[] args);

        /// <summary>
        /// Проверяет наличие свойства у переменной.
        /// </summary>
        /// <param name="name">Имя свойства для проверки.</param>
        /// <returns>
        /// <c>true</c> - если свойство существует; 
        /// <c>false</c> - в противном случае.
        /// </returns>
        bool HasProperty(string name);

        /// <summary>
        /// Проверяет наличие метода у переменной.
        /// </summary>
        /// <param name="methodName">Имя метода для проверки.</param>
        /// <returns>
        /// <c>true</c> - если метод существует; 
        /// <c>false</c> - в противном случае.
        /// </returns>
        bool HasMethod(string methodName);

        /// <summary>
        /// Преобразует значение переменной в целое число.
        /// </summary>
        /// <returns>Значение переменной как целое число.</returns>
        /// <exception cref="InvalidCastException">
        /// Выбрасывается, если значение не может быть преобразовано в целое число.
        /// </exception>
        int ToInt();

        /// <summary>
        /// Преобразует значение переменной в число с плавающей точкой.
        /// </summary>
        /// <returns>Значение переменной как число с плавающей точкой двойной точности.</returns>
        /// <exception cref="InvalidCastException">
        /// Выбрасывается, если значение не может быть преобразовано в число с плавающей точкой.
        /// </exception>
        double ToDouble();

        /// <summary>
        /// Преобразует значение переменной в логическое значение.
        /// </summary>
        /// <returns>Значение переменной как логическое значение.</returns>
        /// <exception cref="InvalidCastException">
        /// Выбрасывается, если значение не может быть преобразовано в логическое значение.
        /// </exception>
        bool ToBool();

        /// <summary>
        /// Преобразует значение переменной в строковое представление.
        /// </summary>
        /// <returns>Строковое представление значения переменной.</returns>
        /// <remarks>
        /// Этот метод используется для отображения значения в пользовательском интерфейсе
        /// и может отличаться от стандартного преобразования <see cref="object.ToString"/>.
        /// </remarks>
        string ToValueString();
    }
}