using System.Text.Json;
using AlgoVis.Yawa.Yawa;

namespace AlgoVis.Yawa.Yawa.Loader;

public static class YawaLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static YawaProgram FromJson(string json)
    {
        YawaProgram? program;
        try
        {
            program = JsonSerializer.Deserialize<YawaProgram>(json, Options);
        }
        catch (JsonException)
        {
            throw;
        }

        if (program is null)
            throw new JsonException("YawaProgram deserialized to null");

        Validate(program);
        return program;
    }

    public static YawaProgram FromFile(string path)
    {
        var json = File.ReadAllText(path);
        return FromJson(json);
    }

    /// <summary>
    /// Строгая валидация YAWA-программы после десериализации.
    /// Все ошибки — JsonException с понятным текстом.
    /// </summary>
    private static void Validate(YawaProgram program)
    {
        if (program.YawaVersion != "1.0")
            throw new JsonException($"Unsupported yawa_version: '{program.YawaVersion}' (expected '1.0')");

        if (program.Functions.Count == 0)
            throw new JsonException("YAWA program has no functions");

        // Дубликаты имён функций
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var f in program.Functions)
        {
            if (string.IsNullOrWhiteSpace(f.Name))
                throw new JsonException("Function with empty name");
            if (!names.Add(f.Name))
                throw new JsonException($"Duplicate function name: '{f.Name}'");
        }

        if (program.Entry is null || string.IsNullOrWhiteSpace(program.Entry.Function))
            throw new JsonException("Entry.function is required and must be non-empty");

        if (!names.Contains(program.Entry.Function))
            throw new JsonException($"Entry function '{program.Entry.Function}' not found among declared functions");

        // Проверяем, что entry-args по количеству совпадают с параметрами функции
        var entryFn = program.Functions.First(f => f.Name == program.Entry.Function);
        if (program.Entry.Args.Count != entryFn.Params.Count)
            throw new JsonException(
                $"Entry function '{entryFn.Name}' expects {entryFn.Params.Count} args, " +
                $"but entry.args has {program.Entry.Args.Count}");
    }
}