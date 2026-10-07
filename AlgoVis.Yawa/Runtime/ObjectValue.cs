namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Анонимный объект (Python-подобный). Поля — словарь.
/// Специальное поле "__type__" — для рендера, интерпретатором игнорируется.
/// </summary>
public sealed class ObjectValue : RuntimeValue
{
    public Dictionary<string, RuntimeValue> Fields { get; } = new();

    public string? ClassName =>
        Fields.TryGetValue("__type__", out var v) && v is StringValue s
            ? s.Value
            : null;

    public override string TypeName =>
        ClassName is null ? "object" : $"object<{ClassName}>";

    public override object? ToJson()
    {
        var dict = new Dictionary<string, object?>();
        foreach (var (k, v) in Fields)
            dict[k] = v.ToJson();
        return dict;
    }

    public override string ToString()
    {
        var parts = Fields
            .Where(kv => kv.Key != "__type__")
            .Select(kv => $"{kv.Key}={kv.Value}");
        var name = ClassName ?? "object";
        return $"{name}{{ {string.Join(", ", parts)} }}";
    }

    public override bool ValueEquals(RuntimeValue other) => ReferenceEquals(this, other);

    public RuntimeValue GetField(string name) =>
        Fields.TryGetValue(name, out var v)
            ? v
            : throw new KeyNotFoundException($"Field '{name}' not found in object");

    public bool HasField(string name) => Fields.ContainsKey(name);

    public void SetField(string name, RuntimeValue value) => Fields[name] = value;
}