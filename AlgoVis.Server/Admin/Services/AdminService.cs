using AlgoVis.Data;
using AlgoVis.Server.Admin.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Admin.Services;

public sealed class AdminService
{
    private readonly AlgoVisDbContext _db;

    public AdminService(AlgoVisDbContext db) => _db = db;

    // ─────────── Stats ───────────

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var totalUsers = await _db.Users.CountAsync(ct);
        var activeUsers = await _db.Users.CountAsync(u => u.IsActive, ct);
        var adminCount = await _db.Users.CountAsync(u => u.Role == "admin", ct);
        var teacherCount = await _db.Users.CountAsync(u => u.Role == "teacher", ct);
        var studentCount = await _db.Users.CountAsync(u => u.Role == "student", ct);

        var totalProjects = await _db.Projects.CountAsync(ct);
        var publicProjects = await _db.Projects.CountAsync(p => p.IsPublic, ct);

        var totalAssignments = await _db.Assignments.CountAsync(ct);
        var publishedAssignments = await _db.Assignments.CountAsync(a => a.IsPublished, ct);

        var totalSubmissions = await _db.Submissions.CountAsync(ct);
        var passedSubmissions = await _db.Submissions.CountAsync(s => s.Status == "passed", ct);

        var totalComments = await _db.Comments.CountAsync(ct);

        var activeRefreshTokens = await _db.RefreshTokens
            .CountAsync(t => t.RevokedAt == null && t.ExpiresAt > now, ct);

