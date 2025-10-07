using AlgoVis.Server.Data;
using AlgoVis.Server.DTO;
using AlgoVis.Server.Hubs;
using AlgoVis.Server.Interfaces;
using AlgoVis.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoVis.Server.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICodeAnalysisService _codeAnalysisService;
        private readonly ILogger<SessionService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SessionService(ApplicationDbContext context, ILogger<SessionService> logger, IServiceScopeFactory scopeFactory, ICodeAnalysisService codeAnalysisService)
        {
            _scopeFactory = scopeFactory;
            _context = context;
            _logger = logger;
            _codeAnalysisService = codeAnalysisService;
        }


        public async Task StartBackgroundAnalysis(string sessionId)
        {
            // Создаем новый scope для фоновой задачи
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<VisualizationHub>>();

            // Получаем connectionId из базы
            var session = await context.Sessions.FindAsync(sessionId);
            var connectionId = session.ClientConnectionId;

            try
            {
                // Выполняем анализ
                await _codeAnalysisService.ProcessSessionAsync(sessionId);
                // Отправляем результат через hubContext
                await hubContext.Clients.Client(connectionId).SendAsync("AnalysisCompleted", sessionId);
            }
            catch (Exception ex)
            {
                await hubContext.Clients.Client(connectionId).SendAsync("AnalysisFailed", sessionId, ex.Message);
            }
        }

        public async Task UpdateSessionConnectionIdAsync(string sessionId, string connectionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var session = await context.Sessions.FindAsync(sessionId);
            if (session != null)
            {
                session.ClientConnectionId = connectionId; // предполагая, что такое поле есть в модели
                await context.SaveChangesAsync();
            }
        }

        public async Task<SessionResponse> CreateSessionAsync(CreateSessionRequest request)
        {
            var session = new VisualizationSession
            {
                ClientConnectionId = request.ClientConnectionId,
                Code = request.Code,
                Language = request.Language,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created session {SessionId} for client {ClientId}",
                session.Id, request.ClientConnectionId);

            return await GetSessionAsync(session.Id);
        }

        public async Task<SessionResponse> GetSessionAsync(string sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Steps.OrderBy(step => step.StepNumber))
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                throw new KeyNotFoundException($"Session {sessionId} not found");

            return MapToResponse(session);
        }

        public async Task<SessionResponse> UpdateSessionStepAsync(string sessionId, int stepIndex)
        {
            var session = await _context.Sessions
                .Include(s => s.Steps)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                throw new KeyNotFoundException($"Session {sessionId} not found");

            if (stepIndex < 0 || stepIndex >= session.Steps.Count)
                throw new ArgumentOutOfRangeException(nameof(stepIndex), "Invalid step index");

            session.CurrentStepIndex = stepIndex;
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(session);
        }

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null) return false;

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted session {SessionId}", sessionId);
            return true;
        }

        public async Task<List<SessionResponse>> GetSessionsByClientAsync(string clientConnectionId)
        {
            var sessions = await _context.Sessions
                .Where(s => s.ClientConnectionId == clientConnectionId)
                .Include(s => s.Steps.OrderBy(step => step.StepNumber))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return sessions.Select(MapToResponse).ToList();
        }

        private SessionResponse MapToResponse(VisualizationSession session)
        {
            return new SessionResponse
            {
                SessionId = session.Id,
                Status = session.Status,
                CurrentStepIndex = session.CurrentStepIndex,
                CreatedAt = session.CreatedAt,
                Steps = session.Steps.Select(step => new StepResponse
                {
                    StepNumber = step.StepNumber,
                    Operation = step.Operation,
                    StructureType = step.StructureType,
                    StructureId = step.StructureId,
                    Parameters = step.Parameters,
                    StateSnapshot = step.StateSnapshot,
                    Description = step.Description
                }).ToList()
            };
        }

    }
}
