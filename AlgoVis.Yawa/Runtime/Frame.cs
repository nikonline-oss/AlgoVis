namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Область видимости: локальные переменные функции.
/// </summary>
public sealed class Frame
{
    public string FunctionName { get; }
    public Frame? Parent { get; }
    public Dictionary<string, RuntimeValue> Locals { get; } = new();

    /// <summary>Глубина стека вызовов (для лимита).</summary>
    public int Depth { get; }

    public Frame(string functionName, Frame? parent = null)
    {
        FunctionName = functionName;
        Parent = parent;
        Depth = (parent?.Depth ?? 0) + 1;
    }

    public RuntimeValue Get(string name)
    {
        if (Locals.TryGetValue(name, out var v)) return v;
        if (Parent is not null)
        {
            try { return Parent.Get(name); }
            catch (KeyNotFoundException) { /* fallback на self ниже */ }
        }

        // Fallback: если в текущем Locals есть self и это ObjectValue — ищем поле.
        // Нужно для C++-методов, где `value = value + 1` неявно значит `this->value`.
        if (Locals.TryGetValue("self", out var selfVal) && selfVal is ObjectValue obj)
        {
            if (obj.HasField(name)) return obj.GetField(name);
        }

        throw new KeyNotFoundException($"Variable '{name}' not found");
    }

    public bool Has(string name)
    {
        if (Locals.ContainsKey(name)) return true;
        return Parent?.Has(name) ?? false;
    }

    public void Set(string name, RuntimeValue value)
    {
        // 1. Если self есть в Locals и у него есть поле name → пишем в поле.
        if (Locals.TryGetValue("self", out var selfVal) && selfVal is ObjectValue obj)
        {
            if (obj.HasField(name))
            {
                obj.SetField(name, value);
                return;
            }
        }

        // 2. Иначе — обычная запись в Locals (Python-семантика).
        Locals[name] = value;
    }

    public void Declare(string name, RuntimeValue value) => Locals[name] = value;

    /// <summary>Все видимые переменные: сначала локальные, потом родительские.</summary>
    public Dictionary<string, RuntimeValue> AllVisible()
    {
        var dict = new Dictionary<string, RuntimeValue>();
        if (Parent is not null)
            foreach (var (k, v) in Parent.AllVisible())
                dict[k] = v;
        foreach (var (k, v) in Locals)
            dict[k] = v;
        return dict;
    }
}