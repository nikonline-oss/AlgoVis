namespace AlgoVis.Data.Entities;

/// <summary>
/// Сохранённый код пользователя. Хранит исходник Python и/или YAWA-JSON.
/// </summary>
public sealed class Project
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = "";
    public string? Description { get; set; }

    /// <summary>Python-исходник. Может быть пустым, если проект создан как YAWA.</summary>
    public string? PythonCode { get; set; }

    /// <summary>Скомпилированная YAWA-программа (JSON). Может быть пустым.</summary>
    public string? YawaJson { get; set; }

    /// <summary>Если true — доступен по публичной ссылке без авторизации.</summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>Слаг для публичной ссылки (генерируется при публикации).</summary>
    public string? PublicSlug { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Счётчик для быстрого отображения в списке
    public int LastRunSteps { get; set; }
    public int LastRunComparisons { get; set; }
    public int LastRunSwaps { get; set; }
}
