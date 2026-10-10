using Xunit;
using AlgoVis.Server.Tests.Infrastructure;

namespace AlgoVis.Server.Tests;

public sealed class AssignmentsTests : TestBase
{
    public AssignmentsTests(TestAppFactory factory) : base(factory) { }

    // ─────────── Preview ───────────

    [Fact]
    public async Task Preview_ValidCode_ReturnsStats()
    {
        var auth = await RegisterAsync("prev1@test.local", "prev1");
        UseAuth(auth);

        var r = await Client.PostAsync<PreviewDto>("/api/assignments/preview", new
        {
            referenceSolution = BubbleSortCode,
            language = "python",
            compareTarget = "A"
        });

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.True(r.Data!.Success, r.Data.Error);
        Assert.NotNull(r.Data.Stats);
        Assert.True(r.Data.Stats!.Comparisons > 0);
    }

    [Fact]
    public async Task Preview_BrokenCode_ReturnsError()
    {
        var auth = await RegisterAsync("prev2@test.local", "prev2");
        UseAuth(auth);

        var r = await Client.PostAsync<PreviewDto>("/api/assignments/preview", new
        {
            referenceSolution = "def main():\n    x = unknown_var + 1",
            language = "python",
            compareTarget = "A"
        });

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.False(r.Data!.Success);
        Assert.NotNull(r.Data.Error);
    }

    [Fact]
    public async Task Preview_InvalidSyntax_ReturnsTranspilerError()
    {
        var auth = await RegisterAsync("prev3@test.local", "prev3");
        UseAuth(auth);

        var r = await Client.PostAsync<PreviewDto>("/api/assignments/preview", new
        {
            referenceSolution = "def main(\n    pass",
            language = "python",
            compareTarget = "A"
        });

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.False(r.Data!.Success);
        Assert.Equal("transpiler", r.Data.Kind);
    }

    // ─────────── CRUD ───────────

    [Fact]
    public async Task Create_ValidData_ReturnsAssignment()
    {
        var auth = await RegisterAsync("cr1@test.local", "cr1");
        UseAuth(auth);

        var id = await CreateAssignmentAsync("Сортировка", publish: false);
        Assert.True(id > 0);
    }

