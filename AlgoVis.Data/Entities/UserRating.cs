namespace AlgoVis.Data.Entities;

/// <summary>
/// Рейтинги пользователя. Обновляются при завершении submission
/// и при создании/прохождении задания.
/// </summary>
public sealed class UserRating
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>Опыт игрока — за выполнение чужих заданий.</summary>
    public long PlayerXp { get; set; }

    /// <summary>Уровень игрока (вычисляется из XP).</summary>
    public int PlayerLevel { get; set; } = 1;

    /// <summary>Количество выполненных заданий.</summary>
    public int CompletedCount { get; set; }

    /// <summary>Рейтинг преподавателя — за создание заданий, которые проходят.</summary>
    public long TeacherRating { get; set; }

    /// <summary>Уровень преподавателя.</summary>
    public int TeacherLevel { get; set; } = 1;

    /// <summary>Количество созданных заданий.</summary>
    public int CreatedCount { get; set; }

    /// <summary>Общее число успешных прохождений заданий этого автора.</summary>
    public int TotalAssignmentsCompleted { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
