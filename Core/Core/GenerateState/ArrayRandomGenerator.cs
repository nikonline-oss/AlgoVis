using AlgoVis.Models.Models.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.GenerateState
{
    public class ArrayRandomGenerator : RandomGeneratorBase<ArrayStructure>
    {
        public ArrayRandomGenerator()
        {
        }

        public ArrayRandomGenerator(int seed)
        {
            _random = new Random(seed);
        }
        public override string StructureType => "array";

        public override ArrayStructure Generate(Dictionary<string, object> parametrs)
        {
            var size = GetParameterValue(parametrs, "size", 10);
            var minValue = GetParameterValue(parametrs, "minValue", 0);
            var maxValue = GetParameterValue(parametrs, "maxValue", 100);
            var sorted = GetParameterValue(parametrs, "sorted", 0);

            var data = new int[size];
            for (var i = 0; i < size; i++)
            {
                data[i] = _random.Next(minValue, maxValue + 1);
            }

            if (sorted == 1)
            {
                Array.Sort(data);
            }

            return new ArrayStructure(data);
        }

        public override Dictionary<string, object> GetDefaultParameters()
        {
            return new Dictionary<string, object>
            {
                { "size", 10 },
                { "minValue", 0 },
                { "maxValue", 100 },
                { "sorted", 0 }
            };
        }
    }
}
