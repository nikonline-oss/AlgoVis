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
    // Операция сравнения
    public class CompareOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            context.Statistics.Comparisons++;

            if (step.parameters.Count < 2)
                throw new ArgumentException("Compare operation requires 2 parameters");

            string arrayName = step.parameters.Count > 2 ? step.parameters[0] : "array";
            var value1 = EvaluateExpression(step.parameters[step.parameters.Count > 2 ? 1 : 0], context);
            var value2 = EvaluateExpression(step.parameters[step.parameters.Count > 2 ? 2 : 1], context);

            // Получаем массив из переменных
            var arrayValue = context.Variables.Get(arrayName) as ArrayValue;

            if (context.Variables.Get(arrayName).HasProperty("values"))
                arrayValue = context.Variables.Get(arrayName).GetProperty("values") as ArrayValue;

            IVariableValue[] args1 = [value1];
            IVariableValue[] args2 = [value2];

            var comparisonResult = CompareValues(arrayValue.CallMethod("get", args1), arrayValue.CallMethod("get", args2));

            context.Variables.Set("last_comparison", new IntValue(comparisonResult));

            AddVisualizationStep(step, context, "compare",
                step.description ?? $"Сравнение {value1} и {value2}",
                new List<HighlightedElement>
                {
                },
                new Dictionary<string, object>
                {
                    ["value1"] = value1,
                    ["value2"] = value2,
                    ["comparison_result"] = comparisonResult
                });

            ExecuteNextStep(step, context);
        }

        //Сравнение чисел
        //"5 > 3" // возвращает 1
        //"2.5 == 2.5" // возвращает 0
        //"10 < 5" // возвращает -1

        //// Сравнение строк
        //"'apple' < 'banana'" // возвращает -1
        //"'hello' == 'hello'" // возвращает 0

        //// Сравнение массивов
        //"arr1.length > arr2.length" // сравнение длин массивов
        //"arr1[0] == arr2[0]" // сравнение элементов массивов

        //// Сравнение с автоматическим преобразованием
        //"'5' > 3" // строка '5' преобразуется в число 5, возвращает 1
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

        private bool IsNumeric(IVariableValue value)
        {
            return value.Type == VariableType.Int || value.Type == VariableType.Double;
        }
    }
}
