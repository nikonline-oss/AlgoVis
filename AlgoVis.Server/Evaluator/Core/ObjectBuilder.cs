//using AlgoVis.Evaluator.Evaluator.Types;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace AlgoVis.Evaluator.Evaluator.Core
//{
//    public class ObjectBuilder
//    {
//        public static VariableValue CreateObjectFromDictionary(Dictionary<string, object> properties)
//        {
//            var dict = new Dictionary<string, VariableValue>();

//            foreach (var prop in properties)
//            {
//                dict[prop.Key] = new VariableValue(prop.Value);
//            }

//            return new VariableValue(dict);
//        }

//        public static VariableValue CreateNestedObject(string json)
//        {
//            try
//            {
//                using var document = JsonDocument.Parse(json);
//                return ParseJsonElement(document.RootElement);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"⚠️ Ошибка создания объекта из JSON: {ex.Message}");
//                return new VariableValue(new Dictionary<string, VariableValue>());
//            }
//        }

//        private static VariableValue ParseJsonElement(JsonElement element)
//        {
//            switch (element.ValueKind)
//            {
//                case JsonValueKind.Object:
//                    var dict = new Dictionary<string, VariableValue>();
//                    foreach (var property in element.EnumerateObject())
//                    {
//                        dict[property.Name] = ParseJsonElement(property.Value);
//                    }
//                    return new VariableValue(dict);

//                case JsonValueKind.Array:
//                    var list = new List<VariableValue>();
//                    foreach (var item in element.EnumerateArray())
//                    {
//                        list.Add(ParseJsonElement(item));
//                    }
//                    return new VariableValue(list);

//                case JsonValueKind.String:
//                    return new VariableValue(element.GetString());

//                case JsonValueKind.Number:
//                    if (element.TryGetInt32(out int intValue))
//                        return new VariableValue(intValue);
//                    else if (element.TryGetDouble(out double doubleValue))
//                        return new VariableValue(doubleValue);
//                    else
//                        return new VariableValue(0);

//                case JsonValueKind.True:
//                    return new VariableValue(true);

//                case JsonValueKind.False:
//                    return new VariableValue(false);

//                case JsonValueKind.Null:
//                    return new VariableValue((object)null);

//                default:
//                    return new VariableValue(0);
//            }
//        }
//    }
//}
