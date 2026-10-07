using System.Text.Json;
using AlgoVis.Yawa.Runtime;
using AlgoVis.Yawa.Trace;
using AlgoVis.Yawa.Yawa;
using AlgoVis.Yawa.Yawa.Loader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoVis.Server.Controllers.Yawa;

[ApiController]
[Route("api/yawa")]
public sealed class YawaController : ControllerBase
{
    // Жёсткий потолок сверху, независимо от того, что указано в YAWA.
    private const int HardMaxSteps = 1_000_000;
    private const int HardMaxDepth = 5_000;
    private const double HardMaxSeconds = 30.0;

    private readonly ILogger<YawaController> _log;

    public YawaController(ILogger<YawaController> log) => _log = log;

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", service = "yawa", version = "1.0" });

    /// <summary>
    /// Основной endpoint. Принимает YAWA-JSON в теле, исполняет, возвращает Trace-JSON.
    /// Content-Type: application/json
    /// </summary>
    [HttpPost("run")]
    [RequestSizeLimit(2 * 1024 * 1024)] // 2 MB
    public async Task<IActionResult> Run(CancellationToken ct)
    {
        string body;
        try
        {
            using var reader = new StreamReader(Request.Body);
            body = await reader.ReadToEndAsync(ct);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiError.Validation("Не удалось прочитать тело запроса", ex.Message));
        }

        if (string.IsNullOrWhiteSpace(body))
            return BadRequest(ApiError.Validation("Пустое тело запроса"));

