using AlgoVis.Server.Models;
using AlgoVis.
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlgorithmsController : ControllerBase
    {
        private readonly AlgorithmManager _algorithmManager;

        public AlgorithmsController()
        {
            _algorithmManager = new AlgorithmManager();
        }

        [HttpPost("bubble-sort")]
        public IActionResult ExecuteBubbleSort([FromBody] BubbleSortRequest request)
        {
            try
            {
                // Создаем конфигурацию алгоритма
                var config = new AlgorithmConfig
                {
                    Name = "BubbleSort",
                    Length = request.Data.Length,
                    SessionId = Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, object>
                    {
                        ["Detailed"] = request.Detailed
                    }
                };

                // Создаем структуру данных
                var structure = StructureFactory.CreateStructure("array", request.Data);

                // Выполняем алгоритм
                var result = _algorithmManager.ExecuteAlgorithm(config, structure);

                return Ok(new
                {
                    Success = true,
                    Result = result,
                    Message = "Bubble sort executed successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Error executing bubble sort: {ex.Message}"
                });
            }
        }

        [HttpGet("available")]
        public IActionResult GetAvailableAlgorithms()
        {
            try
            {
                var algorithms = _algorithmManager.GetAvailableAlgorithms();
                return Ok(new
                {
                    Success = true,
                    Algorithms = algorithms
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Error retrieving algorithms: {ex.Message}"
                });
            }
        }

        [HttpPost("execute")]
        public IActionResult ExecuteAlgorithm([FromBody] GenericAlgorithmRequest request)
        {
            try
            {
                var config = new AlgorithmConfig
                {
                    Name = request.AlgorithmName,
                    SessionId = Guid.NewGuid().ToString(),
                    Parameters = request.Parameters ?? new Dictionary<string, object>()
                };

                // В реальном приложении здесь нужно определить тип структуры на основе алгоритма
                var structure = StructureFactory.CreateStructure("array", request.Data);

                var result = _algorithmManager.ExecuteAlgorithm(config, structure);

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
                    Message = $"Error executing algorithm: {ex.Message}"
                });
            }
        }
    }

    public class BubbleSortRequest
    {
        public int[] Data { get; set; } = Array.Empty<int>();
        public bool Detailed { get; set; } = true;
    }

    public class GenericAlgorithmRequest
    {
        public string AlgorithmName { get; set; } = string.Empty;
        public int[] Data { get; set; } = Array.Empty<int>();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}