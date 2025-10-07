using AlgoVis.Server.Models.enums;

namespace AlgoVis.Server.Models
{

    public class Instruction
    {
        public StructureType StructureType { get; set; } = StructureType.array;
        public List<AlgorithmConfig> Algorithms { get; set; } = new();
        public int LengthAlgorithms { get; set; } = 1;
    }
    public class AlgorithmConfig
    {
        public string Name { get; set; } = string.Empty;
        public int Length { get; set; } = 10;
        public bool IsArgs { get; set; } = false;
        public int[] Args {  get; set; } = Array.Empty<int>();
        public string SessionId { get; set; } = string.Empty;
        public Dictionary<string, string> Sessions { get; set; } // доп параметры
    }
}
