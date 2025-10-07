using AlgoVis.Server.Data;
using AlgoVis.Server.Interfaces;
using AlgoVis.Server.Models;

namespace AlgoVis.Server.Services
{
    public class CodeAnalysisService : ICodeAnalysisService
    {
        private readonly ILogger<CodeAnalysisService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public CodeAnalysisService(
            ILogger<CodeAnalysisService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task<List<VisualizationStep>> AnalyzeCodeAsync(string code, string language)
        {
            _logger.LogInformation("Analyzing {Language} code: {CodeLength} chars", language, code.Length);

            // Заглушка для демонстрации - в реальности здесь будет интеграция с Python
            return await GenerateMockStepsAsync(code);
        }

        public async Task ProcessSessionAsync(string sessionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null) return;

            try
            {
                session.Status = "Analyzing";
                await _context.SaveChangesAsync();

                var steps = await AnalyzeCodeAsync(session.Code, session.Language);

                // Сохраняем шаги в базу
                foreach (var step in steps)
                {
                    step.SessionId = sessionId;
                    _context.Steps.Add(step);
                }

                session.Status = "Ready";
                session.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Processed session {SessionId} with {StepCount} steps",
                    sessionId, steps.Count);
            }
            catch (Exception ex)
            {
                session.Status = "Error";
                await _context.SaveChangesAsync();
                _logger.LogError(ex, "Error processing session {SessionId}", sessionId);
                throw;
            }
        }

        private async Task<List<VisualizationStep>> GenerateMockStepsAsync(string code)
        {
            await Task.Delay(1000);

            var steps = new List<VisualizationStep>();
            var array = new[] { 5, 3, 8, 1, 2 };

            // Демо-шаги для сортировки пузырьком
            steps.Add(new VisualizationStep
            {
                StepNumber = 0,
                Operation = "init_array",
                StructureType = "array",
                StructureId = "main",
                Parameters = new Dictionary<string, object> { ["values"] = array },
                Description = "Инициализация массива",
                StateSnapshot = new Dictionary<string, object> { ["array"] = array }
            });

            // Генерация шагов сортировки...
            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - i - 1; j++)
                {
                    steps.Add(new VisualizationStep
                    {
                        StepNumber = steps.Count,
                        Operation = "compare",
                        StructureType = "array",
                        StructureId = "main",
                        Parameters = new Dictionary<string, object>
                        {
                            ["index1"] = j,
                            ["index2"] = j + 1
                        },
                        Description = $"Сравнение элементов на позициях {j} и {j + 1}"
                    });

                    if (array[j] > array[j + 1])
                    {
                        (array[j], array[j + 1]) = (array[j + 1], array[j]);

                        steps.Add(new VisualizationStep
                        {
                            StepNumber = steps.Count,
                            Operation = "swap",
                            StructureType = "array",
                            StructureId = "main",
                            Parameters = new Dictionary<string, object>
                            {
                                ["index1"] = j,
                                ["index2"] = j + 1
                            },
                            Description = $"Обмен элементов {j} и {j + 1}",
                            StateSnapshot = new Dictionary<string, object> { ["array"] = array.Clone() }
                        });
                    }
                }
            }

            return steps;
        }
    }
}