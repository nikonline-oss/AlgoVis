using System.Reflection;
using System.Runtime.InteropServices;

namespace AlgoVis.Parser.Native;

/// <summary>
/// Резолвер нативных библиотек. Может вызываться несколько раз —
/// повторная регистрация игнорируется.
/// </summary>
internal static class LibLoader
{
    private static int _registered;

    public static void Register()
    {
        // Атомарно: только первый поток войдёт в блок.
        if (Interlocked.Exchange(ref _registered, 1) != 0)
            return;

        NativeLibrary.SetDllImportResolver(
            typeof(LibLoader).Assembly,
            Resolve);
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        var fileName = libraryName switch
        {
            "tree-sitter"        => "libtree-sitter.so",
            "tree-sitter-python" => "libtree-sitter-python.so",
            "tree-sitter-cpp"    => "libtree-sitter-cpp.so",
            _ => null
        };

        if (fileName is null)
            return IntPtr.Zero;

        // 1. Стандартные места (для Linux /usr/local/lib)
        if (NativeLibrary.TryLoad(fileName, out var handle))
            return handle;

        // 2. Явные пути-кандидаты
        var candidates = new[]
        {
            "/usr/local/lib/" + fileName,
            "/usr/lib/" + fileName,
            Path.Combine(AppContext.BaseDirectory, fileName),
            Path.Combine(AppContext.BaseDirectory, "native", "linux-x64", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "native", "linux-x64", fileName)
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out var h))
                return h;
        }

        return IntPtr.Zero;
    }
}