using System.Runtime.InteropServices;

namespace AlgoVis.Parser.Native;

internal static class TsNative
{
    private const string Lib = "tree-sitter";

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_parser_new();

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_parser_delete(IntPtr parser);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ts_parser_set_language(IntPtr parser, IntPtr language);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_parser_parse_string(
        IntPtr parser, IntPtr oldTree, IntPtr source, uint length);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_tree_delete(IntPtr tree);

    // Возвращает TSNode ПО ЗНАЧЕНИЮ — маршалим как структуру.
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_tree_root_node(IntPtr tree);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_node_type(TSNode node);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_node_child_count(TSNode node);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_node_child(TSNode node, uint index);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_node_named_child_count(TSNode node);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_node_named_child(TSNode node, uint index);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ts_node_is_named(TSNode node);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern TsPoint ts_node_start_point(TSNode node);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern TsPoint ts_node_end_point(TSNode node);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_node_start_byte(TSNode node);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_node_end_byte(TSNode node);
}

[StructLayout(LayoutKind.Sequential)]
internal struct TsPoint
{
    public uint Row;
    public uint Column;
}

/// <summary>
/// Соответствует C-структуре TSNode:
///   uint32_t context[4];  → 16 байт
///   const void *id;       → 8 байт
///   const TSTree *tree;   → 8 байт
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 32)]
internal struct TSNode
{
    public ulong Context0;
    public ulong Context1;
    public IntPtr Id;
    public IntPtr Tree;
}