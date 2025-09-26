namespace AlgoVis.Server.DTO
{
    public class CreateSessionRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Language { get; set; } = "python";
        public string ClientConnectionId { get; set; } = string.Empty;
    }

    public class SessionResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<StepResponse> Steps { get; set; } = new();
        public int CurrentStepIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class StepResponse
    {
        public int StepNumber { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string StructureType { get; set; } = string.Empty;
        public string StructureId { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public Dictionary<string, object> StateSnapshot { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }
}
