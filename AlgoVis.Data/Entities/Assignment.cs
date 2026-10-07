namespace AlgoVis.Data.Entities;

/// <summary>
/// Задание — задача, созданная преподавателем.
/// Ученик отправляет решение (Submission), система проверяет его
/// по заданным критериям и выставляет грейд.
/// </summary>
public sealed class Assignment
{
    public int Id { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Title { get; set; } = "";
    public string? Description { get; set; }

    /// <summary>Режим: "visual" (язык не ограничен) или "code" (список языков).</summary>
    public string Mode { get; set; } = "visual";

    /// <summary>Разрешённые языки для mode=code. Для visual — пусто.</summary>
    public List<string> AllowedLanguages { get; set; } = new();

    /// <summary>
    /// Эталонное решение. Запускается при создании, чтобы автор
    /// увидел ожидаемые final_state, шаги, статистику.
    /// </summary>
    public string ReferenceSolution { get; set; } = "";

    /// <summary>Язык эталонного решения (влияет на транспайлер).</summary>
    public string ReferenceLanguage { get; set; } = "python";

    /// <summary>
    /// Имя переменной/функции, значение которой сравнивается
    /// (например, "A" или "__return__").
    /// </summary>
    public string CompareTarget { get; set; } = "__return__";

    /// <summary>
    /// Ожидаемый результат (JSON-сериализованное значение).
    /// Сравнивается с final_state[CompareTarget] решения ученика.
    /// </summary>
    public string? ExpectedResult { get; set; }

    /// <summary>
    /// Критерии для грейдов (JSON). Формат:
    /// {
    ///   "good":      {"required": ["result"]},
    ///   "excellent": {"required": ["result"], "maxComparisons": 100},
    ///   "perfect":   {"required": ["result"], "maxComparisons": 28, "maxSwaps": 13}
    /// }
    /// </summary>
    public string? GradingRules { get; set; }

    /// <summary>Шаблон кода для ученика (опционально).</summary>
    public string? TemplateCode { get; set; }

    public int MaxSteps { get; set; } = 100_000;
    public double MaxSeconds { get; set; } = 5.0;

    /// <summary>Опубликовано ли задание (видно ученикам).</summary>
    public bool IsPublished { get; set; }

    /// <summary>Публичное задание (видно всем авторизованным).</summary>
    public bool IsPublic { get; set; } = true;

    public DateTime? Deadline { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Навигация
    public List<Submission> Submissions { get; set; } = new();
}
