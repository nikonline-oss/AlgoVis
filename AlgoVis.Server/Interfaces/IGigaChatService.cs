namespace AlgoVis.Server.Interfaces
{
    public interface IGigaChatService
    {
        Task<string> GetTokenAsync(bool forceRefresh);
        Task<string> ChatCompletionAsync(List<Dictionary<string, string>> messages, Dictionary<string, object> additionalParams = null);
        Task<string> SendMessageAsync(string message, Dictionary<string, object> additionalParams = null);
        Task<string> RefreshTokenAsync();
        Dictionary<string, object> GetTokenStatus();
    }
}
