//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using testing.Models.Custom;
//using testing.Models.Evaluator;

//namespace testing.Models.Operations
//{
//    public class ObjectOperationHandler : BaseOperationHandler
//    {
//        public override void Execute(AlgorithmStep step, ExecutionContext context)
//        {
//            switch (step.operationSubType?.ToLower())
//            {
//                case "create":
//                    CreateObject(step, context);
//                    break;
//                case "set_property":
//                    SetProperty(step, context);
//                    break;
//                case "get_property":
//                    GetProperty(step, context);
//                    break;
//                default:
//                    throw new ArgumentException($"Неизвестная операция с объектом: {step.operationSubType}");
//            }
//        }

//        private void CreateObject(AlgorithmStep step, ExecutionContext context)
//        {
//            var objectName = step.parameters[0];
//            var properties = new Dictionary<string, VariableValue>();

//            // Инициализация свойствами из параметров
//            for (int i = 1; i < step.parameters.Count; i += 2)
//            {
//                if (i + 1 < step.parameters.Count)
//                {
//                    var propName = step.parameters[i];
//                    var propValue = EvaluateExpression(step.parameters[i + 1], context);
//                    properties[propName] = propValue as VariableValue ?? new VariableValue(propValue);
//                }
//            }

//            context.Variables.Set(objectName, new VariableValue(properties));

//            AddVisualizationStep(step, context, "object_create",
//                $"Создание объекта '{objectName}'");
//        }

//        private void SetProperty(AlgorithmStep step, ExecutionContext context)
//        {
//            var objectName = step.parameters[0];
//            var propertyPath = step.parameters[1];
//            var value = EvaluateExpression(step.parameters[2], context);

//            // Поддержка вложенных свойств: obj.subobj.property
//            var propertyParts = propertyPath.Split('.');

//            if (propertyParts.Length == 1)
//            {
//                // Простое свойство
//                context.Variables.SetProperty(objectName, propertyPath, value);
//            }
//            else
//            {
//                // Вложенные свойства - нужно получить родительский объект
//                var parentObjectName = objectName;
//                for (int i = 0; i < propertyParts.Length - 1; i++)
//                {
//                    parentObjectName = $"{parentObjectName}.{propertyParts[i]}";
//                }
//                context.Variables.SetProperty(parentObjectName, propertyParts[^1], value);
//            }

//            AddVisualizationStep(step, context, "object_set_property",
//                $"Установка свойства '{propertyPath}' = {ExtractValue(value)}");
//        }
//    }
//}
