using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using AlgoVis.Models.Models.Custom;
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
    public class AssignOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            if (step.parameters.Count < 2)
                throw new ArgumentException("Assign operation requires 2 parameters");

            var leftSide = step.parameters[0];
            var rightExpression = step.parameters[1];
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

        private bool IsArrayAccess(string expression)
        {
            return expression.Contains("[") && expression.Contains("]");
        }

        private bool IsPropertyAccess(string expression)
        {
            return expression.Contains(".") && !expression.Contains("[");
        }

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

            array.CallMethod("set",args);


            context.Variables.Set(arrayName, array);
        }

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
