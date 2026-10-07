using Xunit;
using AlgoVis.Server.Tests.Infrastructure;

namespace AlgoVis.Server.Tests;

public sealed class CommentsTests : TestBase
{
    public CommentsTests(TestAppFactory factory) : base(factory) { }

    private async Task<(int submissionId, AuthResult teacher, AuthResult student)> SetupAsync()
    {
        var teacher = await RegisterAsync("tc1@test.local", "tc1");
        UseAuth(teacher);
        var aid = await CreateAssignmentAsync("WithComments", publish: true);

        var student = await RegisterAsync("sc1@test.local", "sc1");
        UseAuth(student);
        var sub = await Client.PostAsync<SubmissionDto>($"/api/assignments/{aid}/submit",
            new { language = "python", code = BubbleSortCode });

        return (sub.Data!.Id, teacher, student);
    }

    [Fact]
    public async Task Create_StudentCanComment()
    {
        var (sid, _, student) = await SetupAsync();
        UseAuth(student);

        var r = await Client.PostAsync<CommentDto>($"/api/submissions/{sid}/comments",
            new { text = "Первый!" });
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal("Первый!", r.Data!.Text);
    }

    [Fact]
    public async Task Create_TeacherCanComment()
    {
        var (sid, teacher, _) = await SetupAsync();
        UseAuth(teacher);

        var r = await Client.PostAsync<CommentDto>($"/api/submissions/{sid}/comments",
            new { text = "Молодец" });
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Create_EmptyText_Returns404()
    {
        var (sid, _, student) = await SetupAsync();
        UseAuth(student);
        var r = await Client.PostAsync<object>($"/api/submissions/{sid}/comments",
            new { text = "" });
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task Create_Outsider_Returns404()
    {
        var (sid, _, _) = await SetupAsync();
        var outsider = await RegisterAsync("outsider@test.local", "outsider");
        UseAuth(outsider);

        var r = await Client.PostAsync<object>($"/api/submissions/{sid}/comments",
            new { text = "Не пройдёт" });
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task List_BothParticipantsSeeAllComments()
    {
        var (sid, teacher, student) = await SetupAsync();

        UseAuth(student);
        await Client.PostAsync<CommentDto>($"/api/submissions/{sid}/comments",
            new { text = "от студента" });

        UseAuth(teacher);
        await Client.PostAsync<CommentDto>($"/api/submissions/{sid}/comments",
            new { text = "от учителя" });

        var r = await Client.GetAsync<List<CommentDto>>($"/api/submissions/{sid}/comments");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
        Assert.Equal(2, r.Data!.Count);
    }

    [Fact]
    public async Task List_Outsider_Returns404()
    {
        var (sid, _, _) = await SetupAsync();
        var outsider = await RegisterAsync("o2@test.local", "o2");
        UseAuth(outsider);

        var r = await Client.GetAsync<object>($"/api/submissions/{sid}/comments");
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task Delete_OwnComment_Works()
    {
        var (sid, _, student) = await SetupAsync();
        UseAuth(student);
        var c = await Client.PostAsync<CommentDto>($"/api/submissions/{sid}/comments",
            new { text = "удалить" });

        var r = await Client.DeleteAsync<object>($"/api/comments/{c.Data!.Id}");
        Assert.True(r.IsSuccess);

        var r2 = await Client.DeleteAsync<object>($"/api/comments/{c.Data.Id}");
        Assert.Equal(404, r2.Status);
    }

    [Fact]
    public async Task Delete_OthersComment_Returns404()
    {
        var (sid, teacher, student) = await SetupAsync();
        UseAuth(teacher);
        var c = await Client.PostAsync<CommentDto>($"/api/submissions/{sid}/comments",
            new { text = "учителя" });

        UseAuth(student);
        var r = await Client.DeleteAsync<object>($"/api/comments/{c.Data!.Id}");
        Assert.Equal(404, r.Status);
    }

    public sealed class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
    }

    public sealed class SubmissionDto
    {
        public int Id { get; set; }
    }
}
