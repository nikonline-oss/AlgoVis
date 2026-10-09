namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Ссылка на функцию как значение.
/// Хранит имя и (опционально) захваченный родительский фрейм —
/// для поддержки замыканий (nested functions, читающие переменные из родителя).
/// </summary>
public sealed class FunctionRefValue : RuntimeValue
{
    public string Name { get; }
    public Frame? CapturedFrame { get; }

    public FunctionRefValue(string name, Frame? capturedFrame = null)
    {
        Name = name;
        CapturedFrame = capturedFrame;
    }

    public override string TypeName => "function";
    public override object ToJson() => Name;
    public override string ToString() => $"<function {Name}>";

    public override bool ValueEquals(RuntimeValue other) =>
        other is FunctionRefValue f && f.Name == Name;
}