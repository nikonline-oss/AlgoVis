using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Nodes
{
    /// <summary>
    /// Представляет узел доступа к члену (полю или свойству) объекта.
    /// </summary>
    /// <remarks>
    /// Этот класс реализует доступ к члену (<c>memberName</c>) целевого объекта,
    /// представленного выражением <c>target</c>. Например, выражение <c>obj.Property</c>
    /// будет представлено как MemberAccessNode с target = obj и memberName = "Property".
    /// </remarks>
    /// <seealso cref="IExpressionNode"/>
    public class MemberAccessNode : IExpressionNode
    {
        private readonly IExpressionNode _target;
        private readonly string _memberName;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MemberAccessNode"/>.
        /// </summary>
        /// <param name="target">Целевой объект, к члену которого осуществляется доступ.</param>
        /// <param name="memberName">Имя члена (поля или свойства) для доступа.</param>
        /// <exception cref="ArgumentNullException">
        /// Вызывается, если <paramref name="target"/> или <paramref name="memberName"/> равны null.
        /// </exception>
        public MemberAccessNode(IExpressionNode target, string memberName)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _memberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
        }

        /// <summary>
        /// Выполняет вычисление значения члена целевого объекта.
        /// </summary>
        /// <param name="variables">Область видимости переменных для вычисления выражения.</param>
        /// <returns>Значение члена <see cref="_memberName"/> целевого объекта.</returns>
        /// <exception cref="InvalidOperationException">
        /// Может быть вызвано, если целевой объект не поддерживает доступ к члену с указанным именем.
        /// </exception>
        /// <remarks>
        /// Метод сначала вычисляет значение целевого выражения, затем получает значение
        /// указанного члена через метод <see cref="IVariableValue.GetProperty(string)"/>.
        /// </remarks>
        public IVariableValue Evaluate(IVariableScope variables)
        {
            var targetValue = _target.Evaluate(variables);
            return targetValue.GetProperty(_memberName);
        }

        /// <summary>
        /// Возвращает строковое представление узла доступа к члену.
        /// </summary>
        /// <returns>
        /// Строка в формате "target.memberName", где target - строковое представление
        /// целевого выражения, а memberName - имя члена.
        /// </returns>
        public override string ToString() => $"{_target}.{_memberName}";
    }
}