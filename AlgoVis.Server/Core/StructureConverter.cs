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
    public class UniversalStructureConverter : IStructureConverter
    {
        private readonly Dictionary<string, IStructureConverter> _converters;

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

        public IVariableValue ConvertToVariableValue(IDataStructure structure)
        {
            if (structure == null)
                return new NullValue();

            var type = structure.Type.ToLower();
            if (_converters.TryGetValue(type, out var converter))
                return converter.ConvertToVariableValue(structure);

            throw new NotSupportedException($"Structure type '{type}' is not supported");
        }

        public IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var type = structureType.ToLower();
            if (_converters.TryGetValue(type, out var converter))
                return converter.ConvertFromVariableValue(value, structureType);

            throw new NotSupportedException($"Structure type '{type}' is not supported");
        }

        public bool CanConvert(string structureType)
        {
            return _converters.ContainsKey(structureType.ToLower());
        }

        public void RegisterConverter(string structureType, IStructureConverter converter)
        {
            _converters[structureType.ToLower()] = converter;
        }
    }

    // Базовый класс для конвертеров
    public abstract class BaseStructureConverter : IStructureConverter
    {
        public abstract IVariableValue ConvertToVariableValue(IDataStructure structure);
        public abstract IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType);
        public abstract bool CanConvert(string structureType);

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

    // Конвертер для массивов
    public class ArrayStructureConverter : BaseStructureConverter
    {
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

        public override IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            if (value is ObjectValue obj && obj.GetProperty("values") is ArrayValue arrayValue)
            {
                var state = ConvertFromArrayValue(arrayValue);
                return StructureFactory.CreateStructure(structureType, state);
            }

            throw new InvalidOperationException("Invalid array structure format");
        }

        public override bool CanConvert(string structureType)
        {
            return structureType.ToLower() == "array";
        }

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

        private ArrayValue ConvertJsonArray(JsonElement jsonArray)
        {
            var items = new List<IVariableValue>();
            foreach (var element in jsonArray.EnumerateArray())
            {
                items.Add(ConvertJsonElement(element));
            }
            return new ArrayValue(items);
        }

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

    // Конвертер для бинарных деревьев
    public class BinaryTreeStructureConverter : BaseStructureConverter
    {
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

        public override IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            if (value is ObjectValue obj)
            {
                var root = ConvertToTreeNode(obj.GetProperty("root"));
                return StructureFactory.CreateStructure(structureType, root);
            }

            throw new InvalidOperationException("Invalid binary tree format");
        }

        public override bool CanConvert(string structureType)
        {
            return structureType.ToLower() == "binarytree";
        }

        private int CalculateTreeHeight(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(CalculateTreeHeight(node.Left), CalculateTreeHeight(node.Right));
        }

        private int CountTreeNodes(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + CountTreeNodes(node.Left) + CountTreeNodes(node.Right);
        }
    }

    // Конвертер для связных списков
    public class LinkedListStructureConverter : BaseStructureConverter
    {
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

        public override IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            if (value is ObjectValue obj && obj.GetProperty("values") is ArrayValue arrayValue)
            {
                var head = ConvertArrayToLinkedList(arrayValue);
                return StructureFactory.CreateStructure(structureType, head);
            }

            throw new InvalidOperationException("Invalid linked list format");
        }

        public override bool CanConvert(string structureType)
        {
            return structureType.ToLower() == "linkedlist";
        }

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

    // Конвертер для графов
    public class GraphStructureConverter : BaseStructureConverter
    {
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

        public override IDataStructure ConvertFromVariableValue(IVariableValue value, string structureType)
        {
            // Реализация для графов
            return StructureFactory.CreateStructure(structureType, null);
        }

        public override bool CanConvert(string structureType)
        {
            return structureType.ToLower() == "graph";
        }
    }
}
