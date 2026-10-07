using System.Security.Claims;
using AlgoVis.Server.Admin.Dtos;
using AlgoVis.Server.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoVis.Server.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public sealed class AdminController : ControllerBase
{
    private readonly AdminService _admin;

    public AdminController(AdminService admin) => _admin = admin;

    // ─────────── Stats ───────────

    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken ct)
        => Ok(await _admin.GetStatsAsync(ct));

    // ─────────── Users ───────────

    [HttpGet("users")]
    public async Task<IActionResult> ListUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        CancellationToken ct = default)
        => Ok(await _admin.ListUsersAsync(page, pageSize, search, role, ct));

    [HttpPatch("users/{id:int}/role")]
    public async Task<IActionResult> ChangeRole(
        int id, [FromBody] ChangeRoleRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var currentAdminId)) return Unauthorized();
        var (result, error) = await _admin.ChangeRoleAsync(currentAdminId, id, req.Role, ct);
        if (result is null) return BadRequest(new { error, kind = "validation" });
        return Ok(result);
    }

    [HttpPatch("users/{id:int}/active")]
    public async Task<IActionResult> ChangeActive(
        int id, [FromBody] ChangeActiveRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var currentAdminId)) return Unauthorized();
        var (result, error) = await _admin.ChangeActiveAsync(currentAdminId, id, req.IsActive, ct);
        if (result is null) return BadRequest(new { error, kind = "validation" });
        return Ok(result);
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var currentAdminId)) return Unauthorized();
        var (ok, error) = await _admin.DeleteUserAsync(currentAdminId, id, ct);
        if (!ok) return BadRequest(new { error, kind = "validation" });
        return Ok(new { ok = true });
    }

    // ─────────── Projects ───────────

    [HttpGet("projects")]
    public async Task<IActionResult> ListProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await _admin.ListProjectsAsync(page, pageSize, search, ct));

    [HttpDelete("projects/{id:int}")]
    public async Task<IActionResult> DeleteProject(int id, CancellationToken ct)
    {
        var ok = await _admin.DeleteProjectAsync(id, ct);
        if (!ok) return NotFound(new { error = "Проект не найден", kind = "not_found" });
        return Ok(new { ok = true });
    }

    // ─────────── Assignments ───────────

    [HttpGet("assignments")]
    public async Task<IActionResult> ListAssignments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await _admin.ListAssignmentsAsync(page, pageSize, search, ct));

    [HttpDelete("assignments/{id:int}")]
    public async Task<IActionResult> DeleteAssignment(int id, CancellationToken ct)
    {
        var ok = await _admin.DeleteAssignmentAsync(id, ct);
        if (!ok) return NotFound(new { error = "Задание не найдено", kind = "not_found" });
        return Ok(new { ok = true });
    }

    // ─────────── Submissions ───────────

    [HttpGet("submissions")]
    public async Task<IActionResult> ListSubmissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
        => Ok(await _admin.ListSubmissionsAsync(page, pageSize, status, ct));

    // ─────────── Helpers ───────────

    private bool TryGetUserId(out int userId)
    {
        var sub = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(sub, out userId);
    }
}
