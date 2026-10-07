using System.Security.Claims;
using AlgoVis.Server.Projects.Dtos;
using AlgoVis.Server.Projects.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoVis.Server.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController : ControllerBase
{
    private readonly ProjectsService _projects;

    public ProjectsController(ProjectsService projects) => _projects = projects;

    // ─────────── List ───────────

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var list = await _projects.ListAsync(userId, ct);
        return Ok(list);
    }

    // ─────────── Get ───────────

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var p = await _projects.GetAsync(userId, id, ct);
        if (p is null) return NotFound(new { error = "Проект не найден", kind = "not_found" });
        return Ok(p);
    }

    // ─────────── Create ───────────

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _projects.CreateAsync(userId, req, ct);
        if (result is null) return BadRequest(new { error, kind = "validation" });
        return Ok(result);
    }

    // ─────────── Update ───────────

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _projects.UpdateAsync(userId, id, req, ct);
        if (result is null) return NotFound(new { error, kind = "not_found" });
        return Ok(result);
    }

    // ─────────── Delete ───────────

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var ok = await _projects.DeleteAsync(userId, id, ct);
        if (!ok) return NotFound(new { error = "Проект не найден", kind = "not_found" });
        return Ok(new { ok = true });
    }

    // ─────────── Publish / Unpublish ───────────

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _projects.PublishAsync(userId, id, ct);
        if (result is null) return NotFound(new { error, kind = "not_found" });
        return Ok(result);
    }

    [HttpDelete("{id:int}/publish")]
    public async Task<IActionResult> Unpublish(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var (result, error) = await _projects.UnpublishAsync(userId, id, ct);
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
