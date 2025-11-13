using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.Operations.Base;
using System;
using System.Collections.Generic;
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
            var value = EvaluateExpression(rightExpression, context);

            Console.WriteLine($"🔍 Assign: {leftSide} = {value} (тип: {value?.GetType()})");

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

        private void SetArrayElement(string arrayAccess, object value, ExecutionContext context)
        {
            var pattern = @"^([a-zA-Z_][a-zA-Z0-9_]*)\[(.+)\]$";
            var match = Regex.Match(arrayAccess, pattern);

            if (!match.Success)
                throw new ArgumentException($"Некорректный доступ к массиву: {arrayAccess}");

            string arrayName = match.Groups[1].Value;
            string indexExpression = match.Groups[2].Value;

            var indexResult = EvaluateExpression(indexExpression, context);
            int index = Convert.ToInt32(ExtractValue(indexResult));

            context.Variables.SetElement(arrayName, index, value);
        }

        private void SetProperty(string propertyAccess, object value, ExecutionContext context)
        {
            Console.WriteLine($"🔍 SetProperty: {propertyAccess} = {value}");

            // Разбираем путь к свойству: obj.prop1.prop2
            var parts = propertyAccess.Split('.');

            if (parts.Length == 2)
            {
                // Простой случай: obj.property
                context.Variables.SetProperty(parts[0], parts[1], value);
            }
            else
            {
                // Сложный случай: obj.prop1.prop2 - используем рекурсивную установку
                SetNestedProperty(context.Variables, parts, value, 0);
            }
        }

        private void SetNestedProperty(IVariableScope variables, string[] path, object value, int depth)
        {
            if (depth >= path.Length - 1)
            {
                // Достигли конечного свойства - устанавливаем значение
                variables.SetProperty(path[depth - 1], path[depth], value);
                return;
            }

            // Получаем текущий объект
            var currentObj = variables.Get(string.Join(".", path.Take(depth + 1)));

            if (currentObj is VariableValue variableValue && variableValue.Type == VariableType.Object)
            {
                // Продолжаем углубляться
                SetNestedProperty(variables, path, value, depth + 1);
            }
            else
            {
                // Создаем промежуточные объекты
                CreateNestedObject(variables, path, value, depth);
            }
        }

        private void CreateNestedObject(IVariableScope variables, string[] path, object value, int depth)
        {
            // Создаем цепочку объектов
            var currentPath = new List<string>();

            for (int i = 0; i <= depth; i++)
            {
                currentPath.Add(path[i]);

                if (i == path.Length - 1)
                {
                    // Последний элемент - устанавливаем значение
                    variables.SetProperty(
                        string.Join(".", currentPath.Take(currentPath.Count - 1)),
                        path[i],
                        value
                    );
                }
                else
                {
                    // Промежуточный объект - создаем пустой объект
                    var obj = new Dictionary<string, VariableValue>();
                    variables.SetProperty(
                        string.Join(".", currentPath.Take(currentPath.Count - 1)),
                        path[i],
                        new VariableValue(obj)
                    );
                }
            }
        }

        private object ExtractDisplayValue(object value)
        {
            // Для отображения в логах и визуализации
            if (value is VariableValue variableValue)
            {
                if (variableValue.Type == VariableType.Object)
                {
                    return "[Object]";
                }
                else if (variableValue.Type == VariableType.Array)
                {
                    return $"[Array({variableValue.ArrayValue.Count})]";
                }
                return variableValue.Value;
            }
            else if (value is Dictionary<string, VariableValue> dict)
            {
                return $"[Object({dict.Count} properties)]";
            }

            return value;
        }
    }
}
