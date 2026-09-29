using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Operations.Base;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = AlgoVis.Models.Models.DataStructures.ExecutionContext;

namespace AlgoVis.Models.Models.Operations.Handlers
{
    /// <summary>
    /// Обработчик операции сравнения значений.
    /// </summary>
    /// <remarks>
    /// Сравнивает два значения и сохраняет результат в переменной "last_comparison".
    /// Результат сравнения: 
    /// -1: первое значение меньше второго
    ///  0: значения равны
    ///  1: первое значение больше второго
    /// </remarks>
    public class CompareOperationHandler : BaseOperationHandler
    {
        /// <summary>
        /// Выполняет операцию сравнения двух значений.
        /// </summary>
        /// <param name="step">Шаг алгоритма с параметрами операции.</param>
        /// <param name="context">Контекст выполнения алгоритма.</param>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если количество параметров меньше 2.
        /// </exception>
        /// <remarks>
        /// Увеличивает счетчик сравнений в статистике.
        /// Параметры шага могут быть в двух форматах:
        /// - 2 параметра: индексы для сравнения (используется массив по умолчанию "struct")
        /// - 3 параметра: имя массива, первый индекс, второй индекс
        /// </remarks>
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            context.Statistics.Comparisons++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Compare operation requires 2 parameters");

            string arrayName = step.parameters.Count > 2 ? step.parameters[0] : "struct";
            var index1 = EvaluateExpression(step.parameters[step.parameters.Count > 2 ? 1 : 0].ToLower(), context);
            var index2 = EvaluateExpression(step.parameters[step.parameters.Count > 2 ? 2 : 1].ToLower(), context);

            // Получаем массив из переменных
            var arrayValue = context.Variables.Get(arrayName) as ArrayValue;

            if (context.Variables.Get(arrayName).HasProperty("values"))
                arrayValue = context.Variables.Get(arrayName).GetProperty("values") as ArrayValue;

            var comparisonResult = CompareValues(index1, index2);

            context.Variables.Set("last_comparison", new IntValue(comparisonResult));

            AddVisualizationStep(step, context, "compare",
                step.description ?? $"Сравнение {index1} и {index2}",
                new List<HighlightedElement>
                {
                },
                new Dictionary<string, object>
                {
                    ["array_name"] = arrayName,
                    ["index1"] = index1,
                    ["index2"] = index2,
                    ["comparison_result"] = comparisonResult
                });

            ExecuteNextStep(step, context);
        }

        // Примеры использования операции сравнения:
        // "5 > 3" // возвращает 1
        // "2.5 == 2.5" // возвращает 0
        // "10 < 5" // возвращает -1
        // "'apple' < 'banana'" // возвращает -1
        // "'hello' == 'hello'" // возвращает 0
        // "arr1.length > arr2.length" // сравнение длин массивов
        // "arr1[0] == arr2[0]" // сравнение элементов массивов
        // "'5' > 3" // строка '5' преобразуется в число 5, возвращает 1

        /// <summary>
        /// Сравнивает два значения и возвращает результат сравнения.
        /// </summary>
        /// <param name="value1">Первое значение для сравнения.</param>
        /// <param name="value2">Второе значение для сравнения.</param>
        /// <returns>
        /// Целое число, представляющее результат сравнения:
        /// -1: value1 < value2
        ///  0: value1 == value2
        ///  1: value1 > value2
        /// </returns>
        /// <remarks>
        /// Поддерживает сравнение чисел, строк, булевых значений и массивов.
        /// Для массивов сравнивает их длины.
        /// При необходимости выполняет автоматическое преобразование типов.
        /// </remarks>
        private int CompareValues(IVariableValue value1, IVariableValue value2)
        {
            // Для чисел - численное сравнение
            if (IsNumeric(value1) && IsNumeric(value2))
            {
                var num1 = value1.ToDouble();
                var num2 = value2.ToDouble();
                return num1.CompareTo(num2);
            }

            // Для строк - строковое сравнение
            if (value1.Type == VariableType.String && value2.Type == VariableType.String)
            {
                return string.Compare(value1.ToString(), value2.ToString(), StringComparison.Ordinal);
            }

            // Для булевых значений
            if (value1.Type == VariableType.Bool && value2.Type == VariableType.Bool)
            {
                var bool1 = value1.ToBool();
                var bool2 = value2.ToBool();
                return bool1.CompareTo(bool2);
            }

            // Для массивов - сравнение длин
            if (value1 is ArrayValue array1 && value2 is ArrayValue array2)
            {
                return array1.Length.CompareTo(array2.Length);
            }

            // Попытка преобразовать к числам
            try
            {
                var num1 = value1.ToDouble();
                var num2 = value2.ToDouble();
                return num1.CompareTo(num2);
            }
            catch
            {
                // Если не удалось, сравниваем как строки
                return string.Compare(value1.ToString(), value2.ToString(), StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Проверяет, является ли значение числовым.
        /// </summary>
        /// <param name="value">Значение для проверки.</param>
        /// <returns>true, если значение типа Int или Double; иначе false.</returns>
        private bool IsNumeric(IVariableValue value)
        {
            return value.Type == VariableType.Int || value.Type == VariableType.Double;
        }
    }
}