using AlgoVis.Server.Projects.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoVis.Server.Controllers;

[ApiController]
[Route("api/public/projects")]
[AllowAnonymous]
public sealed class PublicProjectsController : ControllerBase
{
    private readonly ProjectsService _projects;

    public PublicProjectsController(ProjectsService projects) => _projects = projects;

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 64)
            return BadRequest(new { error = "Некорректный slug", kind = "validation" });

        var p = await _projects.GetPublicAsync(slug, ct);
        if (p is null) return NotFound(new { error = "Проект не найден", kind = "not_found" });
        return Ok(p);
    }
}
