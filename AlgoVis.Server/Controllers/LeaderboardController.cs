using AlgoVis.Server.Leaderboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoVis.Server.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class LeaderboardController : ControllerBase
{
    private readonly LeaderboardService _service;

    public LeaderboardController(LeaderboardService service) => _service = service;

    /// <summary>Топ игроков по XP. Доступно всем авторизованным.</summary>
    [HttpGet("leaderboard/players")]
    [AllowAnonymous]
    public async Task<IActionResult> TopPlayers([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        return Ok(await _service.TopPlayersAsync(limit, ct));
    }

    /// <summary>Топ преподавателей по рейтингу.</summary>
    [HttpGet("leaderboard/teachers")]
    [AllowAnonymous]
    public async Task<IActionResult> TopTeachers([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        return Ok(await _service.TopTeachersAsync(limit, ct));
    }

    /// <summary>Рейтинг по конкретному заданию.</summary>
    [HttpGet("assignments/{id:int}/leaderboard")]
    public async Task<IActionResult> ForAssignment(int id, CancellationToken ct)
    {
        var result = await _service.ForAssignmentAsync(id, ct);
        if (result is null) return NotFound(new { error = "Задание не найдено", kind = "not_found" });
        return Ok(result);
    }
}
