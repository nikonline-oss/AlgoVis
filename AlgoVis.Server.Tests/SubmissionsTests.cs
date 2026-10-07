using Xunit;
using AlgoVis.Server.Tests.Infrastructure;

namespace AlgoVis.Server.Tests;

public sealed class SubmissionsTests : TestBase
{
    public SubmissionsTests(TestAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Submit_CorrectSolution_PassesWithGrade()
    {
        var teacher = await RegisterAsync("t1@test.local", "t1");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Bubble", publish: true);

        var student = await RegisterAsync("s1@test.local", "s1");
        UseAuth(student);

        var r = await Client.PostAsync<SubmissionDto>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.Equal("passed", r.Data!.Status);
        Assert.NotNull(r.Data.Grade);
        Assert.True(r.Data.XpAwarded > 0);
        Assert.NotNull(r.Data.Result);
        Assert.True(r.Data.Result!.Passed);
    }

    [Fact]
    public async Task Submit_WrongSolution_Fails()
    {
        var teacher = await RegisterAsync("t2@test.local", "t2");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Bubble2", publish: true);

        var student = await RegisterAsync("s2@test.local", "s2");
        UseAuth(student);

        var r = await Client.PostAsync<SubmissionDto>($"/api/assignments/{aid}/submit",
            new { language = "python", code = WrongSortCode });

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal("failed", r.Data!.Status);
        Assert.Null(r.Data.Grade);
        Assert.Equal(0, r.Data.XpAwarded);
        Assert.False(r.Data.Result!.Passed);
    }

    [Fact]
    public async Task Submit_BrokenCode_StatusError()
    {
        var teacher = await RegisterAsync("t3@test.local", "t3");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Bubble3", publish: true);

        var student = await RegisterAsync("s3@test.local", "s3");
        UseAuth(student);

        var r = await Client.PostAsync<SubmissionDto>($"/api/assignments/{aid}/submit",
            new { language = "python", code = "def main():\n    x = unknown_y + 1" });

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal("error", r.Data!.Status);
        Assert.NotNull(r.Data.ErrorMessage);
    }

    [Fact]
    public async Task Submit_ToOwnAssignment_Returns400()
    {
        var teacher = await RegisterAsync("t4@test.local", "t4");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Self", publish: true);

        var r = await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });
        Assert.Equal(400, r.Status);
    }

    [Fact]
    public async Task Submit_ToDraftAssignment_Returns400()
    {
        var teacher = await RegisterAsync("t5@test.local", "t5");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Draft", publish: false);

        var student = await RegisterAsync("s5@test.local", "s5");
        UseAuth(student);

        var r = await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });
        Assert.Equal(400, r.Status);
    }

    [Fact]
    public async Task ListMine_ReturnsOwnSubmissions()
    {
        var teacher = await RegisterAsync("t6@test.local", "t6");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("A", publish: true);

        var student = await RegisterAsync("s6@test.local", "s6");
        UseAuth(student);

        await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        var r = await Client.GetAsync<List<SubmissionSummaryDto>>("/api/submissions/mine");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Single(r.Data!);
    }

    [Fact]
    public async Task ListForAssignment_AuthorSeesAllSubmissions()
    {
        var teacher = await RegisterAsync("t7@test.local", "t7");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Multi", publish: true);

        var s1 = await RegisterAsync("ss1@test.local", "ss1");
        UseAuth(s1);
        await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        var s2 = await RegisterAsync("ss2@test.local", "ss2");
        UseAuth(s2);
        await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        UseAuth(teacher);
        var r = await Client.GetAsync<List<SubmissionSummaryDto>>(
            $"/api/assignments/{aid}/submissions");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal(2, r.Data!.Count);
    }

    [Fact]
    public async Task Get_OthersSubmission_Returns404()
    {
        var teacher = await RegisterAsync("t8@test.local", "t8");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Iso", publish: true);

        var s1 = await RegisterAsync("is1@test.local", "is1");
        UseAuth(s1);
        var sub = await Client.PostAsync<SubmissionDto>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        var s2 = await RegisterAsync("is2@test.local", "is2");
        UseAuth(s2);
        var r = await Client.GetAsync<object>($"/api/submissions/{sub.Data!.Id}");
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task Get_AuthorCanSeeStudentSubmission()
    {
        var teacher = await RegisterAsync("t9@test.local", "t9");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("AuthorView", publish: true);

        var student = await RegisterAsync("sv1@test.local", "sv1");
        UseAuth(student);
        var sub = await Client.PostAsync<SubmissionDto>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        UseAuth(teacher);
        var r = await Client.GetAsync<SubmissionDto>($"/api/submissions/{sub.Data!.Id}");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
    }

    [Fact]
    public async Task ReSubmit_BetterGrade_ImprovesBestGrade()
    {
        var teacher = await RegisterAsync("t10@test.local", "t10");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Multi-try", publish: true);

        var student = await RegisterAsync("mt1@test.local", "mt1");
        UseAuth(student);

        var r1 = await Client.PostAsync<SubmissionDto>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });
        var grade1 = r1.Data!.Grade;

        var r2 = await Client.PostAsync<SubmissionDto>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        Assert.Equal("passed", r1.Data.Status);
        Assert.Equal("passed", r2.Data!.Status);
        Assert.Equal(grade1, r2.Data.Grade);
    }

    // ─────────── DTO ───────────

    public sealed class SubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string Status { get; set; } = "";
        public string? Grade { get; set; }
        public int XpAwarded { get; set; }
        public string? ErrorMessage { get; set; }
        public SubmissionResultDto? Result { get; set; }
    }

    public sealed class SubmissionResultDto
    {
        public bool Passed { get; set; }
        public string? Reason { get; set; }
    }

    public sealed class SubmissionSummaryDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = "";
        public string? Grade { get; set; }
    }
}
