using System.Runtime.InteropServices;

namespace AlgoVis.Parser.Native;

internal static class PythonLanguage
{
    private const string Lib = "tree-sitter-python";

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_python();
}