        return new AdminStatsDto(
            totalUsers, activeUsers, adminCount, teacherCount, studentCount,
            totalProjects, publicProjects,
            totalAssignments, publishedAssignments,
            totalSubmissions, passedSubmissions,
            totalComments, activeRefreshTokens,
            now);
    }

    // ─────────── Users ───────────

    public async Task<AdminUserListDto> ListUsersAsync(
        int page, int pageSize, string? search, string? role,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(u => u.Email.ToLower().Contains(s) ||
                             u.Username.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(role))
            q = q.Where(u => u.Role == role);

        var total = await q.CountAsync(ct);

        var users = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id, u.Email, u.Username, u.Role, u.DefaultLanguage,
                u.IsActive, u.CreatedAt, u.LastLoginAt,
                ProjectsCount = u.Projects.Count,
                AssignmentsCount = u.CreatedAssignments.Count,
                SubmissionsCount = u.Submissions.Count,
                PlayerXp = u.Rating != null ? u.Rating.PlayerXp : 0,
                TeacherRating = u.Rating != null ? u.Rating.TeacherRating : 0
            })
            .ToListAsync(ct);

        var list = users.Select(u => new AdminUserDto(
            u.Id, u.Email, u.Username, u.Role, u.DefaultLanguage,
            u.IsActive, u.CreatedAt, u.LastLoginAt,
            u.ProjectsCount, u.AssignmentsCount, u.SubmissionsCount,
            u.PlayerXp, u.TeacherRating)).ToList();

        return new AdminUserListDto(page, pageSize, total, list);
    }

    /// <summary>
    /// Меняем роль. Правила:
    /// — нельзя понизить последнего админа
    /// — допустимые роли: student, teacher, admin
    /// </summary>
    public async Task<(AdminUserDto? Result, string? Error)> ChangeRoleAsync(
        int currentAdminId, int targetUserId, string newRole,
        CancellationToken ct = default)
    {
        var allowed = new[] { "student", "teacher", "admin" };
        if (!allowed.Contains(newRole))
            return (null, $"Недопустимая роль: {newRole}");

        var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct);
        if (target is null) return (null, "Пользователь не найден");

        if (target.Role == newRole) return (await GetUserDtoAsync(targetUserId, ct), null);

        // Если понижаем последнего админа — запрет
        if (target.Role == "admin" && newRole != "admin")
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == "admin", ct);
            if (adminCount <= 1)
                return (null, "Нельзя понизить последнего администратора");
        }

        target.Role = newRole;
        await _db.SaveChangesAsync(ct);

        return (await GetUserDtoAsync(targetUserId, ct), null);
    }

    public async Task<(AdminUserDto? Result, string? Error)> ChangeActiveAsync(
        int currentAdminId, int targetUserId, bool isActive,
        CancellationToken ct = default)
    {
        if (currentAdminId == targetUserId)
            return (null, "Нельзя заблокировать себя");

        var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct);
        if (target is null) return (null, "Пользователь не найден");

        target.IsActive = isActive;

        // Если блокируем — отзываем все refresh-токены
        if (!isActive)
        {
            var activeTokens = await _db.RefreshTokens
                .Where(t => t.UserId == targetUserId && t.RevokedAt == null)
                .ToListAsync(ct);
            foreach (var t in activeTokens) t.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return (await GetUserDtoAsync(targetUserId, ct), null);
    }

    public async Task<(bool Ok, string? Error)> DeleteUserAsync(
        int currentAdminId, int targetUserId, CancellationToken ct = default)
    {
        if (currentAdminId == targetUserId)
            return (false, "Нельзя удалить себя");

        var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct);
        if (target is null) return (false, "Пользователь не найден");

        // Если удаляем последнего админа — запрет
        if (target.Role == "admin")
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == "admin", ct);
            if (adminCount <= 1) return (false, "Нельзя удалить последнего администратора");
        }

        // Проверяем: есть ли у пользователя задания, которые он создал?
        // Их удалять не будем — иначе потеряем историю. Вместо этого блокируем.
        var hasAssignments = await _db.Assignments.AnyAsync(a => a.AuthorId == targetUserId, ct);
        if (hasAssignments)
            return (false, "У пользователя есть созданные задания. " +
                           "Сначала удалите их или заблокируйте пользователя.");

        _db.Users.Remove(target);
        await _db.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task<AdminUserDto?> GetUserDtoAsync(int userId, CancellationToken ct)
    {
        var u = await _db.Users
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.Id, x.Email, x.Username, x.Role, x.DefaultLanguage,
                x.IsActive, x.CreatedAt, x.LastLoginAt,
                ProjectsCount = x.Projects.Count,
                AssignmentsCount = x.CreatedAssignments.Count,
                SubmissionsCount = x.Submissions.Count,
                PlayerXp = x.Rating != null ? x.Rating.PlayerXp : 0,
                TeacherRating = x.Rating != null ? x.Rating.TeacherRating : 0
            })
            .FirstOrDefaultAsync(ct);
        if (u is null) return null;

        return new AdminUserDto(
            u.Id, u.Email, u.Username, u.Role, u.DefaultLanguage,
            u.IsActive, u.CreatedAt, u.LastLoginAt,
            u.ProjectsCount, u.AssignmentsCount, u.SubmissionsCount,
            u.PlayerXp, u.TeacherRating);
    }

    // ─────────── Projects ───────────

    public async Task<AdminProjectListDto> ListProjectsAsync(
        int page, int pageSize, string? search,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Projects.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(p => p.Name.ToLower().Contains(s));
        }

        var total = await q.CountAsync(ct);
        var projects = await q
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminProjectDto(
                p.Id, p.UserId, p.User.Username, p.Name, p.Description,
                p.IsPublic, p.PublicSlug, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return new AdminProjectListDto(page, pageSize, total, projects);
    }

    public async Task<bool> DeleteProjectAsync(int projectId, CancellationToken ct = default)
    {
        var p = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (p is null) return false;
        _db.Projects.Remove(p);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─────────── Assignments ───────────

    public async Task<AdminAssignmentListDto> ListAssignmentsAsync(
        int page, int pageSize, string? search,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Assignments.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(a => a.Title.ToLower().Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(a => a.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AdminAssignmentDto(
                a.Id, a.AuthorId, a.Author.Username, a.Title, a.Mode,
                a.IsPublished, a.IsPublic, a.CreatedAt,
                a.Submissions.Count,
                a.Submissions.Count(s => s.Status == "passed")))
            .ToListAsync(ct);

        return new AdminAssignmentListDto(page, pageSize, total, items);
    }

    public async Task<bool> DeleteAssignmentAsync(int id, CancellationToken ct = default)
    {
        var a = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (a is null) return false;
        _db.Assignments.Remove(a);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─────────── Submissions ───────────

    public async Task<AdminSubmissionListDto> ListSubmissionsAsync(
        int page, int pageSize, string? status,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Submissions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(s => s.Status == status);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new AdminSubmissionDto(
                s.Id, s.AssignmentId, s.Assignment.Title,
                s.UserId, s.User.Username, s.Language,
                s.Status, s.Grade, s.XpAwarded, s.CreatedAt))
            .ToListAsync(ct);

        return new AdminSubmissionListDto(page, pageSize, total, items);
    }
}
