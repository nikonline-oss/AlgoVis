using AlgoVis.Server.DTO;
using AlgoVis.Server.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AlgoVis.Server.Hubs
{
    public class VisualizationHub : Hub
    {
        private readonly ISessionService _sessionService;
        private readonly ICodeAnalysisService _codeAnalysisService;
        private readonly ILogger<VisualizationHub> _logger;

        public VisualizationHub(
            ISessionService sessionService,
            ICodeAnalysisService codeAnalysisService,
            ILogger<VisualizationHub> logger)
        {
            _sessionService = sessionService;
            _codeAnalysisService = codeAnalysisService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<string> CreateSession(string code, string language = "python")
        {
            var request = new CreateSessionRequest
            {
                Code = code,
                Language = language,
                ClientConnectionId = Context.ConnectionId
            };

            var session = await _sessionService.CreateSessionAsync(request);

            // Запускаем анализ в фоне
            _ = Task.Run(async () =>
            {
                try
                {
                    await _codeAnalysisService.ProcessSessionAsync(session.SessionId);
                    await Clients.Caller.SendAsync("AnalysisCompleted", session.SessionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background analysis for session {SessionId}", session.SessionId);
                    await Clients.Caller.SendAsync("AnalysisFailed", session.SessionId, ex.Message);
                }
            });

            return session.SessionId;
        }

        public async Task<SessionResponse> GetSessionState(string sessionId)
        {
            return await _sessionService.GetSessionAsync(sessionId);
        }

        public async Task<bool> GoToStep(string sessionId, int stepIndex)
        {
            try
            {
                await _sessionService.UpdateSessionStepAsync(sessionId, stepIndex);
                var session = await _sessionService.GetSessionAsync(sessionId);
                var step = session.Steps.FirstOrDefault(s => s.StepNumber == stepIndex);

                if (step != null)
                {
                    await Clients.Caller.SendAsync("StepUpdated", step);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error going to step {StepIndex} in session {SessionId}", stepIndex, sessionId);
                return false;
            }
        }
    }
}
