using AlgoVis.Data;
using AlgoVis.Data.Entities;
using AlgoVis.Server.Auth.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Auth.Services;

public sealed class AuthService
{
    private readonly AlgoVisDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly JwtService _jwt;
    private readonly ILogger<AuthService> _log;

    public AuthService(AlgoVisDbContext db, PasswordHasher hasher, JwtService jwt,
                       ILogger<AuthService> log)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _log = log;
    }

    // ─────────── Register ───────────

    public async Task<(TokenResponse? Result, string? Error)> RegisterAsync(
        RegisterRequest req, string? userAgent, string? ip, CancellationToken ct = default)
    {
        var email = NormalizeEmail(req.Email);
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return (null, "Некорректный email");

        if (string.IsNullOrWhiteSpace(req.Username) || req.Username.Length < 3)
            return (null, "Имя пользователя должно быть не короче 3 символов");

        if (string.IsNullOrEmpty(req.Password) || req.Password.Length < 6)
            return (null, "Пароль должен быть не короче 6 символов");

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            return (null, "Пользователь с таким email уже зарегистрирован");

        var user = new User
        {
            Email = email,
            Username = req.Username.Trim(),
            PasswordHash = _hasher.Hash(req.Password),
            Role = "student"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        _log.LogInformation("Registered user {Email} (id={Id})", user.Email, user.Id);

        var result = await IssueTokensAsync(user, userAgent, ip, ct);
        return (result, null);
    }

    // ─────────── Login ───────────

    public async Task<(TokenResponse? Result, string? Error)> LoginAsync(
        LoginRequest req, string? userAgent, string? ip, CancellationToken ct = default)
    {
        var email = NormalizeEmail(req.Email);
        if (string.IsNullOrEmpty(email))
            return (null, "Некорректный email");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null || !_hasher.Verify(req.Password, user.PasswordHash))
            return (null, "Неверный email или пароль");

        if (!user.IsActive)
            return (null, "Аккаунт заблокирован");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _log.LogInformation("Login user {Email} (id={Id})", user.Email, user.Id);

        var result = await IssueTokensAsync(user, userAgent, ip, ct);
        return (result, null);
    }

    // ─────────── Refresh (with rotation) ───────────

    public async Task<(TokenResponse? Result, string? Error)> RefreshAsync(
        string refreshToken, string? userAgent, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return (null, "Отсутствует refresh token");

        var stored = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (stored is null)
            return (null, "Refresh token не найден");

        if (stored.RevokedAt is not null)
        {
            // Кто-то использует отозванный токен — возможна кража.
            _log.LogWarning("Attempt to reuse revoked token {TokenId}, user {UserId}. " +
                            "Revoking all user tokens.", stored.Id, stored.UserId);
            var allActive = await _db.RefreshTokens
                .Where(t => t.UserId == stored.UserId && t.RevokedAt == null)
                .ToListAsync(ct);
            foreach (var t in allActive) t.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return (null, "Refresh token был отозван");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
            return (null, "Refresh token истёк");

        stored.RevokedAt = DateTime.UtcNow;

        var result = await IssueTokensAsync(stored.User, userAgent, ip, ct,
                                            replacedTokenId: stored.Id);
        return (result, null);
    }

    // ─────────── Logout ───────────

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return false;

        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);
        if (stored is null || stored.RevokedAt is not null) return false;

        stored.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─────────── Internal ───────────

    private async Task<TokenResponse> IssueTokensAsync(
        User user, string? userAgent, string? ip, CancellationToken ct,
        int? replacedTokenId = null)
    {
        var access = _jwt.CreateAccessToken(user);
        var refreshValue = JwtService.CreateRefreshToken();

        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            UserAgent = Truncate(userAgent, 500),
            IpAddress = Truncate(ip, 50)
        };
        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync(ct);

        if (replacedTokenId.HasValue)
        {
            var old = await _db.RefreshTokens.FindAsync(new object?[] { replacedTokenId.Value }, ct);
            if (old is not null)
            {
                old.ReplacedByTokenId = refresh.Id;
                await _db.SaveChangesAsync(ct);
            }
        }

        return new TokenResponse(
            access,
            refreshValue,
            _jwt.AccessTokenMinutes * 60,
            ToUserInfo(user));
    }

    private static UserInfoDto ToUserInfo(User u) =>
        new(u.Id, u.Email, u.Username, u.Role, u.DefaultLanguage,
            u.CreatedAt, u.LastLoginAt);

    private static string NormalizeEmail(string email) =>
        (email ?? "").Trim().ToLowerInvariant();

    private static string? Truncate(string? s, int max) =>
        s is null ? null : (s.Length <= max ? s : s[..max]);
}
