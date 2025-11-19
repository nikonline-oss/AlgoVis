using AlgoVis.Core.Core;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlgoVis.Server.Controllers
{ 
    [ApiController]
    [Route("api/[controller]")]
    public class CustomInterpreterController : ControllerBase
    {
        private readonly AlgorithmManager _algorithmManager;

        public CustomInterpreterController()
        {
            _algorithmManager = new AlgorithmManager();
        }

        /// <summary>
        /// Выполнить кастомный алгоритм через AlgorithmInterpreter.
        /// Тело запроса содержит объект Algorithm (CustomAlgorithmRequest) и опционально массив данных для структуры.
        /// </summary>
        [HttpPost("execute")]
        public IActionResult ExecuteCustomAlgorithm([FromBody] InterpreterTestRequest request)
        {
            if (request == null || request.Algorithm == null)
            {
                return BadRequest(new { Success = false, Message = "Request or Algorithm cannot be null" });
            }

            try
            {
                var data = request.Data ?? Array.Empty<int>();
                var structure = StructureFactory.CreateStructure(request.Algorithm.structureType, data);

                var result = _algorithmManager.ExecuteCustomAlgorithm(request.Algorithm, structure);

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
        [HttpGet("test")]
        public IActionResult ExecuteCustomAlgorithmTest()
        {
            try
            {
                // Чтение JSON из файла
                string filePath = "C:\\2025\\Project\\Программная инженерия\\AlgoVis\\AlgoVis.Server\\InstructJson.json"; // укажите правильный путь
                //var json = File.ReadAllText(jsonFilePath)

                // Десериализация JSON в объект
                //var request = JsonSerializer.Deserialize<InterpreterTestRequest>(jsonContent);

                // Если у вас нет класса CustomAlgorithmRequest, создайте его или используйте dynamic
                // dynamic request = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);

                var jsons = System.IO.File.ReadAllText(filePath);
                var request = JsonSerializer.Deserialize<InterpreterTestRequest>(jsons);

                var data = request.Data ?? Array.Empty<int>();
                var structure = StructureFactory.CreateStructure(request.Algorithm.structureType, data);

                var result = _algorithmManager.ExecuteCustomAlgorithm(request.Algorithm, structure);

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

    // DTO для удобства — оборачивает CustomAlgorithmRequest и данные для структуры
    public class InterpreterTestRequest
    {
        [JsonPropertyName("algorithm")]
        public CustomAlgorithmRequest Algorithm { get; set; } = new CustomAlgorithmRequest();
        [JsonPropertyName("data")]
        public object Data { get; set; } = Array.Empty<int>();
    }
}