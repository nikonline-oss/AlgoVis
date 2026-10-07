using Xunit;
using AlgoVis.Server.Tests.Infrastructure;

namespace AlgoVis.Server.Tests;

public sealed class ProjectsTests : TestBase
{
    public ProjectsTests(TestAppFactory factory) : base(factory) { }

    // ─────────── CRUD ───────────

    [Fact]
    public async Task Create_ValidData_ReturnsProject()
    {
        var auth = await RegisterAsync("p1@test.local", "p1");
        UseAuth(auth);

        var r = await Client.PostAsync<ProjectDto>("/api/projects",
            new { name = "My Project", description = "desc", pythonCode = "print(1)" });

        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.True(r.Data!.Id > 0);
        Assert.Equal("My Project", r.Data.Name);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var auth = await RegisterAsync("p2@test.local", "p2");
        UseAuth(auth);

        var r = await Client.PostAsync<object>("/api/projects",
            new { name = "", description = "", pythonCode = "print(1)" });
        Assert.Equal(400, r.Status);
    }

    [Fact]
    public async Task List_ReturnsOnlyMine()
    {
        var a = await RegisterAsync("list1@test.local", "list1");
        UseAuth(a);
        await CreateProjectAsync("A-Proj-1");
        await CreateProjectAsync("A-Proj-2");

        var b = await RegisterAsync("list2@test.local", "list2");
        UseAuth(b);
        await CreateProjectAsync("B-Proj-1");

        UseAuth(a);
        var r = await Client.GetAsync<List<ProjectDto>>("/api/projects");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal(2, r.Data!.Count);
        Assert.All(r.Data!, p => Assert.Contains("A-Proj", p.Name));
    }

    [Fact]
    public async Task List_WithoutAuth_Returns401()
    {
        ClearAuth();
        var r = await Client.GetAsync<object>("/api/projects");
        Assert.Equal(401, r.Status);
    }

    [Fact]
    public async Task Get_OwnProject_ReturnsDetails()
    {
        var auth = await RegisterAsync("g1@test.local", "g1");
        UseAuth(auth);
        var id = await CreateProjectAsync("Detail", "print('hi')");

        var r = await Client.GetAsync<ProjectDetailDto>($"/api/projects/{id}");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal("Detail", r.Data!.Name);
        Assert.Equal("print('hi')", r.Data.PythonCode);
    }

    [Fact]
    public async Task Get_OthersProject_Returns404()
    {
        var a = await RegisterAsync("iso1@test.local", "iso1");
        UseAuth(a);
        var id = await CreateProjectAsync("Secret");

        var b = await RegisterAsync("iso2@test.local", "iso2");
        UseAuth(b);
        var r = await Client.GetAsync<object>($"/api/projects/{id}");
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task Update_OwnProject_ChangesFields()
    {
        var auth = await RegisterAsync("u1@test.local", "u1");
        UseAuth(auth);
        var id = await CreateProjectAsync("Before");

        var r = await Client.PatchAsync<ProjectDto>($"/api/projects/{id}",
            new { name = "After" });
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal("After", r.Data!.Name);
    }

    [Fact]
    public async Task Update_OthersProject_Returns404()
    {
        var a = await RegisterAsync("u2@test.local", "u2");
        UseAuth(a);
        var id = await CreateProjectAsync("Mine");

        var b = await RegisterAsync("u3@test.local", "u3");
        UseAuth(b);
        var r = await Client.PatchAsync<object>($"/api/projects/{id}",
            new { name = "Hacked" });
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task Delete_OwnProject_RemovesIt()
    {
        var auth = await RegisterAsync("d1@test.local", "d1");
        UseAuth(auth);
        var id = await CreateProjectAsync("ToDelete");

        var r = await Client.DeleteAsync<object>($"/api/projects/{id}");
        Assert.True(r.IsSuccess);

        var r2 = await Client.GetAsync<object>($"/api/projects/{id}");
        Assert.Equal(404, r2.Status);
    }

    [Fact]
    public async Task Delete_OthersProject_Returns404()
    {
        var a = await RegisterAsync("d2@test.local", "d2");
        UseAuth(a);
        var id = await CreateProjectAsync("Mine");

        var b = await RegisterAsync("d3@test.local", "d3");
        UseAuth(b);
        var r = await Client.DeleteAsync<object>($"/api/projects/{id}");
        Assert.Equal(404, r.Status);
    }

    // ─────────── Публикация ───────────

    [Fact]
    public async Task Publish_SetsSlugAndPublicFlag()
    {
        var auth = await RegisterAsync("pub1@test.local", "pub1");
        UseAuth(auth);
        var id = await CreateProjectAsync("ForShare", "print(1)");

        var r = await Client.PostAsync<ProjectDetailDto>($"/api/projects/{id}/publish", null);
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.True(r.Data!.IsPublic);
        Assert.NotNull(r.Data.PublicSlug);
        Assert.NotEmpty(r.Data.PublicSlug!);
    }

    [Fact]
    public async Task Publish_Idempotent_KeepsSameSlug()
    {
        var auth = await RegisterAsync("pub2@test.local", "pub2");
        UseAuth(auth);
        var id = await CreateProjectAsync("Idem");

        var r1 = await Client.PostAsync<ProjectDetailDto>($"/api/projects/{id}/publish", null);
        var slug1 = r1.Data!.PublicSlug;

        var r2 = await Client.PostAsync<ProjectDetailDto>($"/api/projects/{id}/publish", null);
        Assert.Equal(slug1, r2.Data!.PublicSlug);
    }

    [Fact]
    public async Task PublicAccess_BySlug_WorksWithoutAuth()
    {
        var auth = await RegisterAsync("pub3@test.local", "pub3");
        UseAuth(auth);
        var id = await CreateProjectAsync("Public", "def main(): pass");
        var pub = await Client.PostAsync<ProjectDetailDto>($"/api/projects/{id}/publish", null);
        var slug = pub.Data!.PublicSlug;

        ClearAuth();
        var r = await Client.GetAsync<PublicProjectDto>($"/api/public/projects/{slug}");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal("Public", r.Data!.Name);
        Assert.Equal(auth.Username, r.Data.OwnerUsername);
    }

    [Fact]
    public async Task PublicAccess_UnpublishedSlug_Returns404()
    {
        var auth = await RegisterAsync("pub4@test.local", "pub4");
        UseAuth(auth);
        var id = await CreateProjectAsync("WillUnpub");
        var pub = await Client.PostAsync<ProjectDetailDto>($"/api/projects/{id}/publish", null);
        var slug = pub.Data!.PublicSlug;

        await Client.DeleteAsync<object>($"/api/projects/{id}/publish");

        ClearAuth();
        var r = await Client.GetAsync<object>($"/api/public/projects/{slug}");
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task PublicAccess_UnknownSlug_Returns404()
    {
        ClearAuth();
        var r = await Client.GetAsync<object>("/api/public/projects/doesnotexist123");
        Assert.Equal(404, r.Status);
    }

    // ─────────── DTO ───────────

    public sealed class ProjectDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? PythonCode { get; set; }
        public bool IsPublic { get; set; }
        public string? PublicSlug { get; set; }
    }

    public sealed class PublicProjectDto
    {
        public string Name { get; set; } = "";
        public string OwnerUsername { get; set; } = "";
    }
}
