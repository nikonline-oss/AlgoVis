using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testing.models
{
    public class VisualizationSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ClientConnectionId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Language { get; set; } = "python";
        public string Status { get; set; } = "Pending";
        public int CurrentStepIndex { get; set; } = -1;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}