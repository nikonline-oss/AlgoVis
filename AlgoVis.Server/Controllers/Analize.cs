using AlgoVis.Core.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public async Task<IActionResult> Post([FromBody] AnalyzeRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { Success = false, Message = "Request or Code cannot be null or empty" });
            }

            try
            {
                // 1. Отправляем код на трансляцию в Python сервис
                var translationResult = await TranslatePythonCode(request);

                if (!translationResult.Success)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"Translation failed: {translationResult.Error}"
                    });
                }

                // 2. Десериализуем результат трансляции
                var algorithm = JsonSerializer.Deserialize<CustomAlgorithmRequest>(
                    translationResult.JavaJson);

                // 3. Генерируем структуру данных для алгоритма
                RandomStructureFactory factory = _factory;
                var defaultParams = factory.GetDefaultParameters(algorithm.structureType);
                var structure = factory.GenerateStructure(algorithm.structureType, defaultParams);

                // 4. Выполняем алгоритм
                var executionResult = _algorithmManager.ExecuteCustomAlgorithm(algorithm, structure);


                return Ok(new
                {
                    Success = true,
                    Message = "Algorithm executed successfully",
                    Data = executionResult,
                    Translation = translationResult
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Error executing algorithm: {ex.Message}",
                    StackTrace = ex.StackTrace
                });
            }
        }

        private async Task<TranslationResult> TranslatePythonCode(AnalyzeRequest request)
        {
            try
            {
                using var httpClient = new HttpClient();

                var translationRequest = new
                {
                    code = request.Code,
                    visualize_types = request.VisualizeTypes ?? new[] { "compare", "swap", "condition", "assign", "complete" },
                    mods_dir = request.ModsDir ?? "mods",
                    validate = request.Validate ?? true,
                    generate_visualization = request.GenerateVisualization ?? true,
                    include_statistics = request.IncludeStatistics ?? true
                };

                var jsonContent = JsonSerializer.Serialize(translationRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("http://localhost:5000/translate", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new TranslationResult
                    {
                        Success = false,
                        Error = $"Translation service returned status: {response.StatusCode}"
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var translationResponse = JsonSerializer.Deserialize<TranslationServiceResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (translationResponse == null || !translationResponse.Success)
                {
                    return new TranslationResult
                    {
                        Success = false,
                        Error = translationResponse?.Error ?? "Unknown translation error"
                    };
                }

                // Извлекаем JSON алгоритма из ответа
                string javaJson;
                if (translationResponse.Java is string jsonString)
                {
                    javaJson = jsonString;
                }
                else
                {
                    javaJson = JsonSerializer.Serialize(translationResponse.Java);
                }

                return new TranslationResult
                {
                    Success = true,
                    JavaJson = javaJson,
                    ValidationResult = translationResponse.Validation,
                    Warnings = translationResponse.Warning
                };
            }
            catch (Exception ex)
            {
                return new TranslationResult
                {
                    Success = false,
                    Error = $"Translation service error: {ex.Message}"
                };
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

    public class AnalyzeRequest
    {
        public string Code { get; set; }
        public string[] VisualizeTypes { get; set; }
        public string ModsDir { get; set; }
        public bool? Validate { get; set; }
        public bool? GenerateVisualization { get; set; }
        public bool? IncludeStatistics { get; set; }
    }

    public class TranslationServiceResponse
    {
        public bool Success { get; set; }
        public object Java { get; set; }
        public object Validation { get; set; }
        public string Error { get; set; }
        public string Warning { get; set; }
    }

    public class TranslationResult
    {
        public bool Success { get; set; }
        public string JavaJson { get; set; }
        public object ValidationResult { get; set; }
        public string Warnings { get; set; }
        public string Error { get; set; }
    }

}
