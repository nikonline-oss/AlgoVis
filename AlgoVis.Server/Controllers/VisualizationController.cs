using AlgoVis.Server.DTO;
using AlgoVis.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlgoVis.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualizationController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<VisualizationController> _logger;

        public VisualizationController(ISessionService sessionService, ILogger<VisualizationController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<SessionResponse>> GetSession(string sessionId)
        {
            try
            {
                var session = await _sessionService.GetSessionAsync(sessionId);
                return Ok(session);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Session {sessionId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("session/{sessionId}")]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            try
            {
                var deleted = await _sessionService.DeleteSessionAsync(sessionId);
                if (!deleted) return NotFound();

                return Ok(new { message = $"Session {sessionId} deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
