using System.Text.Json.Serialization;

namespace AlgoVis.Yawa.Yawa.Statements;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "op")]
[JsonDerivedType(typeof(AssignStatement),   "assign")]
[JsonDerivedType(typeof(DeclareStatement),  "declare")]
[JsonDerivedType(typeof(IfStatement),       "if")]
[JsonDerivedType(typeof(WhileStatement),    "while")]
[JsonDerivedType(typeof(ForStatement),      "for")]
[JsonDerivedType(typeof(ForeachStatement),  "foreach")]
[JsonDerivedType(typeof(ReturnStatement),   "return")]
[JsonDerivedType(typeof(BreakStatement),    "break")]
[JsonDerivedType(typeof(ContinueStatement), "continue")]
[JsonDerivedType(typeof(ExprStatement),     "expr")]
[JsonDerivedType(typeof(SwapStatement),     "swap")]
[JsonDerivedType(typeof(CompareStatement),  "compare")]
[JsonDerivedType(typeof(MarkStatement),     "mark")]
[JsonDerivedType(typeof(UnmarkStatement),   "unmark")]
[JsonDerivedType(typeof(AnnotateStatement), "annotate")]
[JsonDerivedType(typeof(CountStatement),    "count")]
[JsonDerivedType(typeof(SnapshotStatement), "snapshot")]
[JsonDerivedType(typeof(MakeNodeStatement), "make_node")]
[JsonDerivedType(typeof(TupleAssignStatement), "tuple_assign")]
[JsonDerivedType(typeof(AssertStatement), "assert")]
[JsonDerivedType(typeof(TryStatement), "try")]
[JsonDerivedType(typeof(DeleteStatement),   "delete")]
public abstract class YawaStatement
{
    /// <summary>Необязательный ID узла — для отладки и трассировки.</summary>
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NodeId { get; set; }
}