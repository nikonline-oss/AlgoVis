using AlgoVis.Models.Models.Visualization;

namespace AlgoVis.Server.DTO
{
    public class GenerateStructureRequest
    {
        public string StructureType { get; set; } = "array";
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public int? Seed { get; set; }
    }

    public class TestAllStructuresRequest
    {
        public Dictionary<string, Dictionary<string, object>> StructureParameters { get; set; } = new Dictionary<string, Dictionary<string, object>>();
        public int? Seed { get; set; }
    }

    public class StructureGenerationResponse
    {
        public bool Success { get; set; }
        public string StructureType { get; set; }
        public string Error { get; set; }
        public VisualizationData VisualizationData { get; set; }
        public Dictionary<string, object> UsedParameters { get; set; }
        public object State { get; set; }
        public StructureMetadata Metadata { get; set; }
    }

    public class StructureMetadata
    {
        public int NodeCount { get; set; }
        public int EdgeCount { get; set; }
        public int ElementCount { get; set; }
        public string GenerationTime { get; set; }
    }

    public class SupportedStructuresResponse
    {
        public List<StructureInfo> Structures { get; set; } = new List<StructureInfo>();
    }

    public class StructureInfo
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> DefaultParameters { get; set; } = new Dictionary<string, object>();
        public List<ParameterInfo> ParametersInfo { get; set; } = new List<ParameterInfo>();
    }

    public class ParameterInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }
    }

    public class TestAllResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public Dictionary<string, StructureGenerationResponse> Results { get; set; } = new Dictionary<string, StructureGenerationResponse>();
        public string TotalTime { get; set; }
    }
}
