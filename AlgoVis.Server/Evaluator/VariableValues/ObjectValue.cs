using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues
{
    /// <summary>
    /// Представляет объектное значение со свойствами.
    /// </summary>
    /// <remarks>
    /// Этот класс реализует объектный тип со словарем свойств. Поддерживает вложенные свойства
    /// через точечную нотацию и предоставляет методы для работы с объектами, аналогичные JavaScript.
    /// </remarks>
    public class ObjectValue : VariableValue
    {
        private readonly Dictionary<string, IVariableValue> _properties;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ObjectValue"/>.
        /// </summary>
        /// <param name="properties">
        /// Начальные свойства объекта. Если <c>null</c>, создается пустой словарь.
        /// </param>
        public ObjectValue(Dictionary<string, IVariableValue> properties = null)
        {
            _properties = properties ?? new Dictionary<string, IVariableValue>();
        }

        /// <summary>
        /// Получает тип значения - <see cref="VariableType.Object"/>.
        /// </summary>
        public override VariableType Type => VariableType.Object;

        /// <summary>
        /// Получает необработанное значение в виде словаря свойств.
        /// </summary>
        public override object RawValue => _properties;

        /// <summary>
        /// Определяет, содержит ли объект свойство с указанным именем.
        /// </summary>
        /// <param name="name">Имя свойства для проверки.</param>
        /// <returns>
        /// <c>true</c>, если свойство существует; в противном случае <c>false</c>.
        /// </returns>
        public override bool HasProperty(string name) => _properties.ContainsKey(name);

        /// <summary>
        /// Получает значение свойства по имени.
        /// </summary>
        /// <param name="name">Имя свойства для получения.</param>
        /// <returns>
        /// Значение свойства. Если свойство не существует, создается новое свойство 
        /// со значением <see cref="NullValue"/> и возвращается.
        /// </returns>
        public override IVariableValue GetProperty(string name)
        {
            if (_properties.TryGetValue(name, out var value))
                return value;

            // Автоматическое создание свойства при обращении
            var newValue = new NullValue();
            _properties[name] = newValue;
            return newValue;
        }

        /// <summary>
        /// Устанавливает значение свойства.
        /// </summary>
        /// <param name="name">Имя свойства для установки.</param>
        /// <param name="value">Значение свойства. Если <c>null</c>, заменяется на <see cref="NullValue"/>.</param>
        public override void SetProperty(string name, IVariableValue value)
        {
            _properties[name] = value ?? new NullValue();
        }

        /// <summary>
        /// Вызывает метод объекта.
        /// </summary>
        /// <param name="methodName">Имя вызываемого метода (без учета регистра).</param>
        /// <param name="arguments">Аргументы метода.</param>
        /// <returns>
        /// Результат выполнения метода или исключение, если метод не поддерживается.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, когда метод не поддерживается.
        /// </exception>
        public override IVariableValue CallMethod(string methodName, IVariableValue[] arguments)
        {
            return methodName.ToLower() switch
            {
                "keys" => GetKeys(),
                "values" => GetValues(),
                "has" => HasPropertyMethod(arguments),
                "get" => GetPropertyMethod(arguments),
                "set" => SetPropertyMethod(arguments),
                "remove" => RemoveProperty(arguments),
                "toString" => ToStringMethod(),
                "toJSON" => ToJsonMethod(),
                _ => base.CallMethod(methodName, arguments)
            };
        }

        /// <summary>
        /// Возвращает массив ключей свойств объекта.
        /// </summary>
        /// <returns>Массив строковых значений, содержащий ключи свойств.</returns>
        private IVariableValue GetKeys()
        {
            var keys = _properties.Keys.Select(k => new StringValue(k)).ToArray();
            return new ArrayValue(keys);
        }

        /// <summary>
        /// Возвращает массив значений свойств объекта.
        /// </summary>
        /// <returns>Массив значений свойств.</returns>
        private IVariableValue GetValues()
        {
            return new ArrayValue(_properties.Values.ToList());
        }

        /// <summary>
        /// Проверяет наличие свойства (метод 'has').
        /// </summary>
        /// <param name="args">Аргументы: [propertyName].</param>
        /// <returns>
        /// <see cref="BoolValue"/> с результатом проверки существования свойства.
        /// </returns>
        private IVariableValue HasPropertyMethod(IVariableValue[] args)
        {
            if (args.Length == 0) return new BoolValue(false);
            return new BoolValue(HasProperty(args[0].ToString()));
        }

        /// <summary>
        /// Получает значение свойства (метод 'get').
        /// </summary>
        /// <param name="args">Аргументы: [propertyName].</param>
        /// <returns>
        /// Значение свойства или <see cref="NullValue"/>, если аргументы отсутствуют.
        /// </returns>
        private IVariableValue GetPropertyMethod(IVariableValue[] args)
        {
            if (args.Length == 0) return new NullValue();
            return GetProperty(args[0].ToString());
        }

        /// <summary>
        /// Устанавливает значение свойства (метод 'set').
        /// </summary>
        /// <param name="args">Аргументы: [propertyName, value].</param>
        /// <returns>
        /// <see cref="BoolValue"/> с результатом операции (<c>true</c> в случае успеха).
        /// </returns>
        private IVariableValue SetPropertyMethod(IVariableValue[] args)
        {
            if (args.Length < 2) return new BoolValue(false);
            SetProperty(args[0].ToString(), args[1]);
            return new BoolValue(true);
        }

        /// <summary>
        /// Удаляет свойство из объекта.
        /// </summary>
        /// <param name="args">Аргументы: [propertyName].</param>
        /// <returns>
        /// <see cref="BoolValue"/>, указывающее, было ли свойство удалено.
        /// </returns>
        private IVariableValue RemoveProperty(IVariableValue[] args)
        {
            if (args.Length == 0) return new BoolValue(false);
            return new BoolValue(_properties.Remove(args[0].ToString()));
        }

        /// <summary>
        /// Возвращает строковое представление объекта.
        /// </summary>
        /// <returns>Строковое представление объекта.</returns>
        private IVariableValue ToStringMethod()
        {
            return new StringValue(ToString());
        }

        /// <summary>
        /// Сериализует объект в JSON-строку.
        /// </summary>
        /// <returns>
        /// JSON-представление объекта или "{}" в случае ошибки сериализации.
        /// </returns>
        private IVariableValue ToJsonMethod()
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(ToSerializableDictionary());
                return new StringValue(json);
            }
            catch
            {
                return new StringValue("{}");
            }
        }

        /// <summary>
        /// Преобразует объект в сериализуемый словарь.
        /// </summary>
        /// <returns>Словарь, пригодный для JSON-сериализации.</returns>
        private Dictionary<string, object> ToSerializableDictionary()
        {
            var result = new Dictionary<string, object>();
            foreach (var prop in _properties)
            {
                result[prop.Key] = ConvertToSerializable(prop.Value);
            }
            return result;
        }

        /// <summary>
        /// Преобразует значение переменной в сериализуемый объект.
        /// </summary>
        /// <param name="value">Значение для преобразования.</param>
        /// <returns>Сериализуемый объект.</returns>
        private object ConvertToSerializable(IVariableValue value) => value switch
        {
            IntValue intVal => intVal.RawValue,
            DoubleValue doubleVal => doubleVal.RawValue,
            BoolValue boolVal => boolVal.RawValue,
            StringValue stringVal => stringVal.RawValue,
            NullValue => null,
            ArrayValue arrayVal => ConvertArrayToSerializable(arrayVal),
            ObjectValue objVal => objVal.ToSerializableDictionary(),
            _ => value.ToString()
        };

        /// <summary>
        /// Преобразует массив значений в сериализуемый список.
        /// </summary>
        /// <param name="array">Массив для преобразования.</param>
        /// <returns>Список сериализуемых объектов.</returns>
        private List<object> ConvertArrayToSerializable(ArrayValue array)
        {
            var result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add(ConvertToSerializable(array[i]));
            }
            return result;
        }

        /// <summary>
        /// Устанавливает вложенное свойство через точечную нотацию.
        /// </summary>
        /// <param name="path">
        /// Путь к свойству в точечной нотации (например, "user.profile.name").
        /// </param>
        /// <param name="value">Значение для установки.</param>
        /// <remarks>
        /// Создает промежуточные объекты, если они не существуют.
        /// </remarks>
        public void SetNestedProperty(string path, IVariableValue value)
        {
            var parts = path.Split('.');
            IVariableValue current = this;

            // Проходим по всем частям пути, кроме последней
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var next = current.GetProperty(parts[i]);
                if (next is not ObjectValue)
                {
                    // Если следующий элемент не объект, создаем новый объект
                    var newObj = new ObjectValue();
                    current.SetProperty(parts[i], newObj);
                    current = newObj;
                }
                else
                {
                    current = next;
                }
            }

            // Устанавливаем финальное значение
            current.SetProperty(parts[^1], value);
        }

        /// <summary>
        /// Преобразует объект в целое число (количество свойств).
        /// </summary>
        public override int ToInt() => _properties.Count;

        /// <summary>
        /// Преобразует объект в число с плавающей запятой (количество свойств).
        /// </summary>
        public override double ToDouble() => _properties.Count;

        /// <summary>
        /// Преобразует объект в логическое значение.
        /// </summary>
        /// <returns>
        /// <c>true</c>, если объект содержит хотя бы одно свойство; в противном случае <c>false</c>.
        /// </returns>
        public override bool ToBool() => _properties.Count > 0;

        /// <summary>
        /// Возвращает строковое представление объекта в формате "{key1: value1, key2: value2, ...}".
        /// </summary>
        public override string ToValueString() => $"{{{string.Join(", ", _properties.Select(kv => $"{kv.Key}: {kv.Value}"))}}}";
    }
}