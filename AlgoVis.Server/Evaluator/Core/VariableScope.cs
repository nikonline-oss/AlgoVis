using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.Core
{
    /// <summary>
    /// Область видимости переменных, реализующая иерархическую систему с наследованием.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Этот класс управляет хранением и доступом к переменным в рамках определенной области видимости.
    /// Поддерживает иерархическую структуру, где дочерние области могут наследовать переменные от родительских.
    /// </para>
    /// <para>
    /// При поиске переменной сначала проверяется локальная область, затем - родительская (если она существует).
    /// Это позволяет реализовать механизм затенения (shadowing) переменных.
    /// </para>
    /// </remarks>
    public class VariableScope : IVariableScope
    {
        private readonly Dictionary<string, IVariableValue> _variables = new();
        private readonly IVariableScope _parent;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="VariableScope"/>.
        /// </summary>
        /// <param name="parent">Родительская область видимости. Если null, создается корневая область.</param>
        public VariableScope(IVariableScope parent = null)
        {
            _parent = parent;
        }

        /// <summary>
        /// Получает значение переменной по имени.
        /// </summary>
        /// <param name="name">Имя переменной для поиска.</param>
        /// <returns>Значение переменной. Если переменная не найдена в текущей или родительских областях,
        /// возвращается значение по умолчанию (IntValue(0)).</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="name"/> равен null или пустой строке.</exception>
        public IVariableValue Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя переменной не может быть null или пустым", nameof(name));

            if (_variables.ContainsKey(name))
                return _variables[name];

            return _parent?.Get(name) ?? new IntValue(0);
        }

        /// <summary>
        /// Устанавливает значение переменной в текущей области видимости.
        /// </summary>
        /// <param name="name">Имя переменной для установки.</param>
        /// <param name="value">Значение переменной. Если null, будет установлено значение по умолчанию (IntValue(0)).</param>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="name"/> равен null или пустой строке.</exception>
        public void Set(string name, IVariableValue value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя переменной не может быть null или пустым", nameof(name));

            _variables[name] = value ?? new IntValue(0);
        }

        /// <summary>
        /// Проверяет, существует ли переменная в текущей области видимости.
        /// </summary>
        /// <param name="name">Имя переменной для проверки.</param>
        /// <returns>true, если переменная существует в текущей области; иначе false.</returns>
        /// <remarks>Этот метод не проверяет родительские области видимости.</remarks>
        public bool Contains(string name) => _variables.ContainsKey(name);

        /// <summary>
        /// Получает вложенное значение по пути, разделенному точками.
        /// </summary>
        /// <param name="path">Путь к свойству в формате "object.property.nestedProperty".</param>
        /// <returns>Вложенное значение переменной.</returns>
        /// <example>
        /// <code>
        /// // Предположим, есть объект user с свойством address, которое содержит city
        /// var city = scope.GetNested("user.address.city");
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="path"/> равен null.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="path"/> пустой или содержит только точки.</exception>
        public IVariableValue GetNested(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "Путь не может быть null");

            var parts = path.Split('.');
            if (parts.Length == 0 || parts.All(string.IsNullOrEmpty))
                throw new ArgumentException("Некорректный путь", nameof(path));

            IVariableValue current = Get(parts[0]);

            for (int i = 1; i < parts.Length; i++)
            {
                current = current.GetProperty(parts[i]);
            }

            return current;
        }

        /// <summary>
        /// Устанавливает вложенное значение по пути, разделенному точками.
        /// </summary>
        /// <param name="path">Путь к свойству в формате "object.property.nestedProperty".</param>
        /// <param name="value">Значение для установки.</param>
        /// <remarks>
        /// Если корневой объект не существует или не является объектом, он будет создан как <see cref="ObjectValue"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Установка значения для user.address.city
        /// scope.SetNested("user.address.city", new StringValue("Москва"));
        /// </code>
        /// </example>
        public void SetNested(string path, IVariableValue value)
        {
            var parts = path.Split('.');

            if (parts.Length == 1)
            {
                Set(parts[0], value);
                return;
            }

            // Получаем или создаем корневой объект
            var rootName = parts[0];
            var root = Get(rootName);

            if (root is not ObjectValue rootObj)
            {
                rootObj = new ObjectValue();
                Set(rootName, rootObj);
            }

            // Устанавливаем вложенное свойство
            var remainingPath = string.Join(".", parts.Skip(1));
            rootObj.SetNestedProperty(remainingPath, value);
        }

        /// <summary>
        /// Создает глубокую копию текущего состояния всех переменных в области видимости.
        /// </summary>
        /// <returns>Словарь, содержащий копию всех переменных и их значений.</returns>
        /// <remarks>
        /// Использует JSON сериализацию для создания глубокой копии, гарантируя,
        /// что изменения в возвращаемом словаре не повлияют на исходные данные.
        /// </remarks>
        public Dictionary<string, object> GetSnapshot()
        {
            var allVars = GetAllVariables();
            return DeepCopyDictionary(allVars);
        }

        /// <summary>
        /// Создает глубокую копию словаря с использованием JSON сериализации.
        /// </summary>
        /// <param name="original">Исходный словарь для копирования.</param>
        /// <returns>Глубокая копия исходного словаря.</returns>
        private Dictionary<string, object> DeepCopyDictionary(Dictionary<string, object> original)
        {
            if (original == null) return new Dictionary<string, object>();

            var json = JsonSerializer.Serialize(original);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }

        /// <summary>
        /// Получает все переменные из текущей и всех родительских областей видимости.
        /// </summary>
        /// <returns>Словарь, содержащий все доступные переменные.</returns>
        /// <remarks>
        /// Переменные из текущей области переопределяют переменные с тем же именем из родительских областей.
        /// </remarks>
        public Dictionary<string, object> GetAllVariables()
        {
            var result = new Dictionary<string, object>();

            if (_parent != null)
            {
                foreach (var variable in _parent.GetAllVariables())
                {
                    result[variable.Key] = variable.Value;
                }
            }

            foreach (var variable in _variables)
            {
                result[variable.Key] = variable.Value.RawValue;
            }

            return result;
        }
    }
}