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

    private string? UserAgent() => Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
