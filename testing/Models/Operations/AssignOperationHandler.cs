using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using testing.Models.Custom;
using testing.Models.Evaluator;
using ExecutionContext = testing.Models.DataStructures.ExecutionContext;


namespace testing.Models.Operations
{
    public class AssignOperationHandler : BaseOperationHandler
    {
        public override void Execute(AlgorithmStep step, ExecutionContext context)
        {
            if (step.parameters.Count < 2)
                throw new ArgumentException("Assign operation requires 2 parameters");

            var leftSide = step.parameters[0];
            var rightExpression = step.parameters[1];
            var value = EvaluateExpression(rightExpression, context);
            var extractedValue = ExtractValue(value);

            if (IsArrayAccess(leftSide))
            {
                SetArrayElement(leftSide, extractedValue, context);
            }
            else if (IsPropertyAccess(leftSide))
            {
                SetProperty(leftSide, extractedValue, context);
            }
            else
            {
                context.Variables.Set(leftSide, extractedValue);
            }

            AddVisualizationStep(step, context, "assign",
                step.description ?? $"Присвоение {leftSide} = {extractedValue}",
                metadata: new Dictionary<string, object>
                {
                    ["variable"] = leftSide,
                    ["value"] = extractedValue,
                    ["expression"] = rightExpression
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

        private void SetArrayElement(string arrayAccess, object value, ExecutionContext context)
        {
            // Парсим выражение доступа к массиву: array[index]
            var pattern = @"^([a-zA-Z_][a-zA-Z0-9_]*)\[(.+)\]$";
            var match = Regex.Match(arrayAccess, pattern);

            if (!match.Success)
                throw new ArgumentException($"Некорректный доступ к массиву: {arrayAccess}");

            string arrayName = match.Groups[1].Value;
            string indexExpression = match.Groups[2].Value;

            // Вычисляем индекс
            var indexResult = EvaluateExpression(indexExpression, context);
            int index = Convert.ToInt32(ExtractValue(indexResult));

            // Устанавливаем элемент массива
            context.Variables.SetElement(arrayName, index, value);
        }
        private void SetProperty(string propertyAccess, object value, ExecutionContext context)
        {
            // Простой случай: obj.property
            var parts = propertyAccess.Split('.');
            if (parts.Length == 2)
            {
                context.Variables.SetProperty(parts[0], parts[1], value);
            }
            else
            {
                // Сложный случай: obj.subobj.property - используем обычный Set
                context.Variables.Set(propertyAccess, value);
            }
        }
    }
}
