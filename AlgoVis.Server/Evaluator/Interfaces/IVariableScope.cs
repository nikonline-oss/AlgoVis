using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Interfaces
{
    /// <summary>
    /// Представляет область видимости для хранения и доступа к переменным.
    /// </summary>
    /// <remarks>
    /// Область видимости обеспечивает доступ к переменным по имени, поддерживает
    /// вложенную структуру и позволяет получать снимки текущего состояния переменных.
    /// </remarks>
    public interface IVariableScope
    {
        /// <summary>
        /// Получает значение переменной по имени.
        /// </summary>
        /// <param name="name">Имя переменной.</param>
        /// <returns>Значение переменной.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Выбрасывается, если переменная с указанным именем не существует в текущей области.
        /// </exception>
        IVariableValue Get(string name);

        /// <summary>
        /// Устанавливает значение переменной по имени.
        /// </summary>
        /// <param name="name">Имя переменной.</param>
        /// <param name="value">Значение переменной.</param>
        /// <remarks>
        /// Если переменная с указанным именем уже существует, ее значение будет перезаписано.
        /// </remarks>
        void Set(string name, IVariableValue value);

        /// <summary>
        /// Проверяет существование переменной в текущей области.
        /// </summary>
        /// <param name="name">Имя переменной для проверки.</param>
        /// <returns>
        /// <c>true</c> - если переменная существует в текущей области; 
        /// <c>false</c> - в противном случае.
        /// </returns>
        bool Contains(string name);

        /// <summary>
        /// Получает вложенное значение по пути, разделенному точками.
        /// </summary>
        /// <param name="path">
        /// Путь к значению в формате "object.property.subproperty".
        /// </param>
        /// <returns>Значение по указанному пути.</returns>
        /// <remarks>
        /// Метод последовательно проходит по цепочке свойств объектов,
        /// начиная с текущей области видимости.
        /// </remarks>
        IVariableValue GetNested(string path);

        /// <summary>
        /// Устанавливает вложенное значение по пути, разделенному точками.
        /// </summary>
        /// <param name="path">
        /// Путь к значению в формате "object.property.subproperty".
        /// </param>
        /// <param name="value">Значение для установки.</param>
        /// <remarks>
        /// Если промежуточные объекты в пути не существуют, они будут созданы.
        /// </remarks>
        void SetNested(string path, IVariableValue value);

        /// <summary>
        /// Получает все переменные из текущей области видимости.
        /// </summary>
        /// <returns>
        /// Словарь, содержащий все переменные текущей области.
        /// </returns>
        /// <remarks>
        /// Метод предназначен для обратной совместимости и будет постепенно удален.
        /// Используйте методы <see cref="Get"/> и <see cref="Contains"/> вместо него.
        /// </remarks>
        Dictionary<string, object> GetAllVariables();

        /// <summary>
        /// Создает снимок текущего состояния переменных.
        /// </summary>
        /// <returns>
        /// Словарь, содержащий копию всех переменных текущей области на момент вызова.
        /// </returns>
        /// <remarks>
        /// Снимок может использоваться для отладки, сериализации или создания резервных копий.
        /// </remarks>
        Dictionary<string, object> GetSnapshot();
    }
}