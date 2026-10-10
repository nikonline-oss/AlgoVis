namespace AlgoVis.Server.RateLimiting;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    /// <summary>Запусков в минуту для авторизованного пользователя.</summary>
    public int AuthenticatedRunPerMinute { get; set; } = 30;

    /// <summary>Запусков в минуту для анонимного IP.</summary>
    public int AnonymousRunPerMinute { get; set; } = 10;

    /// <summary>Регистраций в час с одного IP.</summary>
    public int RegisterPerHour { get; set; } = 5;

    /// <summary>Попыток логина в минуту с одного IP.</summary>
    public int LoginPerMinute { get; set; } = 10;
}
