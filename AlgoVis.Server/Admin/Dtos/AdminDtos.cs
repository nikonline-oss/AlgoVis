namespace AlgoVis.Server.Admin.Dtos;

public sealed record AdminStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int AdminCount,
    int TeacherCount,
    int StudentCount,
    int TotalProjects,
    int PublicProjects,
    int TotalAssignments,
    int PublishedAssignments,
    int TotalSubmissions,
    int PassedSubmissions,
    int TotalComments,
    int ActiveRefreshTokens,
    DateTime ServerTime);

public sealed record AdminUserDto(
    int Id,
    string Email,
    string Username,
    string Role,
    string DefaultLanguage,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    int ProjectsCount,
    int AssignmentsCount,
    int SubmissionsCount,
    long PlayerXp,
    long TeacherRating);

public sealed record AdminUserListDto(
    int Page,
    int PageSize,
    int TotalCount,
    List<AdminUserDto> Users);

public sealed record ChangeRoleRequest(string Role);
public sealed record ChangeActiveRequest(bool IsActive);

public sealed record AdminProjectDto(
    int Id,
    int UserId,
    string OwnerUsername,
    string Name,
    string? Description,
    bool IsPublic,
    string? PublicSlug,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record AdminProjectListDto(
    int Page, int PageSize, int TotalCount, List<AdminProjectDto> Projects);

public sealed record AdminAssignmentDto(
    int Id,
    int AuthorId,
    string AuthorUsername,
    string Title,
    string Mode,
    bool IsPublished,
    bool IsPublic,
    DateTime CreatedAt,
    int SubmissionsCount,
    int PassedCount);

public sealed record AdminAssignmentListDto(
    int Page, int PageSize, int TotalCount, List<AdminAssignmentDto> Assignments);

public sealed record AdminSubmissionDto(
    int Id,
    int AssignmentId,
    string AssignmentTitle,
    int UserId,
    string Username,
    string Language,
    string Status,
    string? Grade,
    int XpAwarded,
    DateTime CreatedAt);

public sealed record AdminSubmissionListDto(
    int Page, int PageSize, int TotalCount, List<AdminSubmissionDto> Submissions);
