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
    /// <summary>
    /// Фабрика для создания структур данных различных типов.
    /// </summary>
    /// <remarks>
    /// Этот класс предоставляет методы для создания структур данных из различных форматов:
    /// JSON, IVariableValue и нативных объектов C#. Поддерживает массивы, связные списки, 
    /// бинарные деревья и графы.
    /// </remarks>
    public static class StructureFactory
    {
        private static readonly UniversalStructureConverter _converter = new UniversalStructureConverter();

        /// <summary>
        /// Создает структуру данных указанного типа.
        /// </summary>
        /// <param name="type">Тип структуры данных ("array", "linkedlist", "binarytree", "graph").</param>
        /// <param name="data">Данные для создания структуры.</param>
        /// <returns>Созданная структура данных.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если указан неподдерживаемый тип структуры.
        /// </exception>
        /// <remarks>
        /// Метод поддерживает несколько форматов входных данных:
        /// - JsonElement: данные в формате JSON
        /// - IVariableValue: данные в виде значений переменных
        /// - Нативные объекты C#: для обратной совместимости
        /// </remarks>
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

        /// <summary>
        /// Создает структуру массива из различных форматов данных.
        /// </summary>
        /// <param name="data">Данные для создания массива.</param>
        /// <returns>Структура массива.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если формат данных не поддерживается.
        /// </exception>
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

        /// <summary>
        /// Универсальная структура для массивов любых типов.
        /// </summary>
        /// <remarks>
        /// Эта структура оборачивает ArrayValue и предоставляет интерфейс IDataStructure
        /// для работы с массивами различных типов (int, double, string, bool, object).
        /// </remarks>
        public class UniversalArrayStructure : IDataStructure
        {
            /// <summary>
            /// Получает или задает значение массива.
            /// </summary>
            public ArrayValue ArrayValue { get; private set; }

            /// <summary>
            /// Получает тип структуры - "array".
            /// </summary>
            public string Type => "array";

            /// <summary>
            /// Получает уникальный идентификатор структуры.
            /// </summary>
            public string Id { get; } = Guid.NewGuid().ToString();

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="UniversalArrayStructure"/>.
            /// </summary>
            /// <param name="arrayValue">Начальное значение массива.</param>
            public UniversalArrayStructure(ArrayValue arrayValue = null)
            {
                ArrayValue = arrayValue ?? new ArrayValue();
            }

            /// <summary>
            /// Получает текущее состояние структуры.
            /// </summary>
            /// <returns>Текущее значение массива.</returns>
            public object GetState()
            {
                return ArrayValue;
            }

            /// <summary>
            /// Получает исходное состояние структуры.
            /// </summary>
            /// <returns>Исходное значение массива.</returns>
            public object GetOriginState()
            {
                return ArrayValue;
            }

            /// <summary>
            /// Применяет новое состояние к структуре.
            /// </summary>
            /// <param name="state">Новое состояние структуры.</param>
            /// <exception cref="InvalidOperationException">
            /// Выбрасывается, если тип состояния не поддерживается.
            /// </exception>
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

            /// <summary>
            /// Преобразует структуру в данные для визуализации.
            /// </summary>
            /// <returns>Данные для визуализации структуры.</returns>
            /// <exception cref="NotImplementedException">
            /// Метод не реализован в текущей версии.
            /// </exception>
            public VisualizationData ToVisualizationData()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Создает массив из JSON элемента.
        /// </summary>
        /// <param name="jsonElement">JSON элемент, представляющий массив.</param>
        /// <returns>Структура массива.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если JSON элемент не является массивом.
        /// </exception>
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

        /// <summary>
        /// Парсит JSON элемент в целое число.
        /// </summary>
        /// <param name="element">JSON элемент для парсинга.</param>
        /// <returns>Целое число, полученное из JSON элемента.</returns>
        /// <remarks>
        /// Поддерживает различные типы JSON значений:
        /// - Number: получает целое число
        /// - String: пробует парсить строку
        /// - True/False: преобразует в 1/0
        /// - Остальные: возвращает 0
        /// </remarks>
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

        /// <summary>
        /// Создает бинарное дерево из JSON элемента.
        /// </summary>
        /// <param name="jsonElement">JSON элемент, представляющий бинарное дерево.</param>
        /// <returns>Структура бинарного дерева.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если JSON элемент не является объектом.
        /// </exception>
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

        /// <summary>
        /// Парсит JSON элемент в узел дерева.
        /// </summary>
        /// <param name="jsonElement">JSON элемент для парсинга.</param>
        /// <returns>Узел дерева или null, если элемент является null.</returns>
        /// <remarks>
        /// Ожидает JSON объект со свойствами:
        /// - value: значение узла
        /// - left: левое поддерево
        /// - right: правое поддерево
        /// </remarks>
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

        /// <summary>
        /// Создает связный список из JSON элемента.
        /// </summary>
        /// <param name="jsonElement">JSON элемент, представляющий массив значений.</param>
        /// <returns>Структура связного списка.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если JSON элемент не является массивом.
        /// </exception>
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

        /// <summary>
        /// Создает граф из JSON элемента.
        /// </summary>
        /// <param name="jsonElement">JSON элемент, представляющий граф.</param>
        /// <returns>Структура графа.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если JSON элемент не является объектом.
        /// </exception>
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
                            edge.from = fromIdProp.GetString();
                        if (edgeElement.TryGetProperty("toId", out JsonElement toIdProp))
                            edge.to = toIdProp.GetString();
                        if (edgeElement.TryGetProperty("weight", out JsonElement weightProp))
                            edge.weight = weightProp.GetDouble();
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

        /// <summary>
        /// Создает структуру данных указанного типа с типобезопасным возвращаемым значением.
        /// </summary>
        /// <typeparam name="T">Тип структуры данных, реализующий интерфейс IDataStructure.</typeparam>
        /// <param name="type">Тип структуры данных.</param>
        /// <param name="data">Данные для создания структуры.</param>
        /// <returns>Структура данных указанного типа.</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если структуру нельзя привести к указанному типу.
        /// </exception>
        public static IDataStructure<T> CreateStructure<T>(string type, object data)
        {
            var structure = CreateStructure(type, data);
            return structure as IDataStructure<T> ??
                throw new InvalidOperationException($"Не удается привести структуру к типу {typeof(T).Name}");
        }

        /// <summary>
        /// Создает структуру данных из значения переменной.
        /// </summary>
        /// <param name="value">Значение переменной.</param>
        /// <param name="type">Тип структуры данных.</param>
        /// <returns>Структура данных.</returns>
        public static IDataStructure CreateStructureFromVariableValue(IVariableValue value, string type)
        {
            return _converter.ConvertFromVariableValue(value, type);
        }

        /// <summary>
        /// Конвертирует структуру данных в значение переменной.
        /// </summary>
        /// <param name="structure">Структура данных для конвертации.</param>
        /// <returns>Значение переменной, представляющее структуру.</returns>
        public static IVariableValue ConvertStructureToVariableValue(IDataStructure structure)
        {
            return _converter.ConvertToVariableValue(structure);
        }
    }
}