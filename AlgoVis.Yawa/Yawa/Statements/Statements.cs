using System.Text.Json.Serialization;
using AlgoVis.Yawa.Yawa.Expressions;

namespace AlgoVis.Yawa.Yawa.Statements;

public sealed class AssignStatement : YawaStatement
{
    [JsonPropertyName("target")] public YawaExpression Target { get; set; } = null!;
    [JsonPropertyName("value")] public YawaExpression Value { get; set; } = null!;
}

public sealed class DeclareStatement : YawaStatement
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("value")] public YawaExpression? Value { get; set; }
}

public sealed class IfStatement : YawaStatement
{
    [JsonPropertyName("cond")] public YawaExpression Cond { get; set; } = null!;
    [JsonPropertyName("then")] public List<YawaStatement> Then { get; set; } = new();
    [JsonPropertyName("else")] public List<YawaStatement>? Else { get; set; }
}

public sealed class WhileStatement : YawaStatement
{
    [JsonPropertyName("cond")] public YawaExpression Cond { get; set; } = null!;
    [JsonPropertyName("body")] public List<YawaStatement> Body { get; set; } = new();
}

public sealed class ForStatement : YawaStatement
{
    [JsonPropertyName("var")] public string Var { get; set; } = "";
    [JsonPropertyName("from")] public YawaExpression From { get; set; } = null!;
    [JsonPropertyName("to")] public YawaExpression To { get; set; } = null!;
    [JsonPropertyName("step")] public YawaExpression? Step { get; set; }
    [JsonPropertyName("body")] public List<YawaStatement> Body { get; set; } = new();
}

public sealed class ForeachStatement : YawaStatement
{
    [JsonPropertyName("var")] public string Var { get; set; } = "";
    [JsonPropertyName("in")] public YawaExpression In { get; set; } = null!;
    [JsonPropertyName("body")] public List<YawaStatement> Body { get; set; } = new();
}

public sealed class ReturnStatement : YawaStatement
{
    [JsonPropertyName("value")] public YawaExpression? Value { get; set; }
}

public sealed class BreakStatement : YawaStatement { }
public sealed class ContinueStatement : YawaStatement { }

public sealed class ExprStatement : YawaStatement
{
    [JsonPropertyName("value")] public YawaExpression Value { get; set; } = null!;
}

public sealed class SwapStatement : YawaStatement
{
    [JsonPropertyName("a")] public YawaExpression A { get; set; } = null!;
    [JsonPropertyName("b")] public YawaExpression B { get; set; } = null!;
}

public sealed class CompareStatement : YawaStatement
{
    [JsonPropertyName("a")] public YawaExpression A { get; set; } = null!;
    [JsonPropertyName("b")] public YawaExpression B { get; set; } = null!;
    [JsonPropertyName("result")] public string Result { get; set; } = "";
    [JsonPropertyName("label")] public string? Label { get; set; }
}

public sealed class MarkStatement : YawaStatement
{
    [JsonPropertyName("target")] public YawaExpression Target { get; set; } = null!;
    [JsonPropertyName("color")] public string Color { get; set; } = "yellow";
    [JsonPropertyName("label")] public string? Label { get; set; }
}

public sealed class UnmarkStatement : YawaStatement
{
    [JsonPropertyName("target")] public YawaExpression Target { get; set; } = null!;
}

public sealed class AnnotateStatement : YawaStatement
{
    [JsonPropertyName("text")] public string Text { get; set; } = "";
}

public sealed class CountStatement : YawaStatement
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("delta")] public int Delta { get; set; } = 1;
}

public sealed class SnapshotStatement : YawaStatement
{
    [JsonPropertyName("label")] public string? Label { get; set; }
}

public sealed class MakeNodeStatement : YawaStatement
{
    [JsonPropertyName("target")] public YawaExpression Target { get; set; } = null!;
    [JsonPropertyName("value")] public YawaExpression Value { get; set; } = null!;
    [JsonPropertyName("left")] public YawaExpression? Left { get; set; }
    [JsonPropertyName("right")] public YawaExpression? Right { get; set; }
}

public sealed class TupleAssignStatement : YawaStatement
{
    public List<YawaExpression> Targets { get; set; } = new();
    public List<YawaExpression> Values { get; set; } = new();

    /// <summary>Индекс target, в который собираются "лишние" элементы (для *rest). -1 = нет splat.</summary>
    public int SplatIndex { get; set; } = -1;
}

public sealed class AssertStatement : YawaStatement
{
    [JsonPropertyName("cond")] public YawaExpression Cond { get; set; } = null!;
    [JsonPropertyName("message")] public YawaExpression? Message { get; set; }
}

public sealed class TryStatement : YawaStatement
{
    [JsonPropertyName("body")] public List<YawaStatement> Body { get; set; } = new();
    [JsonPropertyName("handlers")] public List<ExceptHandler> Handlers { get; set; } = new();
    [JsonPropertyName("finally")] public List<YawaStatement>? Finally { get; set; }
}

public sealed class ExceptHandler
{
    /// <summary>Имя переменной для исключения (в Python `except X as e:`).</summary>
    [JsonPropertyName("var")] public string? VarName { get; set; }
    [JsonPropertyName("body")] public List<YawaStatement> Body { get; set; } = new();
}

public sealed class DeleteStatement : YawaStatement
{
    [JsonPropertyName("targets")]
    public List<YawaExpression> Targets { get; set; } = new();
}

public sealed class SwapRefStatement : YawaStatement
{
    [JsonPropertyName("a")] public YawaExpression A { get; set; } = null!;
    [JsonPropertyName("b")] public YawaExpression B { get; set; } = null!;
}

public sealed class ForeachPairStatement : YawaStatement
{
    [JsonPropertyName("key_var")] public string KeyVar { get; set; } = "";
    [JsonPropertyName("value_var")] public string ValueVar { get; set; } = "";
    [JsonPropertyName("in")] public YawaExpression In { get; set; } = null!;
    [JsonPropertyName("body")] public List<YawaStatement> Body { get; set; } = new();
}