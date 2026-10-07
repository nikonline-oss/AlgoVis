namespace AlgoVis.Server.Controllers.Yawa;

public sealed record ApiError(string Error, string Kind, string? Detail = null)
{
    public static ApiError Validation(string msg, string? detail = null) =>
        new(msg, "validation", detail);

    public static ApiError Runtime(string msg, string? detail = null) =>
        new(msg, "runtime", detail);

    public static ApiError Internal(string msg, string? detail = null) =>
        new(msg, "internal", detail);
}