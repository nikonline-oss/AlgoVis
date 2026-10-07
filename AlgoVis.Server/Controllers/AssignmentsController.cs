using System.Security.Claims;
using AlgoVis.Server.Assignments.Dtos;
using AlgoVis.Server.Assignments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoVis.Server.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public sealed class AssignmentsController : ControllerBase
{
    private readonly AssignmentsService _service;

    public AssignmentsController(AssignmentsService service) => _service = service;

    // ─────────── Preview эталона ───────────

    /// <summary>Прогнать эталонное решение без сохранения. Возвращает результат и статистику.</summary>
    [HttpPost("preview")]
    [EnableRateLimiting("run")]
    public IActionResult Preview([FromBody] PreviewRequest req)
        => Ok(_service.Preview(req));

    // ─────────── Список ───────────

    /// <summary>Мои задания (созданные мной).</summary>
    [HttpGet("mine")]
    public async Task<IActionResult> ListMine(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await _service.ListMineAsync(userId, ct));
    }

    /// <summary>Публичные задания (для ученика).</summary>
    [HttpGet]
    public async Task<IActionResult> ListPublic(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await _service.ListPublicForStudentAsync(userId, ct));
    }

    // ─────────── Детали ───────────

    /// <summary>Моё задание с эталоном (только автор).</summary>
    [HttpGet("{id:int}/mine")]
    public async Task<IActionResult> GetMine(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var a = await _service.GetMineAsync(userId, id, ct);
        if (a is null) return NotFound(new { error = "Задание не найдено", kind = "not_found" });
        return Ok(a);
    }

    /// <summary>Задание для ученика (без эталона).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetForStudent(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var a = await _service.GetForStudentAsync(userId, id, ct);
        if (a is null) return NotFound(new { error = "Задание не найдено", kind = "not_found" });
        return Ok(a);
    }

    // ─────────── CRUD ───────────

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _service.CreateAsync(userId, req, ct);
        if (result is null) return BadRequest(new { error, kind = "validation" });
        return Ok(result);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAssignmentRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _service.UpdateAsync(userId, id, req, ct);
        if (result is null) return NotFound(new { error, kind = "not_found" });
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var ok = await _service.DeleteAsync(userId, id, ct);
        if (!ok) return NotFound(new { error = "Задание не найдено", kind = "not_found" });
        return Ok(new { ok = true });
    }

    // ─────────── Публикация ───────────

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _service.PublishAsync(userId, id, ct);
        if (result is null) return BadRequest(new { error, kind = "validation" });
        return Ok(result);
    }

    [HttpDelete("{id:int}/publish")]
    public async Task<IActionResult> Unpublish(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _service.UnpublishAsync(userId, id, ct);
        if (result is null) return NotFound(new { error, kind = "not_found" });
        return Ok(result);
    }

    // ─────────── Helpers ───────────

    private bool TryGetUserId(out int userId)
    {
        var sub = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(sub, out userId);
    }
}
