using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    public class MemberAccessNode : IExpressionNode
    {
        private readonly IExpressionNode _objectNode;
        public readonly string _propertyName;

        public MemberAccessNode(IExpressionNode objectNode, string propertyName)
        {
            _objectNode = objectNode;
            _propertyName = propertyName;
        }

        public object Evaluate(IVariableScope variables)
        {
            try
            {
                // Получаем базовое имя переменной
                string baseName = GetBaseVariableName();
                string fullPath = $"{baseName}.{_propertyName}";

                Console.WriteLine($"🔍 MemberAccess: полный путь = {fullPath}");

                // Пробуем получить значение через VariableScope
                var result = variables.Get(fullPath);

                Console.WriteLine($"🔍 MemberAccess результат: {result}, тип = {result?.GetType()}");

                // Если результат null, возвращаем null
                if (result == null)
                {
                    Console.WriteLine($"🔍 MemberAccess: возвращаем null");
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ MemberAccess ошибка: {ex.Message}");
                return 0;
            }
        }

        public string GetBaseVariableName()
        {
            if (_objectNode is VariableNode variableNode)
            {
                return variableNode.Name;
            }
            else if (_objectNode is MemberAccessNode memberAccess)
            {
                return $"{memberAccess.GetBaseVariableName()}.{memberAccess._propertyName}";
            }

            throw new InvalidOperationException("Не удается определить имя базовой переменной");
        }
    }
}
