namespace AlgoVis.Data.Entities;

/// <summary>
/// Refresh-токен. Хранится в БД, чтобы можно было отозвать.
/// При обновлении старый помечается RevokedAt и создаётся новый
/// (rotation). Цепочка восстанавливается через ReplacedByTokenId.
/// </summary>
public sealed class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>Случайная строка (base64url, 32+ байта).</summary>
    public string Token { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    /// <summary>ID токена, который пришёл на смену этому (для аудита).</summary>
    public int? ReplacedByTokenId { get; set; }

    /// <summary>User-Agent клиента — для отображения сессий в профиле.</summary>
    public string? UserAgent { get; set; }

    /// <summary>IP-адрес клиента (для security-логов).</summary>
    public string? IpAddress { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
}
