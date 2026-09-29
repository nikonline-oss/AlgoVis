using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues.Base
{
    /// <summary>
    /// Базовый абстрактный класс для представления значений переменных в системе.
    /// </summary>
    /// <remarks>
    /// Этот класс предоставляет базовую реализацию интерфейса <see cref="IVariableValue"/>
    /// и определяет общие свойства и методы для всех типов значений переменных.
    /// Конкретные типы значений должны наследоваться от этого класса и реализовывать
    /// абстрактные методы преобразования.
    /// </remarks>
    public abstract class VariableValue : IVariableValue
    {
        /// <summary>
        /// Получает тип значения переменной.
        /// </summary>
        /// <value>
        /// Тип значения, который определяется в производных классах.
        /// </value>
        public abstract VariableType Type { get; }

        /// <summary>
        /// Получает необработанное (сырое) значение переменной в виде объекта.
        /// </summary>
        /// <value>
        /// Необработанное значение переменной. Конкретный тип зависит от реализации в производном классе.
        /// </value>
        public abstract object RawValue { get; }

        /// <summary>
        /// Получает значение свойства по имени.
        /// </summary>
        /// <param name="name">Имя свойства для получения.</param>
        /// <returns>Значение указанного свойства.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, когда свойство с указанным именем не поддерживается типом значения.
        /// </exception>
        /// <remarks>
        /// Базовая реализация выбрасывает исключение. Производные классы, поддерживающие свойства,
        /// должны переопределить этот метод.
        /// </remarks>
        public virtual IVariableValue GetProperty(string name)
        {
            throw new InvalidOperationException($"Property '{name}' not supported for type {Type}");
        }

        /// <summary>
        /// Устанавливает значение свойства по имени.
        /// </summary>
        /// <param name="name">Имя свойства для установки.</param>
        /// <param name="value">Значение для установки свойства.</param>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, когда установка свойств не поддерживается типом значения.
        /// </exception>
        /// <remarks>
        /// Базовая реализация выбрасывает исключение. Производные классы, поддерживающие установку свойств,
        /// должны переопределить этот метод.
        /// </remarks>
        public virtual void SetProperty(string name, IVariableValue value)
        {
            throw new InvalidOperationException($"Cannot set properties on type {Type}");
        }

        /// <summary>
        /// Вызывает метод с указанным именем и аргументами.
        /// </summary>
        /// <param name="methodName">Имя метода для вызова.</param>
        /// <param name="args">Аргументы для передачи методу.</param>
        /// <returns>Результат выполнения метода.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, когда метод с указанным именем не поддерживается типом значения.
        /// </exception>
        /// <remarks>
        /// Базовая реализация выбрасывает исключение. Производные классы, поддерживающие методы,
        /// должны переопределить этот метод.
        /// </remarks>
        public virtual IVariableValue CallMethod(string methodName, IVariableValue[] args)
        {
            throw new InvalidOperationException($"Method '{methodName}' not supported for type {Type}");
        }

        /// <summary>
        /// Определяет, поддерживает ли значение указанное свойство.
        /// </summary>
        /// <param name="name">Имя свойства для проверки.</param>
        /// <returns>
        /// <c>true</c>, если значение поддерживает свойство с указанным именем; 
        /// в противном случае <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Базовая реализация всегда возвращает <c>false</c>. Производные классы,
        /// поддерживающие свойства, должны переопределить этот метод.
        /// </remarks>
        public virtual bool HasProperty(string name) => false;

        /// <summary>
        /// Определяет, поддерживает ли значение указанный метод.
        /// </summary>
        /// <param name="methodName">Имя метода для проверки.</param>
        /// <returns>
        /// <c>true</c>, если значение поддерживает метод с указанным именем; 
        /// в противном случае <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Базовая реализация всегда возвращает <c>false</c>. Производные классы,
        /// поддерживающие методы, должны переопределить этот метод.
        /// </remarks>
        public virtual bool HasMethod(string methodName) => false;

        /// <summary>
        /// Преобразует значение в логический тип (bool).
        /// </summary>
        /// <returns>Логическое представление значения.</returns>
        public abstract bool ToBool();

        /// <summary>
        /// Преобразует значение в число с плавающей запятой двойной точности (double).
        /// </summary>
        /// <returns>Числовое представление значения с плавающей запятой.</returns>
        public abstract double ToDouble();

        /// <summary>
        /// Преобразует значение в целое число (int).
        /// </summary>
        /// <returns>Целочисленное представление значения.</returns>
        public abstract int ToInt();

        /// <summary>
        /// Преобразует значение в строковое представление для отображения.
        /// </summary>
        /// <returns>Строковое представление значения, пригодное для отображения.</returns>
        public abstract string ToValueString();
    }
}