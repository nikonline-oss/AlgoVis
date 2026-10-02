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
        var program = JsonSerializer.Deserialize<YawaProgram>(json, Options)
            ?? throw new InvalidOperationException("YawaProgram deserialized to null");
        return program;
    }

    public static YawaProgram FromFile(string path)
    {
        var json = File.ReadAllText(path);
        return FromJson(json);
    }
}