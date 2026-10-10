namespace AlgoVis.Data.Entities;

/// <summary>
/// Попытка решения задания учеником.
/// Хранит код, грейд, результат проверки.
/// </summary>
public sealed class Submission
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Language { get; set; } = "python";
    public string Code { get; set; } = "";

    /// <summary>
    /// Статус: "pending" | "passed" | "failed" | "error".
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Грейд: "good" | "excellent" | "perfect" | null (если не прошло).
    /// </summary>
    public string? Grade { get; set; }

    /// <summary>Очки XP, начисленные за этот сабмишен.</summary>
    public int XpAwarded { get; set; }

    /// <summary>
    /// Результат проверки (JSON):
    /// {
    ///   "passed": true,
    ///   "actualResult": [...],
    ///   "expectedResult": [...],
    ///   "stats": {"comparisons": 28, "swaps": 13, "steps": 101},
    ///   "criteriaChecked": {"good": true, "excellent": true, "perfect": false},
    ///   "reason": "..."
    /// }
    /// </summary>
    public string? ResultJson { get; set; }

    /// <summary>Сообщение об ошибке транспайлера/исполнения, если есть.</summary>
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигация
    public List<Comment> Comments { get; set; } = new();
}
