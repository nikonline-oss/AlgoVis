using System.Security.Claims;
using AlgoVis.Server.Comments.Dtos;
using AlgoVis.Server.Comments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoVis.Server.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class CommentsController : ControllerBase
{
    private readonly CommentsService _service;

    public CommentsController(CommentsService service) => _service = service;

    [HttpPost("submissions/{submissionId:int}/comments")]
    public async Task<IActionResult> Create(
        int submissionId, [FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _service.CreateAsync(userId, submissionId, req, ct);
        if (result is null)
        {
            // Различаем «не найдено» и «нет доступа» — для простоты отдаём 404
            return NotFound(new { error, kind = "not_found" });
        }
        return Ok(result);
    }

    [HttpGet("submissions/{submissionId:int}/comments")]
    public async Task<IActionResult> List(int submissionId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _service.ListAsync(userId, submissionId, ct);
        if (result is null) return NotFound(new { error, kind = "not_found" });
        return Ok(result);
    }

    [HttpDelete("comments/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var ok = await _service.DeleteAsync(userId, id, ct);
        if (!ok) return NotFound(new { error = "Комментарий не найден", kind = "not_found" });
        return Ok(new { ok = true });
    }

    private bool TryGetUserId(out int userId)
    {
        var sub = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(sub, out userId);
    }
}
