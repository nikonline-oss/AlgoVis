using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.VariableValues;
using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Suport;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core
{
    public static class StructureFactory
    {
        private static readonly UniversalStructureConverter _converter = new UniversalStructureConverter();

        public static IDataStructure CreateStructure(string type, object data)
        {
            Console.WriteLine($"🔍 StructureFactory: создание структуры типа '{type}', данные: {data}, тип данных: {data?.GetType()}");

            // Если данные приходят как JsonElement, парсим их
            if (data is JsonElement jsonElement)
            {
                return type.ToLower() switch
                {
                    "array" => CreateArrayFromJson(jsonElement),
                    "linkedlist" => CreateLinkedListFromJson(jsonElement),
                    "binarytree" => CreateBinaryTreeFromJson(jsonElement),
                    "graph" => CreateGraphFromJson(jsonElement),
                    _ => throw new ArgumentException($"Unsupported structure type: {type}")
                };
            }

            // Если данные уже в формате IVariableValue
            if (data is IVariableValue variableValue)
            {
                return _converter.ConvertFromVariableValue(variableValue, type);
            }

            // Старая логика для обратной совместимости
            return type.ToLower() switch
            {
                "array" => CreateArrayStructure(data),
                "linkedlist" => new LinkedListStructure { Head = (ListNode)data },
                "binarytree" => new BinaryTreeStructure { Root = (TreeNode)data },
                "graph" => new GraphStructure
                {
                    Nodes = ((GraphState)data).Nodes,
                    Edges = ((GraphState)data).Edges
                },
                _ => throw new ArgumentException($"Unsupported structure type: {type}")
            };
        }

        private static IDataStructure CreateArrayStructure(object data)
        {
            return data switch
            {
                int[] intArray => new ArrayStructure(intArray),
                double[] doubleArray => new UniversalArrayStructure(ArrayValue.CreateDoubleArray(doubleArray)),
                string[] stringArray => new UniversalArrayStructure(ArrayValue.CreateStringArray(stringArray)),
                bool[] boolArray => new UniversalArrayStructure(ArrayValue.CreateBoolArray(boolArray)),
                object[] objectArray => new UniversalArrayStructure(ArrayValue.CreateFromObjects(objectArray)),
                ArrayValue arrayValue => new UniversalArrayStructure(arrayValue),
                JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Array =>
                    new UniversalArrayStructure(ArrayValue.CreateFromJsonArray(jsonElement.GetRawText())),
                _ => throw new ArgumentException($"Unsupported array data type: {data?.GetType().Name}")
            };
        }

        // Универсальная структура для любых типов массивов
        public class UniversalArrayStructure : IDataStructure
        {
            public ArrayValue ArrayValue { get; private set; }
            public string Type => "array";
            public string Id { get; } = Guid.NewGuid().ToString();

            public UniversalArrayStructure(ArrayValue arrayValue = null)
            {
                ArrayValue = arrayValue ?? new ArrayValue();
            }

            public object GetState()
            {
                return ArrayValue;
            }

            public object GetOriginState()
            {
                return ArrayValue;
            }

            public void ApplyState(object state)
            {
                if (state is ArrayValue arrayValue)
                {
                    ArrayValue = arrayValue;
                }
                else if (state is int[] intArray)
                {
                    ArrayValue = ArrayValue.CreateIntArray(intArray);
                }
                else if (state is string[] stringArray)
                {
                    ArrayValue = ArrayValue.CreateStringArray(stringArray);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid state type for UniversalArrayStructure: {state?.GetType().Name}");
                }
            }

            public VisualizationData ToVisualizationData()
            {
                throw new NotImplementedException();
            }
        }

        private static ArrayStructure CreateArrayFromJson(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                // Парсим универсальный массив
                var array = new List<int>();
                foreach (var element in jsonElement.EnumerateArray())
                {
                    array.Add(ParseJsonValueToInt(element));
                }
                Console.WriteLine($"🔍 Создан массив: [{string.Join(", ", array)}]");
                return new ArrayStructure(array.ToArray());
            }
            else
            {
                throw new ArgumentException("Для массива ожидается JSON массив");
            }
        }

        private static int ParseJsonValueToInt(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.GetInt32(),
                JsonValueKind.String => int.TryParse(element.GetString(), out int result) ? result : 0,
                JsonValueKind.True => 1,
                JsonValueKind.False => 0,
                _ => 0
            };
        }

        private static BinaryTreeStructure CreateBinaryTreeFromJson(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var root = ParseTreeNode(jsonElement);
                Console.WriteLine($"🔍 Создано бинарное дерево с корнем: {root?.Value}");
                return new BinaryTreeStructure { Root = root };
            }
            else
            {
                throw new ArgumentException("Для бинарного дерева ожидается JSON объект");
            }
        }

        private static TreeNode ParseTreeNode(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Null)
                return null;

            var node = new TreeNode();

            // Извлекаем значение узла
            if (jsonElement.TryGetProperty("value", out JsonElement valueProp))
            {
                node.Value = valueProp.GetInt32();
            }

            // Рекурсивно парсим левое поддерево
            if (jsonElement.TryGetProperty("left", out JsonElement leftProp))
            {
                node.Left = ParseTreeNode(leftProp);
            }

            // Рекурсивно парсим правое поддерево
            if (jsonElement.TryGetProperty("right", out JsonElement rightProp))
            {
                node.Right = ParseTreeNode(rightProp);
            }

            return node;
        }

        private static LinkedListStructure CreateLinkedListFromJson(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                var values = jsonElement.EnumerateArray().Select(e => e.GetInt32()).ToArray();
                ListNode head = null;
                ListNode current = null;

                foreach (var value in values)
                {
                    var newNode = new ListNode { Value = value };
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

                Console.WriteLine($"🔍 Создан связный список с {values.Length} элементами");
                return new LinkedListStructure { Head = head };
            }
            else
            {
                throw new ArgumentException("Для связного списка ожидается JSON массив");
            }
        }

        private static GraphStructure CreateGraphFromJson(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var nodes = new List<GraphNode>();
                var edges = new List<GraphEdge>();

                // Парсим узлы
                if (jsonElement.TryGetProperty("nodes", out JsonElement nodesProp) &&
                    nodesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var nodeElement in nodesProp.EnumerateArray())
                    {
                        var node = new GraphNode();
                        if (nodeElement.TryGetProperty("id", out JsonElement idProp))
                            node.Id = idProp.GetString();
                        if (nodeElement.TryGetProperty("value", out JsonElement valueProp))
                            node.Value = valueProp.GetInt32();
                        nodes.Add(node);
                    }
                }

                // Парсим ребра
                if (jsonElement.TryGetProperty("edges", out JsonElement edgesProp) &&
                    edgesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var edgeElement in edgesProp.EnumerateArray())
                    {
                        var edge = new GraphEdge();
                        if (edgeElement.TryGetProperty("fromId", out JsonElement fromIdProp))
                            edge.FromId = fromIdProp.GetString();
                        if (edgeElement.TryGetProperty("toId", out JsonElement toIdProp))
                            edge.ToId = toIdProp.GetString();
                        if (edgeElement.TryGetProperty("weight", out JsonElement weightProp))
                            edge.Weight = weightProp.GetDouble();
                        edges.Add(edge);
                    }
                }

                Console.WriteLine($"🔍 Создан граф с {nodes.Count} узлами и {edges.Count} ребрами");
                return new GraphStructure { Nodes = nodes, Edges = edges };
            }
            else
            {
                throw new ArgumentException("Для графа ожидается JSON объект");
            }
        }

        // Обобщенная версия для типобезопасного использования
        public static IDataStructure<T> CreateStructure<T>(string type, object data)
        {
            var structure = CreateStructure(type, data);
            return structure as IDataStructure<T> ??
                throw new InvalidOperationException($"Не удается привести структуру к типу {typeof(T).Name}");
        }

        public static IDataStructure CreateStructureFromVariableValue(IVariableValue value, string type)
        {
            return _converter.ConvertFromVariableValue(value, type);
        }

        // Метод для конвертации структуры в IVariableValue
        public static IVariableValue ConvertStructureToVariableValue(IDataStructure structure)
        {
            return _converter.ConvertToVariableValue(structure);
        }

    }
}
