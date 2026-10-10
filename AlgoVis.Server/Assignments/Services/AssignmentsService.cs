using System.Text.Json;
using AlgoVis.Data;
using AlgoVis.Data.Entities;
using AlgoVis.Server.Assignments.Dtos;
using AlgoVis.Yawa.Runtime;
using AlgoVis.Yawa.Trace;
using AlgoVis.Yawa.Yawa.Loader;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Assignments.Services;

public sealed class AssignmentsService
{
    private const int HardMaxSteps = 1_000_000;
    private const double HardMaxSeconds = 30.0;

    private readonly AlgoVisDbContext _db;
    private readonly ILogger<AssignmentsService> _log;

    public AssignmentsService(AlgoVisDbContext db, ILogger<AssignmentsService> log)
    {
        _db = db;
        _log = log;
    }

    // ─────────── Preview ───────────

    /// <summary>
    /// Запускает эталонное решение и возвращает результат + статистику.
    /// Используется автором, чтобы выбрать критерии грейдов.
    /// </summary>
    public PreviewResponse Preview(PreviewRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ReferenceSolution))
            return new PreviewResponse(false, "Пустое эталонное решение", "validation",
                null, null, null, null, null);

        if (req.Language != "python")
            return new PreviewResponse(false,
                $"Язык '{req.Language}' пока не поддерживается (только python)",
                "validation", null, null, null, null, null);

        // Транспиляция
        AlgoVis.Yawa.Yawa.YawaProgram program;
        try
        {
            var transpiler = new AlgoVis.Transpiler.PythonToYawa(req.ReferenceSolution);
            program = transpiler.Transpile();
        }
        catch (AlgoVis.Transpiler.UnsupportedFeatureException ex)
        {
            return new PreviewResponse(false, ex.Message, "transpiler",
                ex.Line, ex.Column, null, null, null);
        }
        catch (Exception ex)
        {
            return new PreviewResponse(false, ex.Message, "transpiler",
                null, null, null, null, null);
        }

        // Исполнение
        var opts = new InterpreterOptions
        {
            MaxSteps   = Math.Min(req.MaxSteps ?? program.Limits.MaxSteps, HardMaxSteps),
            MaxDepth   = program.Limits.MaxDepth,
            MaxSeconds = Math.Min(req.MaxSeconds ?? program.Limits.MaxSeconds, HardMaxSeconds),
            SnapshotEvery = program.Limits.SnapshotEvery
        };

        TraceSession session;
        try
        {
            session = new Interpreter(program, opts).Run();
        }
        catch (Exception ex)
        {
            return new PreviewResponse(false, ex.Message, "runtime",
                null, null, null, null, null);
        }

        // Если в trace есть ошибка
        var errorStep = session.Steps.FirstOrDefault(s => s.Kind == "error");
        if (errorStep is not null)
        {
            return new PreviewResponse(false,
                errorStep.Annotation ?? "Ошибка исполнения", "runtime",
                null, null, null, null, session.FinalState);
        }

        // Достаём целевое значение
        var target = string.IsNullOrWhiteSpace(req.CompareTarget)
            ? "__return__"
            : req.CompareTarget;

        object? actual = null;
        if (session.FinalState.TryGetValue(target, out var v))
            actual = v;

        return new PreviewResponse(
            true, null, null, null, null,
            actual,
            new PreviewStats(
                session.Statistics.TotalSteps,
                session.Statistics.Comparisons,
                session.Statistics.Swaps,
                session.Statistics.MemoryAccesses,
                session.Statistics.UserCounters),
            session.FinalState);
    }

    // ─────────── List ───────────

    public async Task<List<AssignmentSummaryDto>> ListMineAsync(int userId, CancellationToken ct = default)
    {
        return await _db.Assignments
            .Where(a => a.AuthorId == userId)
            .OrderByDescending(a => a.UpdatedAt)
            .Select(a => new AssignmentSummaryDto(
                a.Id, a.Title, a.Mode, a.IsPublished, a.IsPublic,
                a.CreatedAt, a.UpdatedAt,
                a.Submissions.Count,
                a.Submissions.Count(s => s.Status == "passed"),
                a.Author.Username))
            .ToListAsync(ct);
    }

    public async Task<List<AssignmentForStudentDto>> ListPublicForStudentAsync(
        int userId, CancellationToken ct = default)
    {
        var assignments = await _db.Assignments
            .Where(a => a.IsPublished && a.IsPublic)
            .OrderByDescending(a => a.CreatedAt)
            .Include(a => a.Author)
            .Include(a => a.Submissions.Where(s => s.UserId == userId))
            .ToListAsync(ct);

        var result = new List<AssignmentForStudentDto>();
        foreach (var a in assignments)
        {
            var my = a.Submissions.FirstOrDefault();
            var passedCount = await _db.Submissions
                .CountAsync(s => s.AssignmentId == a.Id && s.Status == "passed", ct);
            var totalCount = await _db.Submissions
                .CountAsync(s => s.AssignmentId == a.Id, ct);

            result.Add(new AssignmentForStudentDto(
                a.Id,
                a.Author.Username,
                a.Title,
                a.Description,
                a.Mode,
                a.AllowedLanguages,
                a.TemplateCode,
                a.CompareTarget,
                a.MaxSteps,
                a.MaxSeconds,
                a.Deadline,
                a.CreatedAt,
                totalCount,
                passedCount,
                my?.Status == "passed",
                my?.Grade));
        }
        return result;
    }

    // ─────────── Get ───────────

    public async Task<AssignmentDetailDto?> GetMineAsync(
        int userId, int id, CancellationToken ct = default)
    {
        var a = await _db.Assignments
            .Include(x => x.Author)
            .Include(x => x.Submissions)
            .FirstOrDefaultAsync(x => x.Id == id && x.AuthorId == userId, ct);
        return a is null ? null : ToDetail(a);
    }

    public async Task<AssignmentForStudentDto?> GetForStudentAsync(
        int userId, int id, CancellationToken ct = default)
    {
        var a = await _db.Assignments
            .Include(x => x.Author)
            .Include(x => x.Submissions.Where(s => s.UserId == userId))
            .FirstOrDefaultAsync(x => x.Id == id && x.IsPublished, ct);
        if (a is null) return null;

        var passedCount = await _db.Submissions
            .CountAsync(s => s.AssignmentId == a.Id && s.Status == "passed", ct);
        var totalCount = await _db.Submissions
            .CountAsync(s => s.AssignmentId == a.Id, ct);
        var my = a.Submissions.FirstOrDefault();

        return new AssignmentForStudentDto(
            a.Id, a.Author.Username, a.Title, a.Description, a.Mode,
            a.AllowedLanguages, a.TemplateCode, a.CompareTarget,
            a.MaxSteps, a.MaxSeconds, a.Deadline, a.CreatedAt,
            totalCount, passedCount,
            my?.Status == "passed", my?.Grade);
    }

    // ─────────── Create ───────────

    public async Task<(AssignmentDetailDto? Result, string? Error)> CreateAsync(
        int authorId, CreateAssignmentRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || req.Title.Length > 200)
            return (null, "Название должно быть от 1 до 200 символов");

        if (req.Mode is not ("visual" or "code"))
            return (null, "Mode должен быть 'visual' или 'code'");

        if (req.Mode == "code" &&
            (req.AllowedLanguages is null || req.AllowedLanguages.Count == 0))
            return (null, "Для mode=code нужен непустой AllowedLanguages");

        if (string.IsNullOrWhiteSpace(req.ReferenceSolution))
            return (null, "Нужно эталонное решение");

        if (string.IsNullOrWhiteSpace(req.CompareTarget))
            return (null, "Нужно указать CompareTarget");

        // Проверяем, что эталон вообще запускается
        var preview = Preview(new PreviewRequest(
            req.ReferenceSolution,
            req.ReferenceLanguage ?? "python",
            req.CompareTarget,
            req.MaxSteps,
            req.MaxSeconds));

        if (!preview.Success)
            return (null, $"Эталонное решение не работает: {preview.Error}");

        // Если автор не передал ExpectedResult — берём из preview
        var expectedResult = req.ExpectedResult;
        if (string.IsNullOrEmpty(expectedResult) && preview.ActualResult is not null)
            expectedResult = JsonSerializer.Serialize(preview.ActualResult);

        var assignment = new Assignment
        {
            AuthorId = authorId,
            Title = req.Title.Trim(),
            Description = NullIfEmpty(req.Description),
            Mode = req.Mode,
            AllowedLanguages = req.AllowedLanguages ?? new(),
            ReferenceSolution = req.ReferenceSolution,
            ReferenceLanguage = req.ReferenceLanguage ?? "python",
            CompareTarget = req.CompareTarget,
            ExpectedResult = expectedResult,
            GradingRules = NullIfEmpty(req.GradingRules),
            TemplateCode = NullIfEmpty(req.TemplateCode),
            MaxSteps = Math.Min(req.MaxSteps ?? 100_000, HardMaxSteps),
            MaxSeconds = Math.Min(req.MaxSeconds ?? 5.0, HardMaxSeconds),
            IsPublished = req.IsPublished,
            IsPublic = req.IsPublic,
            Deadline = req.Deadline
        };

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync(ct);

        // Обновляем счётчик созданных заданий у автора
        await BumpCreatedCountAsync(authorId, ct);

        // Перезагружаем с автором
        await _db.Entry(assignment).Reference(x => x.Author).LoadAsync(ct);

        _log.LogInformation("Assignment {Id} '{Title}' created by user {UserId}",
            assignment.Id, assignment.Title, authorId);

        return (ToDetail(assignment), null);
    }

    // ─────────── Update ───────────

    public async Task<(AssignmentDetailDto? Result, string? Error)> UpdateAsync(
        int userId, int id, UpdateAssignmentRequest req, CancellationToken ct = default)
    {
        var a = await _db.Assignments
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == id && x.AuthorId == userId, ct);
        if (a is null) return (null, "Задание не найдено");

        if (req.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(req.Title) || req.Title.Length > 200)
                return (null, "Название должно быть от 1 до 200 символов");
            a.Title = req.Title.Trim();
        }
        if (req.Description is not null) a.Description = NullIfEmpty(req.Description);
        if (req.Mode is not null)
        {
            if (req.Mode is not ("visual" or "code"))
                return (null, "Mode должен быть 'visual' или 'code'");
            a.Mode = req.Mode;
        }
        if (req.AllowedLanguages is not null) a.AllowedLanguages = req.AllowedLanguages;
        if (req.ReferenceSolution is not null) a.ReferenceSolution = req.ReferenceSolution;
        if (req.ReferenceLanguage is not null) a.ReferenceLanguage = req.ReferenceLanguage;
        if (req.CompareTarget is not null) a.CompareTarget = req.CompareTarget;
        if (req.ExpectedResult is not null) a.ExpectedResult = req.ExpectedResult;
        if (req.GradingRules is not null) a.GradingRules = NullIfEmpty(req.GradingRules);
        if (req.TemplateCode is not null) a.TemplateCode = NullIfEmpty(req.TemplateCode);
        if (req.MaxSteps.HasValue) a.MaxSteps = Math.Min(req.MaxSteps.Value, HardMaxSteps);
        if (req.MaxSeconds.HasValue) a.MaxSeconds = Math.Min(req.MaxSeconds.Value, HardMaxSeconds);
        if (req.IsPublished.HasValue) a.IsPublished = req.IsPublished.Value;
        if (req.IsPublic.HasValue) a.IsPublic = req.IsPublic.Value;
        if (req.Deadline.HasValue) a.Deadline = req.Deadline;

        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return (ToDetail(a), null);
    }

    // ─────────── Publish / Unpublish ───────────

    public async Task<(AssignmentDetailDto? Result, string? Error)> PublishAsync(
        int userId, int id, CancellationToken ct = default)
    {
        var a = await _db.Assignments
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == id && x.AuthorId == userId, ct);
        if (a is null) return (null, "Задание не найдено");

        if (string.IsNullOrWhiteSpace(a.ExpectedResult))
            return (null, "Нельзя опубликовать: не задан ожидаемый результат. " +
                          "Откройте preview и сохраните эталон.");

        a.IsPublished = true;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return (ToDetail(a), null);
    }

    public async Task<(AssignmentDetailDto? Result, string? Error)> UnpublishAsync(
        int userId, int id, CancellationToken ct = default)
    {
        var a = await _db.Assignments
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == id && x.AuthorId == userId, ct);
        if (a is null) return (null, "Задание не найдено");

        a.IsPublished = false;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return (ToDetail(a), null);
    }

    // ─────────── Delete ───────────

    public async Task<bool> DeleteAsync(int userId, int id, CancellationToken ct = default)
    {
        var a = await _db.Assignments
            .FirstOrDefaultAsync(x => x.Id == id && x.AuthorId == userId, ct);
        if (a is null) return false;

        _db.Assignments.Remove(a);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─────────── Helpers ───────────

    private async Task BumpCreatedCountAsync(int userId, CancellationToken ct)
    {
        var r = await _db.UserRatings.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (r is null)
        {
            r = new UserRating { UserId = userId };
            _db.UserRatings.Add(r);
        }
        r.CreatedCount++;
        r.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private static AssignmentDetailDto ToDetail(Assignment a) => new(
        a.Id, a.AuthorId, a.Author?.Username ?? "?",
        a.Title, a.Description, a.Mode, a.AllowedLanguages,
        a.ReferenceSolution, a.ReferenceLanguage, a.CompareTarget,
        a.ExpectedResult, a.GradingRules, a.TemplateCode,
        a.MaxSteps, a.MaxSeconds,
        a.IsPublished, a.IsPublic, a.Deadline,
        a.CreatedAt, a.UpdatedAt,
        a.Submissions?.Count ?? 0,
        a.Submissions?.Count(s => s.Status == "passed") ?? 0);

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
