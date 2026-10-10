namespace AlgoVis.Server.Comments.Dtos;

public sealed record CreateCommentRequest(string Text);

public sealed record CommentDto(
    int Id,
    int SubmissionId,
    int AuthorId,
    string AuthorUsername,
    string AuthorRole,
    string Text,
    DateTime CreatedAt);
