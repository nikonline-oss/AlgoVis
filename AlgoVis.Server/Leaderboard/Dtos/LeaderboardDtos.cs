namespace AlgoVis.Server.Leaderboard.Dtos;

public sealed record LeaderboardEntryDto(
    int Rank,
    int UserId,
    string Username,
    long Score,
    int Level,
    int CompletedOrCreatedCount);

public sealed record AssignmentLeaderboardEntryDto(
    int Rank,
    int UserId,
    string Username,
    string Status,
    string? Grade,
    int XpAwarded,
    int Comparisons,
    int Swaps,
    int TotalSteps,
    DateTime SubmittedAt);
