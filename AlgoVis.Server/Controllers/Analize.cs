using AlgoVis.Core.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace AlgoVis.Server.Controllers
{
    /// <summary>
    /// Контроллер для анализа и выполнения алгоритмов.
    /// </summary>
    /// <remarks>
    /// Этот контроллер предоставляет endpoint для отправки кода алгоритма,
    /// его трансляции в формат, понятный системе, и выполнения на сгенерированных данных.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class Analyze : ControllerBase
    {
        private readonly GigaChatService _service;
        private readonly RandomStructureFactory _factory;
        private readonly AlgorithmManager _algorithmManager;

        /// <summary>
        /// Конструктор контроллера Analyze.
        /// </summary>
        /// <remarks>
        /// Инициализирует экземпляры AlgorithmManager, RandomStructureFactory и GigaChatService.
        /// </remarks>
        public Analyze()
        {
            _algorithmManager = new AlgorithmManager();
            _factory = new RandomStructureFactory();
            _service = new GigaChatService();
        }

        /// <summary>
        /// Обрабатывает запрос на анализ и выполнение алгоритма.
        /// </summary>
        /// <param name="request">Запрос, содержащий код алгоритма и параметры визуализации.</param>
        /// <returns>Результат выполнения алгоритма или сообщение об ошибке.</returns>
        /// <response code="200">Успешное выполнение алгоритма.</response>
        /// <response code="400">Неверный запрос или ошибка при выполнении.</response>
        /// <remarks>
        /// Последовательность действий:
        /// 1. Перевод Python кода в JSON представление алгоритма
        /// 2. Десериализация в объект CustomAlgorithmRequest
        /// 3. Генерация структуры данных для алгоритма
        /// 4. Выполнение алгоритма на сгенерированной структуре
        /// </remarks>
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
                        Message = $"Translation failed: {translationResult.ValidationResult}"
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

        /// <summary>
        /// Выполняет перевод Python кода в JSON представление алгоритма через внешний сервис.
        /// </summary>
        /// <param name="request">Запрос на анализ, содержащий исходный код.</param>
        /// <returns>Результат трансляции, содержащий JSON алгоритма и информацию о валидации.</returns>
        /// <remarks>
        /// Отправляет POST запрос на локальный сервер перевода (localhost:5001/translate)
        /// и обрабатывает ответ. В случае неудачи возвращает объект с Success = false.
        /// </remarks>
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

                var response = await httpClient.PostAsync("http://localhost:5001/translate", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new TranslationResult
                    {
                        Success = false
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var translationResponse = JsonSerializer.Deserialize<TranslationServiceResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (translationResponse == null || !translationResponse.success)
                {
                    return new TranslationResult
                    {
                        Success = false
                    };
                }

                // Извлекаем JSON алгоритма из ответа
                string javaJson;
                if (translationResponse.yava is string jsonString)
                {
                    javaJson = jsonString;
                }
                else
                {
                    javaJson = JsonSerializer.Serialize(translationResponse.yava);
                }

                return new TranslationResult
                {
                    Success = true,
                    JavaJson = javaJson,
                    ValidationResult = translationResponse.validation,
                };
            }
            catch (Exception ex)
            {
                return new TranslationResult
                {
                    Success = false
                };
            }
        }
    }

    /// <summary>
    /// Запрос на анализ алгоритма.
    /// </summary>
    public class AnalyzeRequest
    {
        /// <summary>
        /// Исходный код алгоритма на Python для анализа.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Типы визуализации для генерации.
        /// </summary>
        /// <remarks>
        /// По умолчанию: compare, swap, condition, assign, complete.
        /// </remarks>
        public string[] VisualizeTypes { get; set; }

        /// <summary>
        /// Директория с модификаторами для трансляции.
        /// </summary>
        public string ModsDir { get; set; }

        /// <summary>
        /// Флаг валидации кода.
        /// </summary>
        public bool? Validate { get; set; }

        /// <summary>
        /// Флаг генерации визуализации.
        /// </summary>
        public bool? GenerateVisualization { get; set; }

        /// <summary>
        /// Флаг включения статистики.
        /// </summary>
        public bool? IncludeStatistics { get; set; }
    }

    /// <summary>
    /// Ответ от сервиса трансляции Python кода.
    /// </summary>
    public class TranslationServiceResponse
    {
        /// <summary>
        /// Флаг успешности трансляции.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Результат трансляции в формате JSON.
        /// </summary>
        public object yava { get; set; }

        /// <summary>
        /// Результат валидации кода.
        /// </summary>
        public object validation { get; set; }

        /// <summary>
        /// Статистика выполнения алгоритма.
        /// </summary>
        public object statistics { get; set; }

        /// <summary>
        /// Сообщение от сервиса трансляции.
        /// </summary>
        public string message { get; set; }
    }

    /// <summary>
    /// Результат трансляции Python кода.
    /// </summary>
    public class TranslationResult
    {
        /// <summary>
        /// Флаг успешности трансляции.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// JSON представление алгоритма.
        /// </summary>
        public string JavaJson { get; set; }

        /// <summary>
        /// Результат валидации кода.
        /// </summary>
        public object ValidationResult { get; set; }
    }
}