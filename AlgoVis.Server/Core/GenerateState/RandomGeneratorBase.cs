using AlgoVis.Models.Models.DataStructures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    public abstract class RandomGeneratorBase<T> : IRandomStructureGenerator<T> where T : IDataStructure
    {
        protected Random _random;

        public RandomGeneratorBase()
        {
            _random = new Random();
        }
        public RandomGeneratorBase(int seed)
        {
            _random = new Random(seed);
        }
        public abstract string StructureType { get; }

        public abstract T Generate(Dictionary<string, object> parametrs);
        public abstract Dictionary<string, object> GetDefaultParameters();

        IDataStructure IRandomStructureGenerator.Generate(Dictionary<string, object> parametrs) => Generate(parametrs);

        protected int GetParameterValue(Dictionary<string, object> parametrs, string key, int defaultValue)
        {
            if(parametrs != null && parametrs.ContainsKey(key))
            {
                if(parametrs[key].GetType().ToString().ToLower() == "system.int32")
                {
                    return (int)parametrs[key];
                }
                else
                {
                    var param = (JsonElement)parametrs[key];
                    return param.GetInt32();
                }
            }
            else
            {
                return defaultValue;
            }
        }

        protected string GetParameterValue(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            return parameters != null && parameters.ContainsKey(key)
                ? parameters[key]?.ToString()
                : defaultValue;
        }
    }
}
