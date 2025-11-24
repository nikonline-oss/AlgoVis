using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Visualization;

namespace testing.Models.DataStructures
{
    public class ArrayStructure : IDataStructure<int[]>
    {
        public string Type => "array";
        public string Id { get; } = Guid.NewGuid().ToString();
        private int[] _data;
        private readonly int[] _originData;

        public ArrayStructure(int[] data)
        {
            _data = data;
            _originData = (int[])_data.Clone();
        }

        public int[] GetState() => (int[])_data.Clone();

        public void ApplyState(int[] state) => _data = (int[])state.Clone();

        public VisualizationData ToVisualizationData()
        {
            return new VisualizationData
            {
                structureType = "array",
                elements = _data.Select((value, index) =>
                    new KeyValuePair<string, object>(index.ToString(), new
                    {
                        value,
                        index,
                        label = $"arr[{index}]"
                    })).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        public int[] GetOriginState() => (int[])_originData;

    }
}
