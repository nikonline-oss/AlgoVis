using AlgoVis.Core.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AlgoVis.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Analyze : ControllerBase
    {
        private readonly GigaChatService _service;

        private readonly RandomStructureFactory _factory;

        private readonly AlgorithmManager _algorithmManager;

        public Analyze()
        {
            _algorithmManager = new AlgorithmManager();
            _factory = new RandomStructureFactory();
            _service = new GigaChatService();
        }

        // GET: api/<Analize>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<Analize>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<Analyze>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AnallyzeRequest request)
        {
            if (request == null || request.code == null)
            {
                return BadRequest(new { Success = false, Message = "Request or Algorithm cannot be null" });
            }

            try
            {
                RandomStructureFactory factory = _factory;

                var promt = _service.GeneratePromt(request.code);
                var result = await _service.SendMessageAsync(promt);

                string pattern = @"```(?:json)?\s*(.*?)\s*```";

                Match match = Regex.Match(result, pattern, RegexOptions.Singleline);
                
                string extracted = match.Groups[1].Value;

                var algorithm = JsonSerializer.Deserialize<CustomAlgorithmRequest>(extracted);

                var defaultParams = factory.GetDefaultParameters(algorithm.structureType);

                var structure = factory.GenerateStructure(algorithm.structureType, defaultParams);

                var analyze = _algorithmManager.ExecuteCustomAlgorithm(algorithm, structure);

                return Ok(new
                {
                    Success = true,
                    ResultGiGaChar = extracted,
                    ResultInter = analyze
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

        // PUT api/<Analize>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Analize>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class AnallyzeRequest
    {
        public string code { get; set; } = string.Empty;
        public string language { get; set; } = "python";
    }
}
