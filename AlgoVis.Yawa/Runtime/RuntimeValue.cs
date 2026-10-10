using System.Text.Json;

namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Runtime-значение YAWA. Наследники — конкретные типы.
/// </summary>
public abstract class RuntimeValue
{
    public abstract string TypeName { get; }

    /// <summary>Для сериализации в JSON (в trace).</summary>
    public abstract object? ToJson();

    /// <summary>Для отображения в консоли / коротких логах.</summary>
    public abstract override string ToString();

    /// <summary>Сравнение значений для == и !=.</summary>
    public virtual bool ValueEquals(RuntimeValue other) => false;
}

public sealed class IntValue : RuntimeValue
{
    public long Value { get; }
    public IntValue(long v) => Value = v;
    public override string TypeName => "int";
    public override object ToJson() => Value;
    public override string ToString() => Value.ToString();
    public override bool ValueEquals(RuntimeValue other) =>
        other is IntValue i && i.Value == Value;
}

public sealed class FloatValue : RuntimeValue
{
    public double Value { get; }
    public FloatValue(double v) => Value = v;
    public override string TypeName => "float";
    public override object ToJson() => Value;
    public override string ToString() => Value.ToString("0.0###");
    public override bool ValueEquals(RuntimeValue other) =>
        other is FloatValue f && Math.Abs(f.Value - Value) < 1e-12;
}

public sealed class StringValue : RuntimeValue
{
    public string Value { get; }
    public StringValue(string v) => Value = v;
    public override string TypeName => "string";
    public override object ToJson() => Value;
    public override string ToString() => $"\"{Value}\"";
    public override bool ValueEquals(RuntimeValue other) =>
        other is StringValue s && s.Value == Value;
}

public sealed class BoolValue : RuntimeValue
{
    public bool Value { get; }
    public BoolValue(bool v) => Value = v;
    public override string TypeName => "bool";
    public override object ToJson() => Value;
    public override string ToString() => Value ? "true" : "false";
    public override bool ValueEquals(RuntimeValue other) =>
        other is BoolValue b && b.Value == Value;

    public static readonly BoolValue True  = new(true);
    public static readonly BoolValue False = new(false);
}

public sealed class NullValue : RuntimeValue
{
    public static readonly NullValue Instance = new();
    private NullValue() { }
    public override string TypeName => "null";
    public override object? ToJson() => null;
    public override string ToString() => "null";
    public override bool ValueEquals(RuntimeValue other) => other is NullValue;
}