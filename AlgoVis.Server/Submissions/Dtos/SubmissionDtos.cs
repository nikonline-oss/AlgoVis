namespace AlgoVis.Server.Submissions.Dtos;

public sealed record SubmitRequest(string Language, string Code);

public sealed record SubmissionDto(
    int Id,
    int AssignmentId,
    string AssignmentTitle,
    int UserId,
    string Username,
    string Language,
    string Code,
    string Status,
    string? Grade,
    int XpAwarded,
    SubmissionResultDto? Result,
    string? ErrorMessage,
    DateTime CreatedAt,
    int CommentsCount);

public sealed record SubmissionResultDto(
    bool Passed,
    object? ActualResult,
    object? ExpectedResult,
    SubmissionStatsDto? Stats,
    Dictionary<string, bool> CriteriaChecked,
    string? Reason);

public sealed record SubmissionStatsDto(
    int TotalSteps,
    int Comparisons,
    int Swaps,
    int MemoryAccesses);

/// <summary>Краткая версия для списка.</summary>
public sealed record SubmissionSummaryDto(
    int Id,
    int UserId,
    string Username,
    string Language,
    string Status,
    string? Grade,
    int XpAwarded,
    DateTime CreatedAt);

/// <summary>Публичный профиль пользователя с рейтингами.</summary>
public sealed record UserProfileDto(
    int Id,
    string Username,
    string Role,
    DateTime CreatedAt,
    UserRatingDto Rating,
    int PublicProjectsCount,
    int PublishedAssignmentsCount);

public sealed record UserRatingDto(
    long PlayerXp,
    int PlayerLevel,
    int CompletedCount,
    long TeacherRating,
    int TeacherLevel,
    int CreatedCount,
    int TotalAssignmentsCompleted);
