using AlgoVis.Parser.Native;

namespace AlgoVis.Parser.Parsing;

/// <summary>
/// Managed-обёртка над TSNode. Безопасна для default-значений
/// (когда FirstOrDefault вернул "пустоту") — все методы возвращают пустые значения.
/// </summary>
public readonly struct TsNode
{
    internal readonly TSNode Native;

    internal TsNode(TSNode native) => Native = native;

    public bool IsValid => Native.Id != IntPtr.Zero;

    public string Type => IsValid ? MarshalPtrToString(TsNative.ts_node_type(Native)) : "";

    public bool IsNamed => IsValid && TsNative.ts_node_is_named(Native);

    public uint ChildCount => IsValid ? TsNative.ts_node_child_count(Native) : 0;
    public uint NamedChildCount => IsValid ? TsNative.ts_node_named_child_count(Native) : 0;

    public TsNode Child(uint index)
        => IsValid ? new(TsNative.ts_node_child(Native, index)) : default;

    public TsNode NamedChild(uint index)
        => IsValid ? new(TsNative.ts_node_named_child(Native, index)) : default;

    public IEnumerable<TsNode> Children()
    {
        if (!IsValid) yield break;
        var n = ChildCount;
        for (uint i = 0; i < n; i++) yield return Child(i);
    }

    public IEnumerable<TsNode> NamedChildren()
    {
        if (!IsValid) yield break;
        var n = NamedChildCount;
        for (uint i = 0; i < n; i++) yield return NamedChild(i);
    }

    public (uint Row, uint Col) StartPoint
    {
        get
        {
            if (!IsValid) return (0, 0);
            var p = TsNative.ts_node_start_point(Native);
            return (p.Row, p.Column);
        }
    }

    public (uint Row, uint Col) EndPoint
    {
        get
        {
            if (!IsValid) return (0, 0);
            var p = TsNative.ts_node_end_point(Native);
            return (p.Row, p.Column);
        }
    }

    public uint StartByte => IsValid ? TsNative.ts_node_start_byte(Native) : 0;
    public uint EndByte => IsValid ? TsNative.ts_node_end_byte(Native) : 0;

    public override string ToString()
        => IsValid ? $"{Type}[{StartPoint}..{EndPoint}]" : "<invalid>";

    private static string MarshalPtrToString(IntPtr ptr)
        => ptr == IntPtr.Zero ? "" : System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr) ?? "";
}