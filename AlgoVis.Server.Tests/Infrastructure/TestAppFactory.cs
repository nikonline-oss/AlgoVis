using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace AlgoVis.Server.Tests.Infrastructure;

/// <summary>
/// Поднимает приложение в памяти для интеграционных тестов.
/// У каждой фабрики (класса тестов) — СВОЯ уникальная БД.
/// Удаляется при Dispose.
/// </summary>
public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    private const string Host = "postgres";
    private const int Port = 5432;
    private const string User = "algovis";
    private const string Password = "dcLK4uHs6Nphg";

    // Instance fields — уникальны для каждой фабрики.
    private readonly string _dbName;
    private readonly string _testConnString;
    private readonly string _adminConnString;

    public TestAppFactory()
    {
        _dbName = "algovis_test_" + Guid.NewGuid().ToString("N")[..16];
        _testConnString = $"Host={Host};Port={Port};Database={_dbName};Username={User};Password={Password}";
        _adminConnString = $"Host={Host};Port={Port};Database=postgres;Username={User};Password={Password}";

        // Env-vars ДО создания хоста.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _testConnString);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "AlgoVis.Test");
        Environment.SetEnvironmentVariable("Jwt__Audience", "AlgoVis.Test.Client");
        Environment.SetEnvironmentVariable("Jwt__Secret",
            "TEST_SECRET_KEY_MIN_32_CHARS_LONG_FOR_HMAC_SHA256!!!!");
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", "15");
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenDays", "30");
        Environment.SetEnvironmentVariable("RateLimit__RegisterPerHour", "1000000");
        Environment.SetEnvironmentVariable("RateLimit__LoginPerMinute", "1000000");
        Environment.SetEnvironmentVariable("RateLimit__AuthenticatedRunPerMinute", "1000000");
        Environment.SetEnvironmentVariable("RateLimit__AnonymousRunPerMinute", "1000000");

        CreateDatabase();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) DropDatabase();
    }

    private void CreateDatabase()
    {
        using var conn = new NpgsqlConnection(_adminConnString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE \"{_dbName}\" OWNER \"{User}\"";
        cmd.ExecuteNonQuery();
    }

    private void DropDatabase()
    {
        NpgsqlConnection.ClearAllPools();
        try
        {
            using var conn = new NpgsqlConnection(_adminConnString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = '{_dbName}' AND pid <> pg_backend_pid();
                DROP DATABASE IF EXISTS ""{_dbName}"";
            ";
            cmd.ExecuteNonQuery();
        }
        catch { /* best-effort */ }
    }
}