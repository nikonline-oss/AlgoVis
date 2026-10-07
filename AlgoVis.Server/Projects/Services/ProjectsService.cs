using System.Security.Cryptography;
using AlgoVis.Data;
using AlgoVis.Data.Entities;
using AlgoVis.Server.Projects.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Projects.Services;

public sealed class ProjectsService
{
    private readonly AlgoVisDbContext _db;

    public ProjectsService(AlgoVisDbContext db) => _db = db;

    // ─────────── List ───────────

    public async Task<List<ProjectSummaryDto>> ListAsync(int userId, CancellationToken ct = default)
    {
        return await _db.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => ToSummary(p))
            .ToListAsync(ct);
    }

    // ─────────── Get ───────────

    public async Task<ProjectDetailDto?> GetAsync(int userId, int projectId, CancellationToken ct = default)
    {
        var p = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);
        return p is null ? null : ToDetail(p);
    }

    // ─────────── Create ───────────

    public async Task<(ProjectDetailDto? Result, string? Error)> CreateAsync(
        int userId, CreateProjectRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length > 200)
            return (null, "Название должно быть от 1 до 200 символов");

        var project = new Project
        {
            UserId = userId,
            Name = req.Name.Trim(),
            Description = NullIfEmpty(req.Description),
            PythonCode = req.PythonCode,
            YawaJson = req.YawaJson
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        return (ToDetail(project), null);
    }

    // ─────────── Update ───────────

    public async Task<(ProjectDetailDto? Result, string? Error)> UpdateAsync(
        int userId, int projectId, UpdateProjectRequest req, CancellationToken ct = default)
    {
        var p = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);
        if (p is null) return (null, "Проект не найден");

        if (req.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length > 200)
                return (null, "Название должно быть от 1 до 200 символов");
            p.Name = req.Name.Trim();
        }

        if (req.Description is not null)
            p.Description = NullIfEmpty(req.Description);

        if (req.PythonCode is not null)
            p.PythonCode = req.PythonCode;

        if (req.YawaJson is not null)
            p.YawaJson = req.YawaJson;

        await _db.SaveChangesAsync(ct);
        return (ToDetail(p), null);
    }

    // ─────────── Delete ───────────

    public async Task<bool> DeleteAsync(int userId, int projectId, CancellationToken ct = default)
    {
        var p = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);
        if (p is null) return false;

        _db.Projects.Remove(p);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─────────── Publish / Unpublish ───────────

    public async Task<(ProjectDetailDto? Result, string? Error)> PublishAsync(
        int userId, int projectId, CancellationToken ct = default)
    {
        var p = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);
        if (p is null) return (null, "Проект не найден");

        if (p.IsPublic && !string.IsNullOrEmpty(p.PublicSlug))
            return (ToDetail(p), null);

        p.IsPublic = true;
        p.PublicSlug = await GenerateUniqueSlugAsync(ct);
        await _db.SaveChangesAsync(ct);
        return (ToDetail(p), null);
    }

    public async Task<(ProjectDetailDto? Result, string? Error)> UnpublishAsync(
        int userId, int projectId, CancellationToken ct = default)
    {
        var p = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);
        if (p is null) return (null, "Проект не найден");

        p.IsPublic = false;
        p.PublicSlug = null;
        await _db.SaveChangesAsync(ct);
        return (ToDetail(p), null);
    }

    // ─────────── Public ───────────

    public async Task<PublicProjectDto?> GetPublicAsync(string slug, CancellationToken ct = default)
    {
        var p = await _db.Projects
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.PublicSlug == slug && x.IsPublic, ct);
        if (p is null) return null;

        return new PublicProjectDto(
            p.Name,
            p.Description,
            p.PythonCode,
            p.YawaJson,
            p.User.Username,
            p.UpdatedAt);
    }

    // ─────────── Helpers ───────────

    private static ProjectSummaryDto ToSummary(Project p) => new(
        p.Id, p.Name, p.Description, p.IsPublic, p.PublicSlug,
        p.CreatedAt, p.UpdatedAt,
        p.LastRunSteps, p.LastRunComparisons, p.LastRunSwaps);

    private static ProjectDetailDto ToDetail(Project p) => new(
        p.Id, p.Name, p.Description, p.PythonCode, p.YawaJson,
        p.IsPublic, p.PublicSlug,
        p.CreatedAt, p.UpdatedAt,
        p.LastRunSteps, p.LastRunComparisons, p.LastRunSwaps);

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private async Task<string> GenerateUniqueSlugAsync(CancellationToken ct)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            var slug = GenerateSlug();
            var exists = await _db.Projects.AnyAsync(p => p.PublicSlug == slug, ct);
            if (!exists) return slug;
        }
        throw new InvalidOperationException("Не удалось сгенерировать уникальный slug");
    }

    private static string GenerateSlug()
    {
        // 12 символов base62 ≈ 71 бит энтропии — достаточно для публичных ссылок.
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = RandomNumberGenerator.GetBytes(12);
        var chars = new char[12];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        return new string(chars);
    }
}
