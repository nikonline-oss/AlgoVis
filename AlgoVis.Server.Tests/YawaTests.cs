using Xunit;
using System.Net.Http.Json;
using AlgoVis.Server.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoVis.Server.Tests;

public sealed class YawaTests : TestBase
{
    public YawaTests(TestAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var r = await Client.GetAsync<object>("/api/yawa/health");
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Samples_ReturnsList()
    {
        var r = await Client.GetAsync<List<object>>("/api/yawa/samples");
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Data);
    }

    [Fact]
    public async Task RunPython_ValidCode_ReturnsTrace()
    {
        var r = await Client.PostAsync<TraceDto>("/api/yawa/run-python",
            new { code = BubbleSortCode });
        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.NotNull(r.Data!.Steps);
        Assert.True(r.Data.Steps!.Count > 0);
        Assert.NotNull(r.Data.Statistics);
        Assert.True(r.Data.Statistics!.Comparisons > 0);
    }

    [Fact]
    public async Task RunPython_TranspilerError_Returns200WithErrorKind()
    {
        var r = await Client.PostAsync<ErrorDto>("/api/yawa/run-python",
            new { code = "def main(\n    pass" });

        // Контроллер отдаёт 200 с телом { error, kind, line, column }
        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.NotNull(r.Data!.Error);
        Assert.Equal("transpiler", r.Data.Kind);
    }

    public sealed class ErrorDto
    {
        public string? Error { get; set; }
        public string? Kind { get; set; }
        public int? Line { get; set; }
        public int? Column { get; set; }
    }

    [Fact]
    public async Task RunPython_RuntimeError_Returns400WithMessage()
    {
        var r = await Client.PostAsync<object>("/api/yawa/run-python",
            new { code = "def main():\n    x = unknown_y + 1" });
        // В нашем контроллере runtime error возвращается как 200 с полем error
        Assert.True(r.IsSuccess);
        Assert.Contains("unknown_y", r.RawText);
    }

    [Fact]
    public async Task RunPython_EmptyBody_Returns400()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/yawa/run-python")
        {
            Content = new StringContent("", System.Text.Encoding.UTF8, "text/plain")
        };
        var r = await Client.SendRawAsync(req);
        Assert.Equal(400, (int)r.StatusCode);
    }

    [Fact]
    public async Task RunPython_NoAuthRequired()
    {
        // Публичный endpoint — работает без токена
        ClearAuth();
        var r = await Client.PostAsync<TraceDto>("/api/yawa/run-python",
            new { code = BubbleSortCode });
        Assert.True(r.IsSuccess);
    }

    // ─────────── C++ (run-cpp / run-code) ───────────

    [Fact]
    public async Task RunCpp_ValidCode_ReturnsTrace()
    {
        var r = await Client.PostAsync<TraceDto>("/api/yawa/run-cpp",
            new { code = "int main() { int x = 5; int y = x + 1; return 0; }" });

        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.NotNull(r.Data!.Steps);
        Assert.True(r.Data.Steps!.Count > 0);
        Assert.NotNull(r.Data.Statistics);
    }

    [Fact]
    public async Task RunCpp_NoAuthRequired()
    {
        ClearAuth();
        var r = await Client.PostAsync<TraceDto>("/api/yawa/run-cpp",
            new { code = "int main() { int a = 1; return 0; }" });
        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
    }

    [Fact]
    public async Task RunCode_LanguageCpp_DispatchesToCpp()
    {
        var r = await Client.PostAsync<TraceDto>("/api/yawa/run-code",
            new { code = "int main() { int a = 1; int b = a + 1; return 0; }", language = "cpp" });
        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.NotNull(r.Data!.Steps);
        Assert.True(r.Data.Steps!.Count > 0);
    }

    [Fact]
    public async Task RunCode_UnsupportedLanguage_Returns400()
    {
        var r = await Client.PostAsync<ErrorDto>("/api/yawa/run-code",
            new { code = "def main(): pass", language = "rust" });
        Assert.Equal(400, (int)r.Status);
    }

    [Fact]
    public async Task RunCpp_TranspilerError_ReturnsKindTranspiler()
    {
        // Синтаксически невалидный C++ — транспайлер должен отдать kind=transpiler
        var r = await Client.PostAsync<ErrorDto>("/api/yawa/run-cpp",
            new { code = "int main( {" });
        Assert.True(r.IsSuccess, $"Status={r.Status} Body={r.RawText}");
        Assert.NotNull(r.Data);
        Assert.NotNull(r.Data!.Error);
        Assert.Equal("transpiler", r.Data.Kind);
    }

    // ─────────── DTO ───────────

    public sealed class TraceDto
    {
        public string? SessionId { get; set; }
        public List<StepDto>? Steps { get; set; }
        public StatsDto? Statistics { get; set; }
    }

    public sealed class StepDto
    {
        public int N { get; set; }
        public string Kind { get; set; } = "";
    }

    public sealed class StatsDto
    {
        public int TotalSteps { get; set; }
        public int Comparisons { get; set; }
        public int Swaps { get; set; }
    }
}
