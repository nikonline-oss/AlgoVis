using Xunit;
using AlgoVis.Server.Tests.Infrastructure;

namespace AlgoVis.Server.Tests;

public sealed class LeaderboardTests : TestBase
{
    public LeaderboardTests(TestAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Players_EmptyWhenNoSubmissions()
    {
        var r = await Client.GetAsync<List<EntryDto>>("/api/leaderboard/players?limit=10");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        // Никто ещё не сдал — пусто
    }

    [Fact]
    public async Task Players_IncludesStudentAfterPass()
    {
        var teacher = await RegisterAsync("tl1@test.local", "tl1");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("L1", publish: true);

        var student = await RegisterAsync("sl1@test.local", "sl1");
        UseAuth(student);
        await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        var r = await Client.GetAsync<List<EntryDto>>("/api/leaderboard/players?limit=50");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Contains(r.Data!, e => e.Username == student.Username);
    }

    [Fact]
    public async Task Teachers_IncludesTeacherAfterPass()
    {
        var teacher = await RegisterAsync("tl2@test.local", "tl2");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("L2", publish: true);

        var student = await RegisterAsync("sl2@test.local", "sl2");
        UseAuth(student);
        await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        // Leaderboard публичный — можно без токена
        ClearAuth();
        var r = await Client.GetAsync<List<EntryDto>>("/api/leaderboard/teachers?limit=50");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Contains(r.Data!, e => e.Username == teacher.Username);
    }

    [Fact]
    public async Task ForAssignment_RequiresAuth()
    {
        ClearAuth();
        var r = await Client.GetAsync<object>("/api/assignments/1/leaderboard");
        Assert.Equal(401, r.Status);
    }

    [Fact]
    public async Task ForAssignment_ReturnsRanking()
    {
        var teacher = await RegisterAsync("tl3@test.local", "tl3");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("Ranking", publish: true);

        var s1 = await RegisterAsync("ra1@test.local", "ra1");
        UseAuth(s1);
        await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        var s2 = await RegisterAsync("ra2@test.local", "ra2");
        UseAuth(s2);
        await Client.PostAsync<object>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        var r = await Client.GetAsync<List<AssignmentEntryDto>>(
            $"/api/assignments/{aid}/leaderboard");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal(2, r.Data!.Count);
        Assert.All(r.Data, e => Assert.True(e.Rank > 0));
    }

    [Fact]
    public async Task ForAssignment_UnknownId_Returns404()
    {
        var auth = await RegisterAsync("l404@test.local", "l404");
        UseAuth(auth);
        var r = await Client.GetAsync<object>("/api/assignments/999999/leaderboard");
        Assert.Equal(404, r.Status);
    }

    public sealed class EntryDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public long Score { get; set; }
        public int Level { get; set; }
    }

    public sealed class AssignmentEntryDto
    {
        public int Rank { get; set; }
        public string Username { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Grade { get; set; }
        public int XpAwarded { get; set; }
    }
}
