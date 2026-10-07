namespace AlgoVis.Server.Projects.Dtos;

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    string? PythonCode,
    string? YawaJson);

public sealed record UpdateProjectRequest(
    string? Name,
    string? Description,
    string? PythonCode,
    string? YawaJson);

public sealed record ProjectSummaryDto(
    int Id,
    string Name,
    string? Description,
    bool IsPublic,
    string? PublicSlug,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int LastRunSteps,
    int LastRunComparisons,
    int LastRunSwaps);

public sealed record ProjectDetailDto(
    int Id,
    string Name,
    string? Description,
    string? PythonCode,
    string? YawaJson,
    bool IsPublic,
    string? PublicSlug,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int LastRunSteps,
    int LastRunComparisons,
    int LastRunSwaps);

public sealed record PublicProjectDto(
    string Name,
    string? Description,
    string? PythonCode,
    string? YawaJson,
    string OwnerUsername,
    DateTime UpdatedAt);
