using System.Runtime.InteropServices;

namespace AlgoVis.Parser.Native;

/// <summary>
/// Помогает .NET находить нативные библиотеки по явному пути.
/// </summary>
internal static class LibLoader
{
    public static void Register()
    {
        NativeLibrary.SetDllImportResolver(typeof(LibLoader).Assembly, Resolve);
    }

    private static IntPtr Resolve(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
    {
        var candidate = libraryName switch
        {
            "tree-sitter"        => "/usr/local/lib/libtree-sitter.so",
            "tree-sitter-python" => "/usr/local/lib/libtree-sitter-python.so",
            _ => null
        };

        if (candidate is not null && File.Exists(candidate))
            return NativeLibrary.Load(candidate);

        return IntPtr.Zero;
    }
}