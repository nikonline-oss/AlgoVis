using AlgoVis.Server.DTO;
using AlgoVis.Server.Models;

namespace AlgoVis.Server.Interfaces
{
    public interface ISessionService
    {
        Task<SessionResponse> CreateSessionAsync(CreateSessionRequest request);
        Task<SessionResponse> GetSessionAsync(string sessionId);
        Task<SessionResponse> UpdateSessionStepAsync(string sessionId, int stepIndex);
        Task UpdateSessionConnectionIdAsync(string sessionId, string connectionId);
        Task<bool> DeleteSessionAsync(string sessionId);
        Task StartBackgroundAnalysis(string sessionId);

        Task<List<SessionResponse>> GetSessionsByClientAsync(string clientConnrctionId);
    }

    public interface ICodeAnalysisService
    {
        Task<List<VisualizationStep>> AnalyzeCodeAsync(string code, string language);
        Task ProcessSessionAsync(string sessionId);
    }
}
