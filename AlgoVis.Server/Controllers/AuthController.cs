using System.Security.Claims;
using AlgoVis.Data;
using AlgoVis.Server.Auth.Dtos;
using AlgoVis.Server.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoVis.Server.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly AlgoVisDbContext _db;

    public AuthController(AuthService auth, AlgoVisDbContext db)
    {
        _auth = auth;
        _db = db;
    }

    [HttpPost("register")]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var (result, error) = await _auth.RegisterAsync(req, UserAgent(), ClientIp(), ct);
        if (result is null) return BadRequest(new { error, kind = "validation" });
        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var (result, error) = await _auth.LoginAsync(req, UserAgent(), ClientIp(), ct);
        if (result is null) return Unauthorized(new { error, kind = "auth" });
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var (result, error) = await _auth.RefreshAsync(req.RefreshToken, UserAgent(), ClientIp(), ct);
        if (result is null) return Unauthorized(new { error, kind = "auth" });
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var ok = await _auth.LogoutAsync(req.RefreshToken, ct);
        return Ok(new { ok });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var sub = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            return Unauthorized();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return Unauthorized();

        return Ok(new UserInfoDto(
            user.Id, user.Email, user.Username, user.Role, user.DefaultLanguage,
            user.CreatedAt, user.LastLoginAt));
    }

    [HttpGet("profile/{username}")]
    [AllowAnonymous]
    public async Task<IActionResult> PublicProfile(string username, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username, ct);
        if (user is null) return NotFound(new { error = "Пользователь не найден" });

        var rating = await _db.UserRatings
            .FirstOrDefaultAsync(r => r.UserId == user.Id, ct);

        var publicProjects = await _db.Projects
            .CountAsync(p => p.UserId == user.Id && p.IsPublic, ct);
        var publishedAssignments = await _db.Assignments
            .CountAsync(a => a.AuthorId == user.Id && a.IsPublished, ct);

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Role,
            user.CreatedAt,
            rating = new
            {
                playerXp = rating?.PlayerXp ?? 0,
                playerLevel = rating?.PlayerLevel ?? 1,
                completedCount = rating?.CompletedCount ?? 0,
                teacherRating = rating?.TeacherRating ?? 0,
                teacherLevel = rating?.TeacherLevel ?? 1,
                createdCount = rating?.CreatedCount ?? 0,
                totalAssignmentsCompleted = rating?.TotalAssignmentsCompleted ?? 0
            },
            publicProjectsCount = publicProjects,
            publishedAssignmentsCount = publishedAssignments
        });
    }

    private string? UserAgent() => Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
