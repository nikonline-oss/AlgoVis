namespace AlgoVis.Data.Entities;

public sealed class User
{
    public int Id { get; set; }

    /// <summary>Email — уникален, используется для входа.</summary>
    public string Email { get; set; } = "";

    /// <summary>Отображаемое имя.</summary>
    public string Username { get; set; } = "";

    /// <summary>BCrypt-хеш пароля.</summary>
    public string PasswordHash { get; set; } = "";

    /// <summary>Роль: "student" / "teacher" / "admin".</summary>
    public string Role { get; set; } = "student";

    /// <summary>Язык программирования по умолчанию (сохраняется из последнего выбора).</summary>
    public string DefaultLanguage { get; set; } = "python";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Навигация
    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}
