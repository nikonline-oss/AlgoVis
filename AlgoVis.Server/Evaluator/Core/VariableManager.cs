using AlgoVis.Evaluator.Evaluator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Core
{
    /// <summary>
    /// Менеджер для создания областей видимости переменных.
    /// Реализует интерфейс <see cref="IVariableManager"/>.
    /// </summary>
    /// <remarks>
    /// Этот класс предоставляет методы для создания новых областей видимости переменных,
    /// включая дочерние области с наследованием переменных от родительской области.
    /// </remarks>
    public class VariableManager : IVariableManager
    {
        /// <summary>
        /// Создает новую корневую область видимости переменных.
        /// </summary>
        /// <returns>Новая область видимости переменных.</returns>
        public IVariableScope CreateScope()
        {
            return new VariableScope();
        }

        /// <summary>
        /// Создает дочернюю область видимости с указанным родителем.
        /// </summary>
        /// <param name="parent">Родительская область видимости. Переменные из родительской
        /// области будут доступны в дочерней области, если они не переопределены локально.</param>
        /// <returns>Новая дочерняя область видимости переменных.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="parent"/> равен null.</exception>
        public IVariableScope CreateChildScope(VariableScope parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent), "Родительская область не может быть null");

            return new VariableScope(parent);
        }
    }
}