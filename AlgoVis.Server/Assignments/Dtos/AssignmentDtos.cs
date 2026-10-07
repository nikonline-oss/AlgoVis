namespace AlgoVis.Server.Assignments.Dtos;

// ─── Preview эталона (перед созданием задания) ───

public sealed record PreviewRequest(
    string ReferenceSolution,
    string Language,          // "python" (пока только)
    string CompareTarget,     // "A" | "__return__" | ...
    int? MaxSteps,
    double? MaxSeconds);

public sealed record PreviewResponse(
    bool Success,
    string? Error,
    string? Kind,             // "transpiler" | "runtime" | null
    int? Line,
    int? Column,
    object? ActualResult,
    PreviewStats? Stats,
    Dictionary<string, object?>? FinalState);

public sealed record PreviewStats(
    int TotalSteps,
    int Comparisons,
    int Swaps,
    int MemoryAccesses,
    Dictionary<string, long> UserCounters);

// ─── Создание / обновление ───

public sealed record CreateAssignmentRequest(
    string Title,
    string? Description,
    string Mode,               // "visual" | "code"
    List<string>? AllowedLanguages,
    string ReferenceSolution,
    string? ReferenceLanguage,
    string CompareTarget,
    string? ExpectedResult,    // JSON-строка значения
    string? GradingRules,      // JSON
    string? TemplateCode,
    int? MaxSteps,
    double? MaxSeconds,
    bool IsPublished,
    bool IsPublic,
    DateTime? Deadline);

public sealed record UpdateAssignmentRequest(
    string? Title,
    string? Description,
    string? Mode,
    List<string>? AllowedLanguages,
    string? ReferenceSolution,
    string? ReferenceLanguage,
    string? CompareTarget,
    string? ExpectedResult,
    string? GradingRules,
    string? TemplateCode,
    int? MaxSteps,
    double? MaxSeconds,
    bool? IsPublished,
    bool? IsPublic,
    DateTime? Deadline);

// ─── Ответы ───

public sealed record AssignmentSummaryDto(
    int Id,
    string Title,
    string Mode,
    bool IsPublished,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int SubmissionsCount,
    int PassedCount,
    string AuthorUsername);

public sealed record AssignmentDetailDto(
    int Id,
    int AuthorId,
    string AuthorUsername,
    string Title,
    string? Description,
    string Mode,
    List<string> AllowedLanguages,
    string ReferenceSolution,
    string ReferenceLanguage,
    string CompareTarget,
    string? ExpectedResult,
    string? GradingRules,
    string? TemplateCode,
    int MaxSteps,
    double MaxSeconds,
    bool IsPublished,
    bool IsPublic,
    DateTime? Deadline,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int SubmissionsCount,
    int PassedCount);

/// <summary>Версия для ученика — без эталонного решения.</summary>
public sealed record AssignmentForStudentDto(
    int Id,
    string AuthorUsername,
    string Title,
    string? Description,
    string Mode,
    List<string> AllowedLanguages,
    string? TemplateCode,
    string CompareTarget,
    int MaxSteps,
    double MaxSeconds,
    DateTime? Deadline,
    DateTime CreatedAt,
    int SubmissionsCount,
    int PassedCount,
    bool MySubmissionPassed,
    string? MyBestGrade);
