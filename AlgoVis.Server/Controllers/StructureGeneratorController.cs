using AlgoVis.Core.Core;
using AlgoVis.Core.Core.GenerateState;
using AlgoVis.Models.Models.DataStructures;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Suport;
using AlgoVis.Server.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;
using ParameterInfo = AlgoVis.Server.DTO.ParameterInfo;

namespace AlgoVis.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StructureGeneratorController : ControllerBase
    {
        private readonly RandomStructureFactory _factory;
        private readonly ILogger<StructureGeneratorController> _logger;

        public StructureGeneratorController(RandomStructureFactory factory, ILogger<StructureGeneratorController> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        /// <summary>
        /// Получить список поддерживаемых структур
        /// </summary>
        [HttpGet("supported")]
        public ActionResult<SupportedStructuresResponse> GetSupportedStructures()
        {
            try
            {
                var response = new SupportedStructuresResponse();
                var availableStructures = _factory.GetAvailableStructures();

                foreach (var structureType in availableStructures)
                {
                    var defaultParams = _factory.GetDefaultParameters(structureType);
                    var structureInfo = new StructureInfo
                    {
                        Type = structureType,
                        Description = GetStructureDescription(structureType),
                        DefaultParameters = defaultParams,
                        ParametersInfo = GetParametersInfo(structureType)
                    };

                    response.Structures.Add(structureInfo);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported structures");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить параметры по умолчанию для конкретной структуры
        /// </summary>
        [HttpGet("parameters/{structureType}")]
        public ActionResult<Dictionary<string, object>> GetDefaultParameters(string structureType)
        {
            try
            {
                var parameters = _factory.GetDefaultParameters(structureType);
                return Ok(parameters);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default parameters for {StructureType}", structureType);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Сгенерировать структуру с заданными параметрами
        /// </summary>
        [HttpPost("generate")]
        public ActionResult<StructureGenerationResponse> GenerateStructure([FromBody] GenerateStructureRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Generating {StructureType} with parameters: {Parameters}",
                    request.StructureType,
                    System.Text.Json.JsonSerializer.Serialize(request.Parameters));

                // Если указан seed, создаем фабрику с заданным seed
                RandomStructureFactory factory = request.Seed.HasValue
                    ? CreateSeededFactory(request.Seed.Value)
                    : _factory;

                var structure = factory.GenerateStructure(request.StructureType, request.Parameters);
                var visualizationData = structure.ToVisualizationData();
                var state = structure.GetState();

                var metadata = CreateMetadata(structure, stopwatch.Elapsed);

                var response = new StructureGenerationResponse
                {
                    Success = true,
                    StructureType = request.StructureType,
                    VisualizationData = visualizationData,
                    UsedParameters = request.Parameters.Any() ? request.Parameters : _factory.GetDefaultParameters(request.StructureType),
                    Metadata = metadata,
                    State = state
                };

                _logger.LogInformation("Successfully generated {StructureType} in {ElapsedMs}ms",
                    request.StructureType, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid structure type or parameters: {StructureType}", request.StructureType);
                return BadRequest(new StructureGenerationResponse
                {
                    Success = false,
                    StructureType = request.StructureType,
                    Error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating structure {StructureType}", request.StructureType);
                return StatusCode(500, new StructureGenerationResponse
                {
                    Success = false,
                    StructureType = request.StructureType,
                    Error = ex.Message
                });
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Сгенерировать несколько структур за один запрос
        /// </summary>
        [HttpPost("generate-multiple")]
        public ActionResult<TestAllResponse> GenerateMultipleStructures([FromBody] TestAllStructuresRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = new TestAllResponse();

            try
            {
                // Если указан seed, создаем фабрику с заданным seed
                RandomStructureFactory factory = request.Seed.HasValue
                    ? CreateSeededFactory(request.Seed.Value)
                    : _factory;

                foreach (var (structureType, parameters) in request.StructureParameters)
                {
                    var structureStopwatch = Stopwatch.StartNew();

                    try
                    {
                        var structure = factory.GenerateStructure(structureType, parameters);
                        var visualizationData = structure.ToVisualizationData();
                        var metadata = CreateMetadata(structure, structureStopwatch.Elapsed);

                        response.Results[structureType] = new StructureGenerationResponse
                        {
                            Success = true,
                            StructureType = structureType,
                            VisualizationData = visualizationData,
                            UsedParameters = parameters,
                            Metadata = metadata
                        };

                        _logger.LogInformation("Successfully generated {StructureType}", structureType);
                    }
                    catch (Exception ex)
                    {
                        response.Results[structureType] = new StructureGenerationResponse
                        {
                            Success = false,
                            StructureType = structureType,
                            Error = ex.Message
                        };

                        _logger.LogWarning(ex, "Failed to generate {StructureType}", structureType);
                    }
                    finally
                    {
                        structureStopwatch.Stop();
                    }
                }

                response.Success = true;
                response.TotalTime = $"{stopwatch.Elapsed.TotalMilliseconds}ms";

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch structure generation");
                return StatusCode(500, new TestAllResponse
                {
                    Success = false,
                    Error = ex.Message
                });
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Быстрая генерация всех типов структур с параметрами по умолчанию
        /// </summary>
        [HttpPost("generate-all")]
        public ActionResult<TestAllResponse> GenerateAllStructures([FromBody] int? seed = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = new TestAllResponse();

            try
            {
                RandomStructureFactory factory = seed.HasValue
                    ? CreateSeededFactory(seed.Value)
                    : _factory;

                var availableStructures = factory.GetAvailableStructures();

                foreach (var structureType in availableStructures)
                {
                    var structureStopwatch = Stopwatch.StartNew();

                    try
                    {
                        var defaultParameters = factory.GetDefaultParameters(structureType);
                        var structure = factory.GenerateStructure(structureType, defaultParameters);
                        var visualizationData = structure.ToVisualizationData();
                        var metadata = CreateMetadata(structure, structureStopwatch.Elapsed);

                        response.Results[structureType] = new StructureGenerationResponse
                        {
                            Success = true,
                            StructureType = structureType,
                            VisualizationData = visualizationData,
                            UsedParameters = defaultParameters,
                            Metadata = metadata
                        };

                        _logger.LogInformation("Successfully generated {StructureType} with default parameters", structureType);
                    }
                    catch (Exception ex)
                    {
                        response.Results[structureType] = new StructureGenerationResponse
                        {
                            Success = false,
                            StructureType = structureType,
                            Error = ex.Message
                        };

                        _logger.LogWarning(ex, "Failed to generate {StructureType} with default parameters", structureType);
                    }
                    finally
                    {
                        structureStopwatch.Stop();
                    }
                }

                response.Success = true;
                response.TotalTime = $"{stopwatch.Elapsed.TotalMilliseconds}ms";

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating all structures");
                return StatusCode(500, new TestAllResponse
                {
                    Success = false,
                    Error = ex.Message
                });
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Тестовый метод для проверки конкретных сценариев
        /// </summary>
        [HttpPost("test-scenarios")]
        public ActionResult<TestAllResponse> TestScenarios()
        {
            var testScenarios = new Dictionary<string, Dictionary<string, object>>
            {
                ["array"] = new Dictionary<string, object>
                {
                    ["size"] = 15,
                    ["minValue"] = -50,
                    ["maxValue"] = 100,
                    ["sorted"] = 0
                },
                ["graph"] = new Dictionary<string, object>
                {
                    ["nodeCount"] = 12,
                    ["type"] = "circular",
                    ["minWeight"] = 1,
                    ["maxWeight"] = 20
                },
                ["binarytree"] = new Dictionary<string, object>
                {
                    ["nodeCount"] = 15,
                    ["type"] = "balanced",
                    ["minValue"] = 1,
                    ["maxValue"] = 100
                }
            };

            var request = new TestAllStructuresRequest
            {
                StructureParameters = testScenarios,
                Seed = 42 // Фиксированный seed для воспроизводимости
            };

            return GenerateMultipleStructures(request);
        }

        #region Helper Methods

        private RandomStructureFactory CreateSeededFactory(int seed)
        {
            var seededFactory = new RandomStructureFactory();

            // Перерегистрируем генераторы с заданным seed
            seededFactory.RegisterGenerator("array", new ArrayRandomGenerator(seed));
            seededFactory.RegisterGenerator("graph", new GraphRandomGenerator(seed));
            seededFactory.RegisterGenerator("binarytree", new BinaryTreeRandomGenerator(seed));

            return seededFactory;
        }

        private StructureMetadata CreateMetadata(IDataStructure structure, TimeSpan generationTime)
        {
            var metadata = new StructureMetadata
            {
                GenerationTime = $"{generationTime.TotalMilliseconds}ms"
            };

            switch (structure)
            {
                case ArrayStructure array:
                    var arrayData = array.GetState();
                    metadata.ElementCount = arrayData.Length;
                    break;

                case GraphStructure graph:
                    metadata.NodeCount = graph.Nodes.Count;
                    metadata.EdgeCount = graph.Edges.Count;
                    metadata.ElementCount = graph.Nodes.Count;
                    break;

                case BinaryTreeStructure tree:
                    metadata.NodeCount = CountTreeNodes(tree.Root);
                    metadata.ElementCount = metadata.NodeCount;
                    break;

                case LinkedListStructure list:
                    metadata.NodeCount = CountListNodes(list.Head);
                    metadata.ElementCount = metadata.NodeCount;
                    break;
            }

            return metadata;
        }

        private int CountTreeNodes(TreeNode node)
        {
            if (node == null) return 0;
            return 1 + CountTreeNodes(node.Left) + CountTreeNodes(node.Right);
        }

        private int CountListNodes(ListNode head)
        {
            int count = 0;
            var current = head;
            var visited = new HashSet<string>();

            while (current != null && !visited.Contains(current.Id))
            {
                visited.Add(current.Id);
                count++;
                current = current.Next;
            }

            return count;
        }

        private string GetStructureDescription(string structureType)
        {
            return structureType.ToLower() switch
            {
                "array" => "One-dimensional array of integers",
                "graph" => "Graph with nodes and weighted edges",
                "binarytree" => "Binary tree structure",
                "linkedlist" => "Singly linked list",
                _ => "Data structure"
            };
        }

        private List<ParameterInfo> GetParametersInfo(string structureType)
        {
            return structureType.ToLower() switch
            {
                "array" => new List<ParameterInfo>
                {
                    new() { Name = "size", Type = "int", Description = "Number of elements", DefaultValue = 10 },
                    new() { Name = "minValue", Type = "int", Description = "Minimum value", DefaultValue = 0 },
                    new() { Name = "maxValue", Type = "int", Description = "Maximum value", DefaultValue = 100 },
                    new() { Name = "sorted", Type = "bool", Description = "Whether array should be sorted", DefaultValue = false }
                },
                "graph" => new List<ParameterInfo>
                {
                    new() { Name = "nodeCount", Type = "int", Description = "Number of nodes", DefaultValue = 8 },
                    new() { Name = "type", Type = "string", Description = "Graph type (circular, grid, complete, random)", DefaultValue = "random" },
                    new() { Name = "minWeight", Type = "int", Description = "Minimum edge weight", DefaultValue = 1 },
                    new() { Name = "maxWeight", Type = "int", Description = "Maximum edge weight", DefaultValue = 10 },
                    new() { Name = "directed", Type = "bool", Description = "Whether graph is directed", DefaultValue = false }
                },
                "binarytree" => new List<ParameterInfo>
                {
                    new() { Name = "nodeCount", Type = "int", Description = "Number of nodes", DefaultValue = 7 },
                    new() { Name = "type", Type = "string", Description = "Tree type (complete, balanced, random)", DefaultValue = "balanced" },
                    new() { Name = "minValue", Type = "int", Description = "Minimum node value", DefaultValue = 1 },
                    new() { Name = "maxValue", Type = "int", Description = "Maximum node value", DefaultValue = 100 }
                },
                "linkedlist" => new List<ParameterInfo>
                {
                    new() { Name = "size", Type = "int", Description = "Number of nodes", DefaultValue = 5 },
                    new() { Name = "minValue", Type = "int", Description = "Minimum node value", DefaultValue = 1 },
                    new() { Name = "maxValue", Type = "int", Description = "Maximum node value", DefaultValue = 100 },
                    new() { Name = "hasCycle", Type = "bool", Description = "Whether list has cycle", DefaultValue = false },
                    new() { Name = "cyclePosition", Type = "int", Description = "Position where cycle starts (-1 for random)", DefaultValue = -1 }
                },
                _ => new List<ParameterInfo>()
            };
        }

        #endregion
    }
}
