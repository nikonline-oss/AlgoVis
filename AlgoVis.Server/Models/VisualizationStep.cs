using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlgoVis.Server.Models
{
    [Table("VisualizationStep")]
    public class VisualizationStep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        public int StepNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string Operation { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string StructureType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string StructureId { get; set; } = string.Empty;

        [Column(TypeName = "JSON")]
        public string ParametersJson { get; set; } = "{}";

        [Column(TypeName = "JSON")]
        public string StateSnapshotJson { get; set; } = "{}";

        [Column(TypeName = "TEXT")]
        public string Description { get; set; } = string.Empty;

        [ForeignKey("SessionId")]
        public virtual VisualizationSession Session { get; set; } = null!;

        [NotMapped]
        public Dictionary<string, object> Parameters
        {
            get => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(ParametersJson) ?? new();
            set => ParametersJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Dictionary<string, object> StateSnapshot
        {
            get => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(StateSnapshotJson) ?? new();
            set => StateSnapshotJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}