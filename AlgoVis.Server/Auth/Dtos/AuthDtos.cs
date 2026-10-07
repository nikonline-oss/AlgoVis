namespace AlgoVis.Server.Auth.Dtos;

public sealed record RegisterRequest(string Email, string Username, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserInfoDto User);

public sealed record UserInfoDto(
    int Id,
    string Email,
    string Username,
    string Role,
    string DefaultLanguage,
    DateTime CreatedAt,
    DateTime? LastLoginAt);
