using System.Net;
using Xunit;
using AlgoVis.Server.Tests.Infrastructure;

namespace AlgoVis.Server.Tests;

public sealed class AuthTests : TestBase
{
    public AuthTests(TestAppFactory factory) : base(factory) { }

    // ─────────── Регистрация ───────────

    [Fact]
    public async Task Register_ValidData_ReturnsTokens()
    {
        var auth = await RegisterAsync("reg1@test.local", "reg1");

        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
        Assert.True(auth.UserId > 0);
        Assert.Contains("reg1+", auth.Email);   // email получил суффикс
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup{Guid.NewGuid():N}@test.local";

        var r1 = await Client.PostAsync<object>("/api/auth/register",
            new { email, username = "dup_a", password = "secret123" });
        Assert.True(r1.IsSuccess, $"первая: {r1.Status} {r1.RawText}");

        var r2 = await Client.PostAsync<object>("/api/auth/register",
            new { email, username = "dup_b", password = "secret123" });
        Assert.Equal(400, r2.Status);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400()
    {
        var r = await Client.PostAsync<object>("/api/auth/register",
            new { email = "not-an-email", username = "user", password = "secret123" });
        Assert.Equal(400, r.Status);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        var r = await Client.PostAsync<object>("/api/auth/register",
            new { email = "short@test.local", username = "user", password = "12" });
        Assert.Equal(400, r.Status);
        Assert.Contains("пароль", r.RawText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_ShortUsername_Returns400()
    {
        var r = await Client.PostAsync<object>("/api/auth/register",
            new { email = "su@test.local", username = "a", password = "secret123" });
        Assert.Equal(400, r.Status);
    }

    // ─────────── Логин ───────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var reg = await RegisterAsync("login1@test.local", "login1");

        var r = await Client.PostAsync<AuthResponse>("/api/auth/login",
            new { email = reg.Email, password = "secret123" });

        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.NotEmpty(r.Data!.AccessToken);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var reg = await RegisterAsync("login2@test.local", "login2");

        var r = await Client.PostAsync<object>("/api/auth/login",
            new { email = reg.Email, password = "WRONG" });
        Assert.Equal(401, r.Status);
    }
    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var r = await Client.PostAsync<object>("/api/auth/login",
            new { email = "nobody@test.local", password = "secret123" });
        Assert.Equal(401, r.Status);
    }

    // ─────────── /me ───────────

    [Fact]
    public async Task Me_WithToken_ReturnsUser()
    {
        var auth = await RegisterAsync("me1@test.local", "me1");
        UseAuth(auth);

        var r = await Client.GetAsync<UserDto>("/api/auth/me");
        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.Equal(auth.Email, r.Data!.Email);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        ClearAuth();
        var r = await Client.GetAsync<object>("/api/auth/me");
        Assert.Equal(401, r.Status);
    }

    [Fact]
    public async Task Me_WithInvalidToken_Returns401()
    {
        Client.SetTokens("totally.invalid.jwt");
        var r = await Client.GetAsync<object>("/api/auth/me");
        Assert.Equal(401, r.Status);
    }

    // ─────────── Refresh ───────────

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        var auth = await RegisterAsync("rf1@test.local", "rf1");

        var r = await Client.PostAsync<AuthResponse>("/api/auth/refresh",
            new { refreshToken = auth.RefreshToken });

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.NotEmpty(r.Data!.AccessToken);
        Assert.NotEqual(auth.RefreshToken, r.Data.RefreshToken);
    }

    [Fact]
    public async Task Refresh_ReusedToken_RevokesAll()
    {
        var auth = await RegisterAsync("rf2@test.local", "rf2");

        // Первый refresh успешен
        var r1 = await Client.PostAsync<AuthResponse>("/api/auth/refresh",
            new { refreshToken = auth.RefreshToken });
        Assert.True(r1.IsSuccess);

        // Повторный refresh с тем же токеном — ошибка (rotation)
        var r2 = await Client.PostAsync<object>("/api/auth/refresh",
            new { refreshToken = auth.RefreshToken });
        Assert.Equal(401, r2.Status);

        // И новый refresh-токен тоже отозван (детект кражи → revoke all)
        var r3 = await Client.PostAsync<object>("/api/auth/refresh",
            new { refreshToken = r1.Data!.RefreshToken });
        Assert.Equal(401, r3.Status);
    }

    [Fact]
    public async Task Refresh_InvalidToken_Returns401()
    {
        var r = await Client.PostAsync<object>("/api/auth/refresh",
            new { refreshToken = "nonexistent-token" });
        Assert.Equal(401, r.Status);
    }

    // ─────────── Logout ───────────

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var auth = await RegisterAsync("lo1@test.local", "lo1");

        var r = await Client.PostAsync<object>("/api/auth/logout",
            new { refreshToken = auth.RefreshToken });
        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");

        var r2 = await Client.PostAsync<object>("/api/auth/refresh",
            new { refreshToken = auth.RefreshToken });
        Assert.Equal(401, r2.Status);
    }
}
