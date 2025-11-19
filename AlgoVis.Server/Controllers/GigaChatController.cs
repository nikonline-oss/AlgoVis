using AlgoVis.Core.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlgoVis.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GigaChatController : ControllerBase
    {
        private readonly GigaChatService _service;
        public GigaChatController()
        {
            _service = new GigaChatService();
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteCustomAlgorithm([FromBody] GigaChatTestRequest request)
        {
            if (request == null || request.message == null)
            {
                return BadRequest(new { Success = false, Message = "Request or Algorithm cannot be null" });
            }

            try
            {
                var result = await _service.SendMessageAsync(request.message);

                return Ok(new
                {
                    Success = true,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Error executing custom algorithm: {ex.Message}"
                });
            }
        }
        [HttpPost("code")]
        public async Task<IActionResult> ExecuteCodePromt([FromBody] GigaChatTestRequest request)
        {
            if (request == null || request.message == null)
            {
                return BadRequest(new { Success = false, Message = "Request or Algorithm cannot be null" });
            }

            try
            {
                var promt = _service.GeneratePromt(request.message);
                var result = await _service.SendMessageAsync(promt);

                return Ok(new
                {
                    Success = true,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Error executing custom algorithm: {ex.Message}"
                });
            }
        }
    }

    public class GigaChatTestRequest
    {
        public string message{ get; set; }
    }
}
