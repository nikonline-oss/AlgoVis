using AlgoVis.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace AlgoVis.Server.Tests.Infrastructure;

/// <summary>
/// Базовый класс для всех интеграционных тестов.
/// Перед каждым тестом все таблицы очищаются (TRUNCATE), чтобы каждый
/// тест стартовал с чистой БД. Так избегаем коллизий и хардкода email.
/// </summary>
public abstract class TestBase : IClassFixture<TestAppFactory>, IAsyncLifetime
{
    protected readonly TestAppFactory Factory;
    protected readonly ApiClient Client;
    protected readonly IServiceScope Scope;
    protected readonly AlgoVisDbContext Db;

    protected TestBase(TestAppFactory factory)
    {
        Factory = factory;
        Client = new ApiClient(factory.CreateClient());
        Scope = factory.Services.CreateScope();
        Db = Scope.ServiceProvider.GetRequiredService<AlgoVisDbContext>();
    }

    // ─────────── IAsyncLifetime ───────────

    public async Task InitializeAsync()
    {
        // Открываем отдельное соединение, чтобы не мешать DbContext.
        var connStr = Db.Database.GetConnectionString();
        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            TRUNCATE TABLE
                comments,
                submissions,
                assignments,
                refresh_tokens,
                projects,
                user_ratings,
                users
            RESTART IDENTITY CASCADE;
        ";
        cmd.CommandTimeout = 30;
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync()
    {
        Scope.Dispose();
        return Task.CompletedTask;
    }

    // ─────────── Утилиты ───────────

    protected async Task<AuthResult> RegisterAsync(
        string emailBase, string usernameBase, string password = "secret123")
    {
        var sfx = Guid.NewGuid().ToString("N")[..8];
        var email = emailBase.Contains('@')
            ? emailBase.Replace("@", $"+{sfx}@")
            : $"{emailBase}+{sfx}@test.local";
        var username = $"{usernameBase}_{sfx}";

        var r = await Client.PostAsync<AuthResponse>("/api/auth/register",
            new { email, username, password });

        if (!r.IsSuccess || r.Data is null)
            throw new InvalidOperationException(
                $"Не удалось зарегистрировать {email}: {r.Status} {r.RawText}");

        return new AuthResult(
            r.Data.AccessToken, r.Data.RefreshToken,
            r.Data.User.Id, r.Data.User.Username, email);
    }

    protected async Task<AuthResult> LoginAsync(string email, string password = "secret123")
    {
        var r = await Client.PostAsync<AuthResponse>("/api/auth/login",
            new { email, password });
        if (!r.IsSuccess || r.Data is null)
            throw new InvalidOperationException(
                $"Не удалось залогиниться {email}: {r.Status} {r.RawText}");
        return new AuthResult(
            r.Data.AccessToken, r.Data.RefreshToken,
            r.Data.User.Id, r.Data.User.Username, email);
    }

    protected void UseAuth(AuthResult auth) =>
        Client.SetTokens(auth.AccessToken, auth.RefreshToken);

    protected void ClearAuth() => Client.ClearTokens();

    protected async Task MakeAdminAsync(int userId)
    {
        var u = await Db.Users.FindAsync(userId);
        if (u is null) throw new InvalidOperationException($"Пользователь {userId} не найден");
        u.Role = "admin";
        await Db.SaveChangesAsync();
    }

    // ─────────── Общие константы ───────────

    protected const string BubbleSortCode = """
def bubble_sort(A):
    n = len(A)
    for i in range(n):
        for j in range(n - i - 1):
            if A[j] > A[j + 1]:
                A[j], A[j + 1] = A[j + 1], A[j]
    return A

def main():
    A = [5, 2, 8, 1, 9, 3]
    bubble_sort(A)
""";

    protected const string WrongSortCode = """
def bubble_sort(A):
    n = len(A)
    for i in range(n):
        for j in range(n - i - 1):
            if A[j] < A[j + 1]:
                A[j], A[j + 1] = A[j + 1], A[j]
    return A

def main():
    A = [5, 2, 8, 1, 9, 3]
    bubble_sort(A)
""";

    // ─────────── Хелперы ───────────

    protected async Task<int> CreateProjectAsync(string name, string? code = null)
    {
        var r = await Client.PostAsync<ProjectDto>("/api/projects",
            new { name, description = (string?)null, pythonCode = code });
        if (!r.IsSuccess || r.Data is null)
            throw new InvalidOperationException(
                $"Не удалось создать проект: {r.Status} {r.RawText}");
        return r.Data.Id;
    }

    protected async Task<int> CreateAssignmentAsync(
        string title,
        string referenceSolution = BubbleSortCode,
        string compareTarget = "A",
        string mode = "visual",
        bool publish = true)
    {
        var r = await Client.PostAsync<AssignmentDto>("/api/assignments", new
        {
            title,
            description = "Автотест",
            mode,
            allowedLanguages = new List<string>(),
            referenceSolution,
            referenceLanguage = "python",
            compareTarget,
            isPublished = false,
            isPublic = true
        });
        if (!r.IsSuccess || r.Data is null)
            throw new InvalidOperationException(
                $"Не удалось создать задание: {r.Status} {r.RawText}");

        var id = r.Data.Id;

        if (publish)
        {
            var pub = await Client.PostAsync<object>($"/api/assignments/{id}/publish", null);
            if (!pub.IsSuccess)
                throw new InvalidOperationException(
                    $"Не удалось опубликовать задание: {pub.Status} {pub.RawText}");
        }

        return id;
    }

    // ─────────── DTO ───────────

    protected sealed record AuthResult(
        string AccessToken, string RefreshToken, int UserId, string Username, string Email);

    public sealed class AuthResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public int ExpiresInSeconds { get; set; }
        public UserDto User { get; set; } = new();
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "student";
        public string DefaultLanguage { get; set; } = "python";
    }

    public sealed class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class AssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public bool IsPublished { get; set; }
    }
}