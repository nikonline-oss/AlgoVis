using AlgoVis.Core.Core;
using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.Operations.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;


namespace AlgoVis.Models.Models.Operations.Handlers
{
    /// <summary>
    /// Обработчик операции присваивания значений переменным, элементам массивов и свойствам.
    /// </summary>
    /// <remarks>
    /// Поддерживает три типа присваивания:
    /// 1. Прямое присваивание переменной: x = 5
    /// 2. Присваивание элемента массива: arr[0] = 10
    /// 3. Присваивание свойства объекта: obj.property = value
    /// </remarks>
    public class AssignOperationHandler : BaseOperationHandler
    {
        /// <summary>
        /// Выполняет операцию присваивания значения переменной, элементу массива или свойству.
        /// </summary>
        /// <param name="step">Шаг алгоритма с параметрами операции.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если количество параметров меньше 2.
        /// </exception>
        /// <remarks>
        /// Параметры шага:
        /// - parameters[0]: Левый операнд (переменная, элемент массива или свойство)
        /// - parameters[1]: Выражение для вычисления значения (правый операнд)
        /// </remarks>
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            if (step.parameters.Count < 2)
                throw new ArgumentException("Assign operation requires 2 parameters");

            var leftSide = step.parameters[0];
            var rightExpression = step.parameters[1].ToLower();
            IVariableValue value = EvaluateExpression(rightExpression, context);

            Console.WriteLine($"🔍 Assign: {leftSide} = {value.ToValueString()} (тип: {value?.GetType()})");

            if (IsArrayAccess(leftSide))
            {
                SetArrayElement(leftSide, value, context);
            }
            else if (IsPropertyAccess(leftSide))
            {
                SetProperty(leftSide, value, context);
            }
            else
            {
                // Прямое присваивание переменной
                context.Variables.Set(leftSide, value);
            }

            AddVisualizationStep(step, context, "assign",
                step.description ?? $"Присвоение {leftSide} = {ExtractDisplayValue(value)}",
                metadata: new Dictionary<string, object>
                {
                    ["variable"] = leftSide,
                    ["value"] = ExtractDisplayValue(value),
                    ["expression"] = rightExpression,
                    ["value_type"] = value?.GetType().Name
                });

            ExecuteNextStep(step, context);
        }

        /// <summary>
        /// Проверяет, является ли выражение доступом к элементу массива.
        /// </summary>
        /// <param name="expression">Выражение для проверки.</param>
        /// <returns>true, если выражение содержит доступ к массиву через []; иначе false.</returns>
        private bool IsArrayAccess(string expression)
        {
            return expression.Contains("[") && expression.Contains("]");
        }

        /// <summary>
        /// Проверяет, является ли выражение доступом к свойству объекта.
        /// </summary>
        /// <param name="expression">Выражение для проверки.</param>
        /// <returns>true, если выражение содержит доступ к свойству через точку; иначе false.</returns>
        private bool IsPropertyAccess(string expression)
        {
            return expression.Contains(".") && !expression.Contains("[");
        }

        /// <summary>
        /// Устанавливает значение элемента массива.
        /// </summary>
        /// <param name="arrayAccess">Выражение доступа к массиву (например, "arr[index]").</param>
        /// <param name="value">Значение для установки.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если выражение доступа к массиву некорректно.
        /// </exception>
        /// <remarks>
        /// Парсит выражение для получения имени массива и индекса,
        /// вычисляет индекс, получает массив и устанавливает значение.
        /// Особый случай: если имя массива "struct", обновляет состояние структуры.
        /// </remarks>
        private void SetArrayElement(string arrayAccess, IVariableValue value, ExecutionContext context)
        {
            var pattern = @"^([a-zA-Z_][a-zA-Z0-9_]*)\[(.+)\]$";
            var match = Regex.Match(arrayAccess, pattern);

            if (!match.Success)
                throw new ArgumentException($"Некорректный доступ к массиву: {arrayAccess}");

            string arrayName = match.Groups[1].Value;
            string indexExpression = match.Groups[2].Value;

            var index = EvaluateExpression(indexExpression, context);

            var arrayValue = context.Variables.Get(arrayName);

            ArrayValue array = context.Variables.Get(arrayName) as ArrayValue;

            if (array == null)
                array = arrayValue.GetProperty("values") as ArrayValue;


            IVariableValue[] args = [index, value];

            array.CallMethod("set", args);

            if (arrayName == "struct")
                FromArrayValue(array, context);

            context.Variables.Set(arrayName, array);
        }

        /// <summary>
        /// Обновляет состояние ArrayStructure из ArrayValue через конвертер.
        /// </summary>
        /// <param name="arrayValue">Значение массива для конвертации.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если arrayValue равен null.
        /// </exception>
        /// <remarks>
        /// Используется для синхронизации состояния структуры данных с значением массива.
        /// </remarks>
        public void FromArrayValue(ArrayValue arrayValue, ExecutionContext context)
        {
            if (arrayValue == null)
                throw new ArgumentNullException(nameof(arrayValue));

            // Создаем ObjectValue с данными массива
            var obj = new ObjectValue(new Dictionary<string, IVariableValue>
            {
                ["values"] = arrayValue
            });

            var converter = new UniversalStructureConverter();
            var newStructure = converter.ConvertFromVariableValue(obj, "array");

            if (newStructure is ArrayStructure newArrayStructure)
            {
                // Копируем состояние из нового ArrayStructure
                context.Structure.ApplyState(newArrayStructure.GetState());
            }
        }

        /// <summary>
        /// Устанавливает значение свойства объекта.
        /// </summary>
        /// <param name="propertyAccess">Выражение доступа к свойству (например, "obj.property").</param>
        /// <param name="value">Значение для установки.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <remarks>
        /// Обрабатывает как простые (obj.property), так и вложенные (obj.prop1.prop2) свойства.
        /// </remarks>
        private void SetProperty(string propertyAccess, IVariableValue value, ExecutionContext context)
        {
            Console.WriteLine($"🔍 SetProperty: {propertyAccess} = {value}");

            // Разбираем путь к свойству: obj.prop1.prop2
            var parts = propertyAccess.Split('.');

            if (parts.Length == 1)
            {
                // Простой случай: obj.property
                context.Variables.SetNested(propertyAccess, value);
            }
            else
            {
                // Сложный случай: obj.prop1.prop2 - используем рекурсивную установку
                context.Variables.Set(propertyAccess, value);
            }
        }

        /// <summary>
        /// Извлекает отображаемое значение из IVariableValue для логов и визуализации.
        /// </summary>
        /// <param name="value">Значение для форматирования.</param>
        /// <returns>Отформатированное строковое представление значения.</returns>
        /// <remarks>
        /// Специально обрабатывает объекты и массивы для более информативного отображения.
        /// </remarks>
        private object ExtractDisplayValue(IVariableValue value)
        {
            // Для отображения в логах и визуализации
            if (value is VariableValue variableValue)
            {
                if (variableValue.Type == VariableType.Object)
                {
                    return value.ToValueString();
                }
                else if (variableValue.Type == VariableType.Array)
                {
                    return $"[Array({variableValue.ToValueString()})]";
                }
                return variableValue.ToValueString();
            }
            else if (value is Dictionary<string, VariableValue> dict)
            {
                return $"[Object({dict.Count} properties)]";
            }

            return value;
        }
    }
}