using AlgoVis.Data;
using AlgoVis.Data.Entities;
using AlgoVis.Server.Comments.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Comments.Services;

public sealed class CommentsService
{
    private readonly AlgoVisDbContext _db;

    public CommentsService(AlgoVisDbContext db) => _db = db;

    /// <summary>
    /// Комментарии к submission. Писать могут:
    /// — автор submission (ученик)
    /// — автор задания
    /// </summary>
    public async Task<(CommentDto? Result, string? Error)> CreateAsync(
        int userId, int submissionId, CreateCommentRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Text))
            return (null, "Текст комментария пуст");

        if (req.Text.Length > 4000)
            return (null, "Текст длиннее 4000 символов");

        var sub = await _db.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == submissionId, ct);

        if (sub is null)
            return (null, "Отправка не найдена");

        // Проверка доступа
        var isOwner = sub.UserId == userId;
        var isAuthor = sub.Assignment.AuthorId == userId;
        if (!isOwner && !isAuthor)
            return (null, "Нет доступа к этой отправке");

        var author = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (author is null) return (null, "Пользователь не найден");

        var comment = new Comment
        {
            SubmissionId = submissionId,
            AuthorId = userId,
            Text = req.Text.Trim()
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);

        return (ToDto(comment, author), null);
    }

    /// <summary>Список комментариев. Читать могут: автор submission и автор задания.</summary>
    public async Task<(List<CommentDto>? Result, string? Error)> ListAsync(
        int userId, int submissionId, CancellationToken ct = default)
    {
        var sub = await _db.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId, ct);
        if (sub is null) return (null, "Отправка не найдена");

        var isOwner = sub.UserId == userId;
        var isAuthor = sub.Assignment.AuthorId == userId;
        if (!isOwner && !isAuthor)
            return (null, "Нет доступа к этой отправке");

        var comments = await _db.Comments
            .Where(c => c.SubmissionId == submissionId)
            .OrderBy(c => c.CreatedAt)
            .Include(c => c.Author)
            .Select(c => ToDto(c, c.Author))
            .ToListAsync(ct);

        return (comments, null);
    }

    /// <summary>Удалить комментарий. Только автор комментария.</summary>
    public async Task<bool> DeleteAsync(int userId, int commentId, CancellationToken ct = default)
    {
        var c = await _db.Comments
            .FirstOrDefaultAsync(x => x.Id == commentId && x.AuthorId == userId, ct);
        if (c is null) return false;

        _db.Comments.Remove(c);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static CommentDto ToDto(Comment c, User author) => new(
        c.Id,
        c.SubmissionId,
        c.AuthorId,
        author.Username,
        author.Role,
        c.Text,
        c.CreatedAt);
}