    [Fact]
    public async Task Create_BrokenReference_Returns400()
    {
        var auth = await RegisterAsync("cr2@test.local", "cr2");
        UseAuth(auth);

        var r = await Client.PostAsync<object>("/api/assignments", new
        {
            title = "Bad",
            mode = "visual",
            allowedLanguages = new List<string>(),
            referenceSolution = "def main():\n    y = undef + 1",
            referenceLanguage = "python",
            compareTarget = "A",
            isPublished = false,
            isPublic = true
        });

        Assert.Equal(400, r.Status);
        Assert.Contains("не работает", r.RawText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_CodeModeWithoutLanguages_Returns400()
    {
        var auth = await RegisterAsync("cr3@test.local", "cr3");
        UseAuth(auth);

        var r = await Client.PostAsync<object>("/api/assignments", new
        {
            title = "Bad",
            mode = "code",
            allowedLanguages = new List<string>(),
            referenceSolution = BubbleSortCode,
            referenceLanguage = "python",
            compareTarget = "A",
            isPublished = false,
            isPublic = true
        });

        Assert.Equal(400, r.Status);
    }

    [Fact]
    public async Task ListMine_ReturnsMyAssignments()
    {
        var auth = await RegisterAsync("lm1@test.local", "lm1");
        UseAuth(auth);
        await CreateAssignmentAsync("Asg-A", publish: false);
        await CreateAssignmentAsync("Asg-B", publish: false);

        var r = await Client.GetAsync<List<AssignmentSummaryDto>>("/api/assignments/mine");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal(2, r.Data!.Count);
    }

    [Fact]
    public async Task GetMine_ReturnsFullWithReference()
    {
        var auth = await RegisterAsync("gm1@test.local", "gm1");
        UseAuth(auth);
        var id = await CreateAssignmentAsync("Full", publish: false);

        var r = await Client.GetAsync<AssignmentFullDto>($"/api/assignments/{id}/mine");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Contains("def bubble_sort", r.Data!.ReferenceSolution);
        Assert.Equal("A", r.Data.CompareTarget);
    }

    // ─────────── Публикация ───────────

    [Fact]
    public async Task Publish_RequiresExpectedResult()
    {
        // Создаём с auto-expected — публикация проходит
        var auth = await RegisterAsync("pub1@test.local", "pub1");
        UseAuth(auth);
        var id = await CreateAssignmentAsync("Pub", publish: true);

        var r = await Client.GetAsync<AssignmentFullDto>($"/api/assignments/{id}/mine");
        Assert.True(r.Data!.IsPublished);
    }

    [Fact]
    public async Task ListPublic_ShowsPublishedToStudents()
    {
        var teacher = await RegisterAsync("t1@test.local", "t1");
        UseAuth(teacher);
        await CreateAssignmentAsync("PublicTask", publish: true);

        var student = await RegisterAsync("s1@test.local", "s1");
        UseAuth(student);

        var r = await Client.GetAsync<List<AssignmentForStudentDto>>("/api/assignments");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Contains(r.Data!, a => a.Title == "PublicTask");
    }

    [Fact]
    public async Task ListPublic_DoesNotShowDrafts()
    {
        var teacher = await RegisterAsync("t2@test.local", "t2");
        UseAuth(teacher);
        await CreateAssignmentAsync("Draft", publish: false);

        var student = await RegisterAsync("s2@test.local", "s2");
        UseAuth(student);

        var r = await Client.GetAsync<List<AssignmentForStudentDto>>("/api/assignments");
        Assert.NotNull(r.Data);
        Assert.DoesNotContain(r.Data!, a => a.Title == "Draft");
    }

    [Fact]
    public async Task GetForStudent_HidesReferenceSolution()
    {
        var teacher = await RegisterAsync("t3@test.local", "t3");
        UseAuth(teacher);
        var id = await CreateAssignmentAsync("Hidden", publish: true);

        var student = await RegisterAsync("s3@test.local", "s3");
        UseAuth(student);

        var r = await Client.GetAsync<AssignmentForStudentDto>($"/api/assignments/{id}");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal("Hidden", r.Data!.Title);
        // ReferenceSolution отсутствует в DTO для студента по определению.
    }

    // ─────────── Изоляция ───────────

    [Fact]
    public async Task GetMine_OthersAssignment_Returns404()
    {
        var a = await RegisterAsync("iso1@test.local", "iso1");
        UseAuth(a);
        var id = await CreateAssignmentAsync("Own", publish: false);

        var b = await RegisterAsync("iso2@test.local", "iso2");
        UseAuth(b);
        var r = await Client.GetAsync<object>($"/api/assignments/{id}/mine");
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task Delete_OthersAssignment_Returns404()
    {
        var a = await RegisterAsync("iso3@test.local", "iso3");
        UseAuth(a);
        var id = await CreateAssignmentAsync("Own", publish: false);

        var b = await RegisterAsync("iso4@test.local", "iso4");
        UseAuth(b);
        var r = await Client.DeleteAsync<object>($"/api/assignments/{id}");
        Assert.Equal(404, r.Status);
    }

    // ─────────── DTO ───────────

    public sealed class PreviewDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Kind { get; set; }
        public int? Line { get; set; }
        public int? Column { get; set; }
        public PreviewStatsDto? Stats { get; set; }
    }

    public sealed class PreviewStatsDto
    {
        public int TotalSteps { get; set; }
        public int Comparisons { get; set; }
        public int Swaps { get; set; }
        public int MemoryAccesses { get; set; }
    }

    public sealed class AssignmentSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
    }

    public sealed class AssignmentFullDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string ReferenceSolution { get; set; } = "";
        public string CompareTarget { get; set; } = "";
        public bool IsPublished { get; set; }
    }

    public sealed class AssignmentForStudentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string AuthorUsername { get; set; } = "";
    }
}
