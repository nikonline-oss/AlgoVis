using System.Text.Json;
using AlgoVis.Data;
using AlgoVis.Data.Entities;
using AlgoVis.Server.Ratings;
using AlgoVis.Server.Submissions.Dtos;
using AlgoVis.Yawa.Runtime;
using AlgoVis.Yawa.Trace;
using AlgoVis.Yawa.Yawa.Loader;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Submissions.Services;

public sealed class SubmissionsService
{
    private const int HardMaxSteps = 1_000_000;
    private const double HardMaxSeconds = 15.0;

    // XP за каждый грейд
    private const int XpGood = 10;
    private const int XpExcellent = 25;
    private const int XpPerfect = 50;

    // Рейтинг преподавателя за каждое прохождение его задания
    private const int TeacherRatingPerPass = 5;

    private readonly AlgoVisDbContext _db;
    private readonly ILogger<SubmissionsService> _log;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public SubmissionsService(AlgoVisDbContext db, ILogger<SubmissionsService> log)
    {
        _db = db;
        _log = log;
    }

    // ─────────── Submit ───────────

    public async Task<(SubmissionDto? Result, string? Error)> SubmitAsync(
        int userId, int assignmentId, SubmitRequest req, CancellationToken ct = default)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.IsPublished, ct);
        if (assignment is null)
            return (null, "Задание не найдено или не опубликовано");

        if (assignment.AuthorId == userId)
            return (null, "Автор не может отправлять решение на своё задание");

        if (assignment.Deadline.HasValue && assignment.Deadline.Value < DateTime.UtcNow)
            return (null, "Срок сдачи истёк");

        if (string.IsNullOrWhiteSpace(req.Code))
            return (null, "Пустое решение");

        if (assignment.Mode == "code" &&
            !assignment.AllowedLanguages.Contains(req.Language, StringComparer.OrdinalIgnoreCase))
            return (null, $"Язык '{req.Language}' не разрешён. Разрешены: " +
                          string.Join(", ", assignment.AllowedLanguages));

        if (req.Language != "python")
            return (null, $"Язык '{req.Language}' пока не поддерживается транспайлером");

        // Создаём submission (pending)
        var submission = new Submission
        {
            AssignmentId = assignmentId,
            UserId = userId,
            Language = req.Language,
            Code = req.Code,
            Status = "pending"
        };
        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync(ct);

        // Запускаем проверку
        var (resultDto, errorMsg) = CheckAssignment(assignment, req.Code);

        if (errorMsg is not null)
        {
            submission.Status = "error";
            submission.ErrorMessage = errorMsg;
            await _db.SaveChangesAsync(ct);

            await _db.Entry(submission).Reference(s => s.Assignment).LoadAsync(ct);
            await _db.Entry(submission).Reference(s => s.User).LoadAsync(ct);

            return (ToDto(submission, null), null);
        }

        submission.ResultJson = JsonSerializer.Serialize(resultDto, JsonOpts);

        if (resultDto!.Passed)
        {
            submission.Status = "passed";
            submission.Grade = PickGrade(assignment, resultDto);
            submission.XpAwarded = XpForGrade(submission.Grade);

            // Начисляем XP ученику и рейтинг автору
            await AwardPlayerXpAsync(userId, submission.XpAwarded, ct);
            await AwardTeacherRatingAsync(assignment.AuthorId, TeacherRatingPerPass, ct);
        }
        else
        {
            submission.Status = "failed";
            submission.Grade = null;
            submission.XpAwarded = 0;
        }

        await _db.SaveChangesAsync(ct);

        _log.LogInformation(
            "Submission {Id}: user {UserId} → assignment {AsgId}, status={Status}, grade={Grade}",
            submission.Id, userId, assignmentId, submission.Status, submission.Grade);

        await _db.Entry(submission).Reference(s => s.Assignment).LoadAsync(ct);
        await _db.Entry(submission).Reference(s => s.User).LoadAsync(ct);

        return (ToDto(submission, resultDto), null);
    }

    // ─────────── Проверка ───────────

    private (SubmissionResultDto? Result, string? Error) CheckAssignment(
        Assignment assignment, string code)
    {
        // 1. Транспиляция
        AlgoVis.Yawa.Yawa.YawaProgram program;
        try
        {
            var transpiler = new AlgoVis.Transpiler.PythonToYawa(code);
            program = transpiler.Transpile();
        }
        catch (AlgoVis.Transpiler.UnsupportedFeatureException ex)
        {
            return (null, $"Строка {ex.Line + 1}:{ex.Column} — {ex.Message}");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }

        // 2. Исполнение
        var opts = new InterpreterOptions
        {
            MaxSteps = Math.Min(assignment.MaxSteps, HardMaxSteps),
            MaxDepth = program.Limits.MaxDepth,
            MaxSeconds = Math.Min(assignment.MaxSeconds, HardMaxSeconds),
            SnapshotEvery = 0
        };

        TraceSession session;
        try
        {
            session = new Interpreter(program, opts).Run();
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }

        var errorStep = session.Steps.FirstOrDefault(s => s.Kind == "error");
        if (errorStep is not null)
            return (null, errorStep.Annotation ?? "Ошибка исполнения");

        // 3. Достаём целевое значение
        var target = assignment.CompareTarget;
        object? actual = null;
        if (session.FinalState.TryGetValue(target, out var actualVal))
            actual = actualVal;

        object? expected = null;
        if (!string.IsNullOrEmpty(assignment.ExpectedResult))
        {
            try { expected = JsonSerializer.Deserialize<object>(assignment.ExpectedResult); }
            catch { /* ignore */ }
        }

        // 4. Сравниваем
        bool resultMatches = JsonEquals(actual, expected);

        // 5. Проверяем критерии грейдов
        var criteria = new Dictionary<string, bool>();
        criteria["result"] = resultMatches;

        if (resultMatches && !string.IsNullOrEmpty(assignment.GradingRules))
        {
            try
            {
                var rules = JsonDocument.Parse(assignment.GradingRules).RootElement;

                // good
                if (rules.TryGetProperty("good", out _))
                    criteria["good"] = true;

                // excellent
                if (rules.TryGetProperty("excellent", out var exc))
                {
                    var ok = CheckCriteria(exc, session.Statistics);
                    criteria["excellent"] = ok;
                }

                // perfect
                if (rules.TryGetProperty("perfect", out var perf))
                {
                    var ok = CheckCriteria(perf, session.Statistics);
                    criteria["perfect"] = ok;
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to parse GradingRules for assignment {Id}",
                    assignment.Id);
            }
        }

        var statsDto = new SubmissionStatsDto(
            session.Statistics.TotalSteps,
            session.Statistics.Comparisons,
            session.Statistics.Swaps,
            session.Statistics.MemoryAccesses);

        var reason = resultMatches
            ? null
            : $"Ожидалось {Fmt(expected)}, получено {Fmt(actual)}";

        return (new SubmissionResultDto(
            resultMatches,
            actual,
            expected,
            statsDto,
            criteria,
            reason), null);
    }

    private static bool CheckCriteria(JsonElement criteria, AlgoVis.Yawa.Trace.TraceStatistics stats)
    {
        if (criteria.TryGetProperty("maxComparisons", out var mc) &&
            stats.Comparisons > mc.GetInt32()) return false;

        if (criteria.TryGetProperty("maxSwaps", out var ms) &&
            stats.Swaps > ms.GetInt32()) return false;

        if (criteria.TryGetProperty("maxSteps", out var mst) &&
            stats.TotalSteps > mst.GetInt32()) return false;

        if (criteria.TryGetProperty("maxMemory", out var mm) &&
            stats.MemoryAccesses > mm.GetInt32()) return false;

        return true;
    }

    private static string PickGrade(Assignment a, SubmissionResultDto result)
    {
        if (result.CriteriaChecked.TryGetValue("perfect", out var p) && p) return "perfect";
        if (result.CriteriaChecked.TryGetValue("excellent", out var e) && e) return "excellent";
        return "good";
    }

    private static int XpForGrade(string? grade) => grade switch
    {
        "perfect" => XpPerfect,
        "excellent" => XpExcellent,
        "good" => XpGood,
        _ => 0
    };

    // ─────────── Rating updates ───────────

    private async Task AwardPlayerXpAsync(int userId, int xp, CancellationToken ct)
    {
        var r = await _db.UserRatings.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (r is null)
        {
            r = new UserRating { UserId = userId };
            _db.UserRatings.Add(r);
        }
        r.PlayerXp += xp;
        r.PlayerLevel = LevelCalculator.LevelFromXp(r.PlayerXp);
        r.CompletedCount++;
        r.UpdatedAt = DateTime.UtcNow;
    }

    private async Task AwardTeacherRatingAsync(int authorId, int delta, CancellationToken ct)
    {
        var r = await _db.UserRatings.FirstOrDefaultAsync(x => x.UserId == authorId, ct);
        if (r is null)
        {
            r = new UserRating { UserId = authorId };
            _db.UserRatings.Add(r);
        }
        r.TeacherRating += delta;
        r.TeacherLevel = LevelCalculator.LevelFromXp(r.TeacherRating);
        r.TotalAssignmentsCompleted++;
        r.UpdatedAt = DateTime.UtcNow;
    }

    // ─────────── List / Get ───────────

    public async Task<List<SubmissionSummaryDto>> ListMineAsync(
        int userId, CancellationToken ct = default)
    {
        return await _db.Submissions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubmissionSummaryDto(
                s.Id, s.UserId, s.User.Username, s.Language,
                s.Status, s.Grade, s.XpAwarded, s.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<List<SubmissionSummaryDto>> ListForAssignmentAsync(
        int authorId, int assignmentId, CancellationToken ct = default)
    {
        // Проверяем, что это задание автора
        var owns = await _db.Assignments
            .AnyAsync(a => a.Id == assignmentId && a.AuthorId == authorId, ct);
        if (!owns) return new();

        return await _db.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubmissionSummaryDto(
                s.Id, s.UserId, s.User.Username, s.Language,
                s.Status, s.Grade, s.XpAwarded, s.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<SubmissionDto?> GetAsync(
        int userId, int submissionId, CancellationToken ct = default)
    {
        var s = await _db.Submissions
            .Include(x => x.Assignment)
            .Include(x => x.User)
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == submissionId, ct);
        if (s is null) return null;

        // Доступ: сам ученик, автор задания, или публичная отправка
        if (s.UserId != userId && s.Assignment.AuthorId != userId)
            return null;

        SubmissionResultDto? result = null;
        if (!string.IsNullOrEmpty(s.ResultJson))
        {
            try { result = JsonSerializer.Deserialize<SubmissionResultDto>(s.ResultJson, JsonOpts); }
            catch { /* ignore */ }
        }

        return ToDto(s, result);
    }

    // ─────────── Helpers ───────────

    private static SubmissionDto ToDto(Submission s, SubmissionResultDto? result) => new(
        s.Id, s.AssignmentId, s.Assignment?.Title ?? "",
        s.UserId, s.User?.Username ?? "?",
        s.Language, s.Code,
        s.Status, s.Grade, s.XpAwarded,
        result, s.ErrorMessage, s.CreatedAt,
        s.Comments?.Count ?? 0);

    private static bool JsonEquals(object? a, object? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        var ja = JsonSerializer.Serialize(a);
        var jb = JsonSerializer.Serialize(b);
        return ja == jb;
    }

    private static string Fmt(object? v) =>
        v is null ? "null" : JsonSerializer.Serialize(v);
}
