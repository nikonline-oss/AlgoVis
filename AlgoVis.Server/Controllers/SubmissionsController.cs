using System.Security.Claims;
using AlgoVis.Server.Submissions.Dtos;
using AlgoVis.Server.Submissions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoVis.Server.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class SubmissionsController : ControllerBase
{
    private readonly SubmissionsService _service;

    public SubmissionsController(SubmissionsService service) => _service = service;

    /// <summary>Отправить решение на задание.</summary>
    [HttpPost("assignments/{assignmentId:int}/submit")]
    [EnableRateLimiting("run")]
    public async Task<IActionResult> Submit(
        int assignmentId, [FromBody] SubmitRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _service.SubmitAsync(userId, assignmentId, req, ct);
        if (result is null) return BadRequest(new { error, kind = "validation" });
        return Ok(result);
    }

    /// <summary>Мои отправки.</summary>
    [HttpGet("submissions/mine")]
    public async Task<IActionResult> ListMine(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await _service.ListMineAsync(userId, ct));
    }

    /// <summary>Отправки на моё задание (только автор).</summary>
    [HttpGet("assignments/{assignmentId:int}/submissions")]
    public async Task<IActionResult> ListForAssignment(int assignmentId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await _service.ListForAssignmentAsync(userId, assignmentId, ct));
    }

    /// <summary>Конкретная отправка (свой или автор задания).</summary>
    [HttpGet("submissions/{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var s = await _service.GetAsync(userId, id, ct);
        if (s is null) return NotFound(new { error = "Отправка не найдена", kind = "not_found" });
        return Ok(s);
    }

    private bool TryGetUserId(out int userId)
    {
        var sub = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(sub, out userId);
    }
}
