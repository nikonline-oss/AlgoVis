using System.Runtime.InteropServices;

namespace AlgoVis.Parser.Native;

internal static class CppLanguage
{
    private const string Lib = "tree-sitter-cpp";

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_cpp();
}
