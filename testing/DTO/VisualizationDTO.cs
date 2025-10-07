using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.models;
using testing.models.enums;

namespace testing.DTO
{
        public class VisualizationRequest
        {
            public StructureType StructureType { get; set; } = StructureType.array;
            public List<AlgorithmConfig> Algorithms { get; set; } = new();
            public int LengthAlgorithms { get; set; } = 1;
        }

        public class VisualizationResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<AlgorithmResult> Results { get; set; } = new();
            public OverallStatistics overallStats { get; set; } = new();
        }
}
