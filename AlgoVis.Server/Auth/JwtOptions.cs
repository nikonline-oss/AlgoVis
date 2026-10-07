namespace AlgoVis.Server.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "AlgoVis";
    public string Audience { get; set; } = "AlgoVis.Client";
    public string Secret { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Secret) || Secret.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Secret должен быть установлен и быть не короче 32 символов. " +
                "Сгенерируйте через 'openssl rand -base64 48'.");
    }
}
