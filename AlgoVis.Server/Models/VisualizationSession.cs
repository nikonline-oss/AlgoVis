using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlgoVis.Server.Models
{
    [Table("VisualizationSession")]
    public class VisualizationSession
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(500)]
        public string ClientConnectionId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "TEXT")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Language { get; set; } = "python";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public int CurrentStepIndex { get; set; } = -1;

        [Required]
        [Column(TypeName = "TIMESTAMP")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "TIMESTAMP")]
        public DateTime? UpdatedAt { get; set; }

        [Column(TypeName = "TIMESTAMP")]
        public DateTime? CompletedAt { get; set; }

        public virtual ICollection<VisualizationStep> Steps { get; set; } = new List<VisualizationStep>();
    }
}