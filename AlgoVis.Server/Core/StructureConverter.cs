using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Suport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core
{
    /// <summary>
    /// Универсальный конвертер структур данных в значения переменных и обратно.
    /// </summary>
    /// <remarks>
    /// Этот класс делегирует конвертацию специализированным конвертерам для каждого типа структур данных.
    /// Поддерживает регистрацию пользовательских конвертеров.
    /// </remarks>
    public class UniversalStructureConverter : IStructureConverter
    {
        private readonly Dictionary<string, IStructureConverter> _converters;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UniversalStructureConverter"/>.
        /// </summary>
        /// <remarks>
        /// Регистрирует конвертеры для стандартных типов структур данных: array, binarytree, linkedlist, graph.
        /// </remarks>
        public UniversalStructureConverter()
        {
            _converters = new Dictionary<string, IStructureConverter>
            {
                ["array"] = new ArrayStructureConverter(),
                ["binarytree"] = new BinaryTreeStructureConverter(),
                ["linkedlist"] = new LinkedListStructureConverter(),
                ["graph"] = new GraphStructureConverter()
            };
        }

        /// <summary>
        /// Конвертирует структуру данных в значение переменной.
        /// </summary>
        /// <param name="structure">Структура данных для конвертации.</param>
        /// <returns>Значение переменной, представляющее структуру.</returns>
        /// <exception cref="NotSupportedException">
        /// Выбрасывается, если тип структуры не поддерживается.
        /// </exception>
        public IVariableValue ConvertToVariableValue(IDataStructure structure)
        {
            if (structure == null)
                return new NullValue();

            var type = structure.Type.ToLower();
            if (_converters.TryGetValue(type, out var converter))
                return converter.ConvertToVariableValue(structure);

            throw new NotSupportedException($"Structure type '{type}' is not supported");
        }

        /// <summary>
        /// Конвертирует значение переменной в структуру данных.
        /// </summary>
        /// <param name="value">Значение переменной для конвертации.</param>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>Структура данных.</returns>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если значение равно null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Выбрасывается, если тип структуры не поддерживается.
        /// </exception>
        public IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var type = structureType.ToLower();
            if (_converters.TryGetValue(type, out var converter))
                return converter.ConvertFromVariableValue(value, structureType);

            throw new NotSupportedException($"Structure type '{type}' is not supported");
        }

        /// <summary>
        /// Проверяет, поддерживает ли конвертер указанный тип структуры.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>true, если тип поддерживается; иначе false.</returns>
        public bool CanConvert(string structureType)
        {
            return _converters.ContainsKey(structureType.ToLower());
        }

        /// <summary>
        /// Регистрирует новый конвертер для указанного типа структуры.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <param name="converter">Конвертер для регистрации.</param>
        public void RegisterConverter(string structureType, IStructureConverter converter)
        {
            _converters[structureType.ToLower()] = converter;
        }
    }

    /// <summary>
    /// Базовый абстрактный класс для конвертеров структур данных.
    /// </summary>
    /// <remarks>
    /// Предоставляет базовую функциональность и абстрактные методы для конвертации структур данных.
    /// </remarks>
    public abstract class BaseStructureConverter : IStructureConverter
    {
        /// <summary>
        /// Конвертирует структуру данных в значение переменной.
        /// </summary>
        /// <param name="structure">Структура данных для конвертации.</param>
        /// <returns>Значение переменной.</returns>
        public abstract IVariableValue ConvertToVariableValue(IDataStructure structure);

        /// <summary>
        /// Конвертирует значение переменной в структуру данных.
        /// </summary>
        /// <param name="value">Значение переменной для конвертации.</param>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>Структура данных.</returns>
        public abstract IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType);

        /// <summary>
        /// Проверяет, поддерживает ли конвертер указанный тип структуры.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>true, если тип поддерживается; иначе false.</returns>
        public abstract bool CanConvert(string structureType);

        /// <summary>
        /// Конвертирует узел дерева в значение переменной.
        /// </summary>
        /// <param name="node">Узел дерева.</param>
        /// <returns>Значение переменной, представляющее узел дерева.</returns>
        protected IVariableValue ConvertTreeNode(TreeNode node)
        {
            if (node == null)
                return new NullValue();

            var properties = new Dictionary<string, IVariableValue>
            {
                ["value"] = new IntValue(node.Value),
                ["id"] = new StringValue(node.Id),
                ["isLeaf"] = new BoolValue(node.Left == null && node.Right == null)
            };

            if (node.Left != null)
                properties["left"] = ConvertTreeNode(node.Left);
            if (node.Right != null)
                properties["right"] = ConvertTreeNode(node.Right);
            if (node.Parent != null)
                properties["parent"] = new StringValue(node.Parent.Id);

            return new ObjectValue(properties);
        }

        /// <summary>
        /// Конвертирует значение переменной в узел дерева.
        /// </summary>
        /// <param name="value">Значение переменной.</param>
        /// <returns>Узел дерева.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если формат значения некорректен.
        /// </exception>
        protected TreeNode ConvertToTreeNode(IVariableValue value)
        {
            if (value is NullValue)
                return null;

            if (value is ObjectValue obj)
            {
                var node = new TreeNode
                {
                    Value = obj.GetProperty("value").ToInt(),
                    Id = obj.GetProperty("id").ToString()
                };

                if (obj.HasProperty("left"))
                    node.Left = ConvertToTreeNode(obj.GetProperty("left"));
                if (obj.HasProperty("right"))
                    node.Right = ConvertToTreeNode(obj.GetProperty("right"));

                return node;
            }

            throw new InvalidOperationException("Invalid tree node format");
        }
    }

    /// <summary>
    /// Конвертер для массивов.
    /// </summary>
    /// <remarks>
    /// Поддерживает конвертацию массивов различных типов: int[], double[], string[], bool[], ArrayValue.
    /// </remarks>
    public class ArrayStructureConverter : BaseStructureConverter
    {
        /// <summary>
        /// Конвертирует структуру массива в значение переменной.
        /// </summary>
        /// <param name="structure">Структура массива.</param>
        /// <returns>Значение переменной, представляющее массив.</returns>
        public override IVariableValue ConvertToVariableValue(IDataStructure structure)
        {
            var state = structure.GetState();
            var arrayValue = ConvertToArrayValue(state);

            var properties = new Dictionary<string, IVariableValue>
            {
                ["type"] = new StringValue("array"),
                ["id"] = new StringValue(structure.Id),
                ["values"] = arrayValue,
                ["len"] = new IntValue(arrayValue.Length),
                ["isEmpty"] = new BoolValue(arrayValue.Length == 0)
            };

            return new ObjectValue(properties);
        }

        /// <summary>
        /// Конвертирует значение переменной в структуру массива.
        /// </summary>
        /// <param name="value">Значение переменной.</param>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>Структура массива.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если формат значения некорректен.
        /// </exception>
        public override IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            if (value is ObjectValue obj && obj.GetProperty("values") is ArrayValue arrayValue)
            {
                var state = ConvertFromArrayValue(arrayValue);
                return StructureFactory.CreateStructure(structureType, state);
            }

            throw new InvalidOperationException("Invalid array structure format");
        }

        /// <summary>
        /// Проверяет, поддерживает ли конвертер указанный тип структуры.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>true, если тип "array"; иначе false.</returns>
        public override bool CanConvert(string structureType)
        {
            return structureType.ToLower() == "array";
        }

        /// <summary>
        /// Конвертирует состояние массива в значение ArrayValue.
        /// </summary>
        /// <param name="state">Состояние массива.</param>
        /// <returns>Значение ArrayValue.</returns>
        /// <exception cref="NotSupportedException">
        /// Выбрасывается, если тип состояния не поддерживается.
        /// </exception>
        private ArrayValue ConvertToArrayValue(object state)
        {
            return state switch
            {
                int[] intArray => ArrayValue.CreateIntArray(intArray),
                double[] doubleArray => ArrayValue.CreateDoubleArray(doubleArray),
                string[] stringArray => ArrayValue.CreateStringArray(stringArray),
                bool[] boolArray => ArrayValue.CreateBoolArray(boolArray),
                ArrayValue arrayValue => arrayValue,
                JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Array => ConvertJsonArray(jsonElement),
                _ => throw new NotSupportedException($"Unsupported array type: {state?.GetType().Name}")
            };
        }

        /// <summary>
        /// Конвертирует значение ArrayValue в состояние массива.
        /// </summary>
        /// <param name="arrayValue">Значение ArrayValue.</param>
        /// <returns>Состояние массива (массив соответствующего типа).</returns>
        /// <remarks>
        /// Определяет тип массива на основе типа первого элемента.
        /// </remarks>
        private object ConvertFromArrayValue(ArrayValue arrayValue)
        {
            // Определяем тип массива на основе содержимого
            if (arrayValue.Length == 0)
                return Array.Empty<int>();

            var firstElement = arrayValue[0];
            return firstElement.Type switch
            {
                VariableType.Int => arrayValue.ToArray(v => v.ToInt()),
                VariableType.Double => arrayValue.ToArray(v => v.ToDouble()),
                VariableType.String => arrayValue.ToArray(v => v.ToString()),
                VariableType.Bool => arrayValue.ToArray(v => v.ToBool()),
                _ => arrayValue.ToArray(v => v.ToString())
            };
        }

        /// <summary>
        /// Конвертирует JSON массив в значение ArrayValue.
        /// </summary>
        /// <param name="jsonArray">JSON элемент, представляющий массив.</param>
        /// <returns>Значение ArrayValue.</returns>
        private ArrayValue ConvertJsonArray(JsonElement jsonArray)
        {
            var items = new List<IVariableValue>();
            foreach (var element in jsonArray.EnumerateArray())
            {
                items.Add(ConvertJsonElement(element));
            }
            return new ArrayValue(items);
        }

        /// <summary>
        /// Конвертирует JSON элемент в значение переменной.
        /// </summary>
        /// <param name="element">JSON элемент.</param>
        /// <returns>Значение переменной.</returns>
        private IVariableValue ConvertJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.TryGetInt32(out int intVal)
                    ? new IntValue(intVal)
                    : new DoubleValue(element.GetDouble()),
                JsonValueKind.String => new StringValue(element.GetString()),
                JsonValueKind.True => new BoolValue(true),
                JsonValueKind.False => new BoolValue(false),
                JsonValueKind.Array => ConvertJsonArray(element),
                JsonValueKind.Object => ConvertJsonObject(element),
                JsonValueKind.Null => new NullValue(),
                _ => new StringValue(element.ToString())
            };
        }

        /// <summary>
        /// Конвертирует JSON объект в значение ObjectValue.
        /// </summary>
        /// <param name="jsonObject">JSON элемент, представляющий объект.</param>
        /// <returns>Значение ObjectValue.</returns>
        private ObjectValue ConvertJsonObject(JsonElement jsonObject)
        {
            var properties = new Dictionary<string, IVariableValue>();
            foreach (var property in jsonObject.EnumerateObject())
            {
                properties[property.Name] = ConvertJsonElement(property.Value);
            }
            return new ObjectValue(properties);
        }
    }

    /// <summary>
    /// Конвертер для бинарных деревьев.
    /// </summary>
    public class BinaryTreeStructureConverter : BaseStructureConverter
    {
        /// <summary>
        /// Конвертирует структуру бинарного дерева в значение переменной.
        /// </summary>
        /// <param name="structure">Структура бинарного дерева.</param>
        /// <returns>Значение переменной, представляющее дерево.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если состояние структуры некорректно.
        /// </exception>
        public override IVariableValue ConvertToVariableValue(IDataStructure structure)
        {
            var state = structure.GetState();
            if (state is TreeNode root)
            {
                var properties = new Dictionary<string, IVariableValue>
                {
                    ["type"] = new StringValue("binarytree"),
                    ["id"] = new StringValue(structure.Id),
                    ["root"] = ConvertTreeNode(root),
                    ["isEmpty"] = new BoolValue(root == null),
                    ["height"] = new IntValue(CalculateTreeHeight(root)),
                    ["nodeCount"] = new IntValue(CountTreeNodes(root))
                };

                return new ObjectValue(properties);
            }

            throw new InvalidOperationException("Invalid binary tree state");
        }

        /// <summary>
        /// Конвертирует значение переменной в структуру бинарного дерева.
        /// </summary>
        /// <param name="value">Значение переменной.</param>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>Структура бинарного дерева.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если формат значения некорректен.
        /// </exception>
        public override IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            if (value is ObjectValue obj)
            {
                var root = ConvertToTreeNode(obj.GetProperty("root"));
                return StructureFactory.CreateStructure(structureType, root);
            }

            throw new InvalidOperationException("Invalid binary tree format");
        }

        /// <summary>
        /// Проверяет, поддерживает ли конвертер указанный тип структуры.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>true, если тип "binarytree"; иначе false.</returns>
        public override bool CanConvert(string structureType)
        {
            return structureType.ToLower() == "binarytree";
        }

        /// <summary>
        /// Вычисляет высоту дерева.
        /// </summary>
        /// <param name="node">Корень дерева.</param>
        /// <returns>Высота дерева.</returns>
        private int CalculateTreeHeight(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(CalculateTreeHeight(node.Left), CalculateTreeHeight(node.Right));
        }

        /// <summary>
        /// Подсчитывает количество узлов в дереве.
        /// </summary>
        /// <param name="node">Корень дерева.</param>
        /// <returns>Количество узлов.</returns>
        private int CountTreeNodes(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + CountTreeNodes(node.Left) + CountTreeNodes(node.Right);
        }
    }

    /// <summary>
    /// Конвертер для связных списков.
    /// </summary>
    public class LinkedListStructureConverter : BaseStructureConverter
    {
        /// <summary>
        /// Конвертирует структуру связного списка в значение переменной.
        /// </summary>
        /// <param name="structure">Структура связного списка.</param>
        /// <returns>Значение переменной, представляющее список.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если состояние структуры некорректно.
        /// </exception>
        public override IVariableValue ConvertToVariableValue(IDataStructure structure)
        {
            var state = structure.GetState();
            if (state is ListNode head)
            {
                var arrayValue = ConvertLinkedListToArray(head);

                var properties = new Dictionary<string, IVariableValue>
                {
                    ["type"] = new StringValue("linkedlist"),
                    ["id"] = new StringValue(structure.Id),
                    ["values"] = arrayValue,
                    ["length"] = new IntValue(arrayValue.Length),
                    ["headValue"] = head != null ? new IntValue(head.Value) : new IntValue(0),
                    ["isEmpty"] = new BoolValue(head == null)
                };

                return new ObjectValue(properties);
            }

            throw new InvalidOperationException("Invalid linked list state");
        }

        /// <summary>
        /// Конвертирует значение переменной в структуру связного списка.
        /// </summary>
        /// <param name="value">Значение переменной.</param>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>Структура связного списка.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если формат значения некорректен.
        /// </exception>
        public override IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            if (value is ObjectValue obj && obj.GetProperty("values") is ArrayValue arrayValue)
            {
                var head = ConvertArrayToLinkedList(arrayValue);
                return StructureFactory.CreateStructure(structureType, head);
            }

            throw new InvalidOperationException("Invalid linked list format");
        }

        /// <summary>
        /// Проверяет, поддерживает ли конвертер указанный тип структуры.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>true, если тип "linkedlist"; иначе false.</returns>
        public override bool CanConvert(string structureType)
        {
            return structureType.ToLower() == "linkedlist";
        }

        /// <summary>
        /// Конвертирует связный список в массив значений.
        /// </summary>
        /// <param name="head">Голова списка.</param>
        /// <returns>Массив значений.</returns>
        private ArrayValue ConvertLinkedListToArray(ListNode head)
        {
            var values = new List<int>();
            var current = head;
            while (current != null)
            {
                values.Add(current.Value);
                current = current.Next;
            }
            return ArrayValue.CreateIntArray(values.ToArray());
        }

        /// <summary>
        /// Конвертирует массив значений в связный список.
        /// </summary>
        /// <param name="arrayValue">Массив значений.</param>
        /// <returns>Голова списка.</returns>
        private ListNode ConvertArrayToLinkedList(ArrayValue arrayValue)
        {
            if (arrayValue.Length == 0)
                return null;

            ListNode head = null;
            ListNode current = null;

            for (int i = 0; i < arrayValue.Length; i++)
            {
                var newNode = new ListNode { Value = arrayValue[i].ToInt() };
                if (head == null)
                {
                    head = newNode;
                    current = head;
                }
                else
                {
                    current.Next = newNode;
                    current = current.Next;
                }
            }

            return head;
        }
    }

    /// <summary>
    /// Конвертер для графов.
    /// </summary>
    /// <remarks>
    /// Базовая реализация, требует доработки для полной поддержки графов.
    /// </remarks>
    public class GraphStructureConverter : BaseStructureConverter
    {
        /// <summary>
        /// Конвертирует структуру графа в значение переменной.
        /// </summary>
        /// <param name="structure">Структура графа.</param>
        /// <returns>Базовое значение переменной для графа.</returns>
        public override IVariableValue ConvertToVariableValue(IDataStructure structure)
        {
            // Реализация для графов
            var properties = new Dictionary<string, IVariableValue>
            {
                ["type"] = new StringValue("graph"),
                ["id"] = new StringValue(structure.Id)
            };

            return new ObjectValue(properties);
        }

        /// <summary>
        /// Конвертирует значение переменной в структуру графа.
        /// </summary>
        /// <param name="value">Значение переменной.</param>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>Базовая структура графа.</returns>
        public override IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            // Реализация для графов
            return StructureFactory.CreateStructure(structureType, null);
        }

        /// <summary>
        /// Проверяет, поддерживает ли конвертер указанный тип структуры.
        /// </summary>
        /// <param name="structureType">Тип структуры данных.</param>
        /// <returns>true, если тип "graph"; иначе false.</returns>
        public override bool CanConvert(string structureType)
        {
            return structureType.ToLower() == "graph";
        }
    }
}