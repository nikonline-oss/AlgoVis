using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using System.Text.Json.Serialization;

namespace AlgoVis.Server.Tests.Infrastructure;

/// <summary>
/// Обёртка над HttpClient для тестов: следит за access-токеном,
/// парсит ответы, даёт короткие методы.
/// </summary>
public sealed class ApiClient
{
    private readonly HttpClient _http;

    public static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ApiClient(HttpClient http) => _http = http;

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }

    public void SetTokens(string? access, string? refresh = null)
    {
        AccessToken = access;
        if (refresh is not null) RefreshToken = refresh;
    }

    public void ClearTokens()
    {
        AccessToken = null;
        RefreshToken = null;
    }

    // ─────────── HTTP ───────────

    public async Task<ApiResponse<T>> GetAsync<T>(string path)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, path);
        return await SendAsync<T>(req);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string path, object? body)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, path);
        if (body is not null)
            req.Content = JsonContent.Create(body, options: JsonOpts);
        return await SendAsync<T>(req);
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string path, object? body)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, path);
        if (body is not null)
            req.Content = JsonContent.Create(body, options: JsonOpts);
        return await SendAsync<T>(req);
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string path, object? body = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, path);
        if (body is not null)
            req.Content = JsonContent.Create(body, options: JsonOpts);
        return await SendAsync<T>(req);
    }

    public async Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage req)
    {
        AttachAuth(req);
        return await _http.SendAsync(req);
    }

    private async Task<ApiResponse<T>> SendAsync<T>(HttpRequestMessage req)
    {
        AttachAuth(req);
        var r = await _http.SendAsync(req);
        var text = await r.Content.ReadAsStringAsync();

        T? data = default;
        if (!string.IsNullOrWhiteSpace(text))
        {
            try
            {
                data = JsonSerializer.Deserialize<T>(text, JsonOpts);
            }
            catch
            {
                // оставляем как есть — тест увидит null
            }
        }

        return new ApiResponse<T>
        {
            Status = (int)r.StatusCode,
            Data = data,
            RawText = text
        };
    }

    private void AttachAuth(HttpRequestMessage req)
    {
        if (!string.IsNullOrEmpty(AccessToken))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
    }
}

public sealed class ApiResponse<T>
{
    public int Status { get; init; }
    public T? Data { get; init; }
    public string RawText { get; init; } = "";

    public bool IsSuccess => Status >= 200 && Status < 300;
}
