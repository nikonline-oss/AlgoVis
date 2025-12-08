using AlgoVis.Core.Core;
using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.Visualization;
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

        [HttpPost("array/{sort}")]
        public AlgorithmResponse<SortingStep> ExecuteSort(string sort,[FromBody] BubbleSortRequest request)
        {
            try
            {
                var config = new AlgorithmConfig
                {
                    Name = sort,
                    Length = request.data.Length,
                    SessionId = Guid.NewGuid().ToString()
                };

                var structure = StructureFactory.CreateStructure("array", request.data);

                var result = _algorithmManager.ExecuteAlgorithm<SortingStep>(config, structure);

                return new AlgorithmResponse<SortingStep>
                {
                    success = true,
                    message = "Успешная визуализация",
                    data = result
                };
            }
            catch (Exception ex)
            {
                return new AlgorithmResponse<SortingStep>
                {
                    success = false,
                    message = $"Error executing bubble sort: {ex.Message}"
                };
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
                    Name = request.algorithmName,
                    SessionId = Guid.NewGuid().ToString(),
                    Parameters = request.parameters ?? new Dictionary<string, object>()
                };

                // В реальном приложении здесь нужно определить тип структуры на основе алгоритма
                var structure = StructureFactory.CreateStructure("array", request.data);

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
        public int[] data { get; set; } = Array.Empty<int>();
    }

    public class SearchRequest
    {
        public int[] data { get; set; } = Array.Empty<int>();
        public int target { get; set; }
    }

    public class AlgorithmResponse<TStep> where TStep : IStep
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public AlgorithmResult<TStep> data { get; set; } = new();
    }

    public class GenericAlgorithmRequest
    {
        public string algorithmName { get; set; } = string.Empty;
        public int[] data { get; set; } = Array.Empty<int>();
        public Dictionary<string, object> parameters { get; set; } = new Dictionary<string, object>();
    }
}