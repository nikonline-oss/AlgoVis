using Xunit;
using AlgoVis.Server.Tests.Infrastructure;

namespace AlgoVis.Server.Tests;

public sealed class AdminTests : TestBase
{
    public AdminTests(TestAppFactory factory) : base(factory) { }

    private async Task<AuthResult> SetupAdminAsync()
    {
        var admin = await RegisterAsync("admin@test.local", "admin");
        await MakeAdminAsync(admin.UserId);

        // Перевыпускаем токен, чтобы в JWT была роль admin
        var r = await Client.PostAsync<AuthResponse>("/api/auth/login",
            new { email = admin.Email, password = "secret123" });
        return new AuthResult(
            r.Data!.AccessToken, r.Data.RefreshToken,
            r.Data.User.Id, r.Data.User.Username, admin.Email);
    }

    [Fact]
    public async Task Stats_ReturnsCounts()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);

        var r = await Client.GetAsync<StatsDto>("/api/admin/stats");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.True(r.Data!.TotalUsers >= 1);
        Assert.True(r.Data.AdminCount >= 1);
    }

    [Fact]
    public async Task Stats_NonAdmin_Returns403()
    {
        var student = await RegisterAsync("noadmin@test.local", "noadmin");
        UseAuth(student);

        var r = await Client.GetAsync<object>("/api/admin/stats");
        Assert.Equal(403, r.Status);
    }

    [Fact]
    public async Task Stats_Unauthorized_Returns401()
    {
        ClearAuth();
        var r = await Client.GetAsync<object>("/api/admin/stats");
        Assert.Equal(401, r.Status);
    }

    [Fact]
    public async Task ListUsers_ReturnsPaged()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);

        var r = await Client.GetAsync<UserListDto>("/api/admin/users?page=1&pageSize=10");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.True(r.Data!.TotalCount >= 1);
        Assert.Equal(1, r.Data.Page);
    }

    [Fact]
    public async Task ListUsers_SearchByEmail()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);

        var target = await RegisterAsync("searchme@test.local", "searchme");

        var r = await Client.GetAsync<UserListDto>(
            "/api/admin/users?search=searchme");
        Assert.NotNull(r.Data);
        Assert.Contains(r.Data!.Users, u => u.Email == target.Email);
    }

    [Fact]
    public async Task ChangeRole_ToTeacher_Works()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);

        var target = await RegisterAsync("torole@test.local", "torole");

        var r = await Client.PatchAsync<AdminUserDto>($"/api/admin/users/{target.UserId}/role",
            new { role = "teacher" });
        Assert.True(r.IsSuccess);
        Assert.Equal("teacher", r.Data!.Role);
    }

    [Fact]
    public async Task ChangeRole_InvalidRole_Returns400()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);
        var target = await RegisterAsync("badrole@test.local", "badrole");

        var r = await Client.PatchAsync<object>($"/api/admin/users/{target.UserId}/role",
            new { role = "superhero" });
        Assert.Equal(400, r.Status);
    }

    [Fact]
    public async Task ChangeRole_LastAdmin_Returns400()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);

        var r = await Client.PatchAsync<object>($"/api/admin/users/{admin.UserId}/role",
            new { role = "student" });
        Assert.Equal(400, r.Status);
        Assert.Contains("последнего администратора", r.RawText);
    }

    [Fact]
    public async Task ChangeActive_BlocksUser()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);
        var target = await RegisterAsync("toblock@test.local", "toblock");

        var r = await Client.PatchAsync<AdminUserDto>($"/api/admin/users/{target.UserId}/active",
            new { isActive = false });
        Assert.True(r.IsSuccess);
        Assert.False(r.Data!.IsActive);

        // Проверяем, что логин не проходит
        ClearAuth();
        var login = await Client.PostAsync<object>("/api/auth/login",
            new { email = target.Email, password = "secret123" });
        Assert.Equal(401, login.Status);
        Assert.Contains("заблокирован", login.RawText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangeActive_Self_Returns400()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);

        var r = await Client.PatchAsync<object>($"/api/admin/users/{admin.UserId}/active",
            new { isActive = false });
        Assert.Equal(400, r.Status);
    }

    [Fact]
    public async Task DeleteUser_Self_Returns400()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);

        var r = await Client.DeleteAsync<object>($"/api/admin/users/{admin.UserId}");
        Assert.Equal(400, r.Status);
    }

    [Fact]
    public async Task DeleteUser_WithAssignments_Returns400()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);

        var teacher = await RegisterAsync("delteacher@test.local", "delteacher");
        UseAuth(teacher);
        await CreateAssignmentAsync("HasAsg", publish: false);

        UseAuth(admin);
        var r = await Client.DeleteAsync<object>($"/api/admin/users/{teacher.UserId}");
        Assert.Equal(400, r.Status);
        Assert.Contains("созданные задания", r.RawText);
    }

    [Fact]
    public async Task DeleteUser_FreshStudent_Works()
    {
        var admin = await SetupAdminAsync();
        UseAuth(admin);
        var target = await RegisterAsync("fresh@test.local", "fresh");

        var r = await Client.DeleteAsync<object>($"/api/admin/users/{target.UserId}");
        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
    }

    // ─────────── DTO ───────────

    public sealed class StatsDto
    {
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int StudentCount { get; set; }
    }

    public sealed class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public sealed class UserListDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<AdminUserDto> Users { get; set; } = new();
    }
}