        YawaProgram program;
        try
        {
            program = YawaLoader.FromJson(body);
        }
        catch (JsonException jx)
        {
            return BadRequest(ApiError.Validation("Некорректный YAWA-JSON", jx.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiError.Validation("Ошибка загрузки YAWA", ex.Message));
        }

        var opts = new InterpreterOptions
        {
            MaxSteps = Math.Min(program.Limits.MaxSteps, HardMaxSteps),
            MaxDepth = Math.Min(program.Limits.MaxDepth, HardMaxDepth),
            MaxSeconds = Math.Min(program.Limits.MaxSeconds, HardMaxSeconds),
            SnapshotEvery = program.Limits.SnapshotEvery,
            CollectSteps = true
        };

        TraceSession session;
        try
        {
            var interp = new Interpreter(program, opts);
            session = interp.Run();
        }
        catch (YawaRuntimeException rex)
        {
            _log.LogInformation("YAWA runtime error: {Msg}", rex.Message);
            return Ok(new
            {
                error = rex.Message,
                kind = "runtime",
                session_id = (string?)null
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "YAWA unexpected error");
            return StatusCode(500, ApiError.Internal("Внутренняя ошибка интерпретатора", ex.Message));
        }

        var limitHit = session.Steps.Count > 0 &&
                       session.Steps[^1].Kind == "error";

        if (limitHit)
        {
            _log.LogInformation(
    "YAWA run: name={Name}, steps={Steps}, cmp={Cmp}, swaps={Swaps}",
    program.Metadata.Name, session.Statistics.TotalSteps,
    session.Statistics.Comparisons, session.Statistics.Swaps);
            return Ok(new
            {
                error = session.Steps[^1].Annotation ?? "Превышены лимиты исполнения",
                kind = "runtime",
                session
            });
        }
        _log.LogInformation(
    "YAWA run: name={Name}, steps={Steps}, cmp={Cmp}, swaps={Swaps}",
    program.Metadata.Name, session.Statistics.TotalSteps,
    session.Statistics.Comparisons, session.Statistics.Swaps);

        return Ok(session);
    }

    /// <summary>
    /// Вспомогательный endpoint для отладки: принимает path к локальному
    /// YAWA-файлу на сервере (в пределах samples/).
    /// </summary>
    [HttpPost("run-sample")]
    public IActionResult RunSample([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains("..") || name.Contains('/'))
            return BadRequest(ApiError.Validation("Некорректное имя примера"));

        var samplesRoot = FindSamplesRoot();
        if (samplesRoot is null)
            return StatusCode(500, ApiError.Internal("Папка samples/ не найдена"));

        var path = Path.Combine(samplesRoot, name);
        if (!System.IO.File.Exists(path))
            return NotFound(ApiError.Validation($"Пример не найден: {name}"));

        try
        {
            var program = YawaLoader.FromFile(path);
            var opts = new InterpreterOptions
            {
                MaxSteps = Math.Min(program.Limits.MaxSteps, HardMaxSteps),
                MaxDepth = Math.Min(program.Limits.MaxDepth, HardMaxDepth),
                MaxSeconds = Math.Min(program.Limits.MaxSeconds, HardMaxSeconds),
                SnapshotEvery = program.Limits.SnapshotEvery
            };
            var session = new Interpreter(program, opts).Run();
            return Ok(session);
        }
        catch (JsonException jx)
        {
            return BadRequest(ApiError.Validation("Ошибка в YAWA-файле", jx.Message));
        }
        catch (YawaRuntimeException rex)
        {
            return Ok(new { error = rex.Message, kind = "runtime" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiError.Internal(ex.Message));
        }
    }

    /// <summary>
    /// Ищет папку samples/ вверх по дереву от текущего каталога и ContentRoot.
    /// </summary>
    private static string? FindSamplesRoot()
    {
        var candidates = new List<string>
    {
        Directory.GetCurrentDirectory(),
        AppContext.BaseDirectory
    };

        foreach (var start in candidates)
        {
            var dir = new DirectoryInfo(start);
            for (int i = 0; i < 6 && dir is not null; i++)
            {
                var candidate = Path.Combine(dir.FullName, "samples");
                if (Directory.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
        }
        return null;
    }

    [HttpGet("samples/{name}")]
    public IActionResult GetSample(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains("..") || name.Contains('/'))
            return BadRequest(ApiError.Validation("Некорректное имя примера"));

        var root = FindSamplesRoot();
        if (root is null)
            return StatusCode(500, ApiError.Internal("Папка samples/ не найдена"));

        var path = Path.Combine(root, name);
        if (!System.IO.File.Exists(path))
            return NotFound(ApiError.Validation($"Пример не найден: {name}"));

        var json = System.IO.File.ReadAllText(path);
        return Content(json, "application/json", System.Text.Encoding.UTF8);
    }

    [HttpGet("samples")]
    public IActionResult ListSamples()
    {
        var root = FindSamplesRoot();
        if (root is null) return Ok(Array.Empty<object>());

        var files = Directory.GetFiles(root, "*.yawa.json")
            .Select(p =>
            {
                var info = new FileInfo(p);
                return new
                {
                    name = info.Name,
                    size = info.Length,
                    modified = info.LastWriteTimeUtc
                };
            })
            .OrderBy(x => x.name)
            .ToList();

        return Ok(files);
    }

    /// <summary>
    /// Принимает Python-код в теле (text/plain или application/json с полем "code"),
    /// транспайлит в YAWA, исполняет и возвращает trace.
    /// </summary>
    [HttpPost("run-python")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [EnableRateLimiting("run")]
    public async Task<IActionResult> RunPython(CancellationToken ct)
    {
        // Читаем тело как текст
        string body;
        try
        {
            using var reader = new StreamReader(Request.Body);
            body = await reader.ReadToEndAsync(ct);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiError.Validation("Не удалось прочитать тело", ex.Message));
        }

        if (string.IsNullOrWhiteSpace(body))
            return BadRequest(ApiError.Validation("Пустое тело запроса"));

        // Если Content-Type: application/json — ожидаем { "code": "...", "entry": "..." }
        string pythonCode;
        string? entry = null;
        var ctHeader = Request.ContentType ?? "";
        if (ctHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("code", out var codeEl) ||
                    codeEl.ValueKind != JsonValueKind.String)
                    return BadRequest(ApiError.Validation("JSON должен содержать поле 'code'"));
                pythonCode = codeEl.GetString() ?? "";
                if (doc.RootElement.TryGetProperty("entry", out var entryEl) &&
                    entryEl.ValueKind == JsonValueKind.String)
                    entry = entryEl.GetString();
            }
            catch (JsonException jx)
            {
                return BadRequest(ApiError.Validation("Некорректный JSON", jx.Message));
            }
        }
        else
        {
            pythonCode = body;
        }

        // Транспайлим
        YawaProgram program;
        try
        {
            var transpiler = new Transpiler.PythonToYawa(pythonCode);
            program = transpiler.Transpile(entry);
        }
        catch (Transpiler.UnsupportedFeatureException ex)
        {
            return Ok(new
            {
                error = ex.Message,
                kind = "transpiler",
                line = ex.Line,
                column = ex.Column
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Transpiler unexpected error");
            return Ok(new { error = ex.Message, kind = "transpiler", detail = ex.GetType().Name });
        }

        // Исполняем
        var opts = new InterpreterOptions
        {
            MaxSteps = Math.Min(program.Limits.MaxSteps, HardMaxSteps),
            MaxDepth = Math.Min(program.Limits.MaxDepth, HardMaxDepth),
            MaxSeconds = Math.Min(program.Limits.MaxSeconds, HardMaxSeconds),
            SnapshotEvery = program.Limits.SnapshotEvery
        };

        TraceSession session;
        try
        {
            session = new Interpreter(program, opts).Run();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Interpreter error");
            return Ok(new { error = ex.Message, kind = "runtime" });
        }

        _log.LogInformation(
            "YAWA run-python: name={Name}, steps={Steps}, cmp={Cmp}, swaps={Swaps}",
            program.Metadata.Name, session.Statistics.TotalSteps,
            session.Statistics.Comparisons, session.Statistics.Swaps);

        return Ok(session);
    }


    [HttpGet("viewer")]
    [Produces("text/html")]
    public IActionResult Viewer() => Content(YawaViewerHtml.Value, "text/html", System.Text.Encoding.UTF8);
}