using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.models.enums;

namespace testing.models
{
    public class VisualizationStep
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int StepNumber { get; set; }
        public string Operation { get; set; } = string.Empty;
        public StructureType StructureType { get; set; } = StructureType.array;
        public string StructureId { get; set; } = string.Empty;
        public string ParametersJson { get; set; } = "{}";
        public string StateSnapshotJson { get; set; } = "{}";
        public string Description { get; set; } = string.Empty;
    }
}
