using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AlgoVis.Core.Core;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Custom;
using AlgoVis.Models.Models.DataStructures;

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
    }

    // DTO для удобства — оборачивает CustomAlgorithmRequest и данные для структуры
    public class InterpreterTestRequest
    {
        public CustomAlgorithmRequest Algorithm { get; set; } = new CustomAlgorithmRequest();
        public object Data { get; set; } = Array.Empty<int>();
    }
}