using System.Text.Json;
using AlgoVis.Data;
using AlgoVis.Server.Leaderboard.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Leaderboard.Services;

public sealed class LeaderboardService
{
    private readonly AlgoVisDbContext _db;

    public LeaderboardService(AlgoVisDbContext db) => _db = db;

    // ─────────── Игроки ───────────

    public async Task<List<LeaderboardEntryDto>> TopPlayersAsync(
        int limit, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 200);

        var rows = await _db.UserRatings
            .Where(r => r.PlayerXp > 0)
            .OrderByDescending(r => r.PlayerXp)
            .ThenBy(r => r.UpdatedAt)
            .Take(limit)
            .Include(r => r.User)
            .Select(r => new
            {
                r.UserId,
                r.User.Username,
                r.PlayerXp,
                r.PlayerLevel,
                r.CompletedCount
            })
            .ToListAsync(ct);

        return rows.Select((r, i) => new LeaderboardEntryDto(
            i + 1, r.UserId, r.Username, r.PlayerXp, r.PlayerLevel, r.CompletedCount
        )).ToList();
    }

    // ─────────── Преподаватели ───────────

    public async Task<List<LeaderboardEntryDto>> TopTeachersAsync(
        int limit, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 200);

        var rows = await _db.UserRatings
            .Where(r => r.TeacherRating > 0)
            .OrderByDescending(r => r.TeacherRating)
            .ThenBy(r => r.UpdatedAt)
            .Take(limit)
            .Include(r => r.User)
            .Select(r => new
            {
                r.UserId,
                r.User.Username,
                r.TeacherRating,
                r.TeacherLevel,
                r.CreatedCount
            })
            .ToListAsync(ct);

        return rows.Select((r, i) => new LeaderboardEntryDto(
            i + 1, r.UserId, r.Username, r.TeacherRating, r.TeacherLevel, r.CreatedCount
        )).ToList();
    }

    // ─────────── По заданию ───────────

    /// <summary>
    /// Топ по заданию. Каждый ученик — один раз с лучшим результатом.
    /// Сортировка: perfect > excellent > good > failed > error,
    /// потом по XP, потом по меньшему числу swaps.
    /// </summary>
    public async Task<List<AssignmentLeaderboardEntryDto>?> ForAssignmentAsync(
        int assignmentId, CancellationToken ct = default)
    {
        var exists = await _db.Assignments.AnyAsync(a => a.Id == assignmentId, ct);
        if (!exists) return null;

        var subs = await _db.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .Include(s => s.User)
            .ToListAsync(ct);

        // Берём лучшую submission каждого ученика
        var bestByUser = subs
            .GroupBy(s => s.UserId)
            .Select(g => g
                .OrderByDescending(s => GradeRank(s.Grade))
                .ThenByDescending(s => s.XpAwarded)
                .ThenBy(s => ExtractStat(s, "swaps"))
                .ThenBy(s => s.CreatedAt)
                .First())
            .OrderByDescending(s => GradeRank(s.Grade))
            .ThenByDescending(s => s.XpAwarded)
            .ThenBy(s => ExtractStat(s, "swaps"))
            .ThenBy(s => s.CreatedAt)
            .ToList();

        return bestByUser.Select((s, i) => new AssignmentLeaderboardEntryDto(
            i + 1,
            s.UserId,
            s.User?.Username ?? "?",
            s.Status,
            s.Grade,
            s.XpAwarded,
            ExtractStat(s, "comparisons"),
            ExtractStat(s, "swaps"),
            ExtractStat(s, "totalSteps"),
            s.CreatedAt
        )).ToList();
    }

    private static int GradeRank(string? grade) => grade switch
    {
        "perfect" => 4,
        "excellent" => 3,
        "good" => 2,
        _ => 1
    };

    private static int ExtractStat(Data.Entities.Submission s, string key)
    {
        if (string.IsNullOrEmpty(s.ResultJson)) return 0;
        try
        {
            using var doc = JsonDocument.Parse(s.ResultJson);
            var root = doc.RootElement;

            // Ищем "stats" или "Stats" — на случай старых записей
            if (!TryGetProp(root, "stats", out var stats)) return 0;
            if (!TryGetProp(stats, key, out var val)) return 0;

            return val.ValueKind == JsonValueKind.Number ? val.GetInt32() : 0;
        }
        catch { return 0; }
    }

    /// <summary>
    /// Пробует сначала camelCase, потом PascalCase. System.Text.Json регистрозависим,
    /// а у нас в базе могут быть данные обоих форматов.
    /// </summary>
    private static bool TryGetProp(JsonElement el, string camelName, out JsonElement result)
    {
        if (el.TryGetProperty(camelName, out result)) return true;
        // PascalCase
        var pascal = char.ToUpperInvariant(camelName[0]) + camelName[1..];
        return el.TryGetProperty(pascal, out result);
    }
}
