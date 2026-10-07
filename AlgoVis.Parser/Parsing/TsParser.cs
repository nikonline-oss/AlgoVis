using System.Runtime.InteropServices;
using System.Text;
using AlgoVis.Parser.Native;

namespace AlgoVis.Parser.Parsing;

/// <summary>
/// Обёртка над TSParser. Не потокобезопасна — создавайте на каждый вызов.
/// </summary>
public sealed class TsParser : IDisposable
{
    private IntPtr _parser;
    private IntPtr _language;
    private bool _disposed;

    static TsParser()
    {
        LibLoader.Register();
    }

    public TsParser()
    {
        _parser = TsNative.ts_parser_new();
        if (_parser == IntPtr.Zero)
            throw new InvalidOperationException("ts_parser_new failed");

        _language = PythonLanguage.tree_sitter_python();
        if (_language == IntPtr.Zero)
            throw new InvalidOperationException("tree_sitter_python returned null");

        if (!TsNative.ts_parser_set_language(_parser, _language))
            throw new InvalidOperationException("ts_parser_set_language failed");
    }

    /// <summary>
    /// Парсит строку и возвращает корневой узел. Освободить через Dispose у TsTree.
    /// </summary>
    public TsTree Parse(string source)
    {
        var bytes = Encoding.UTF8.GetBytes(source);
        IntPtr sourcePtr = Marshal.AllocHGlobal(bytes.Length + 1);
        try
        {
            Marshal.Copy(bytes, 0, sourcePtr, bytes.Length);
            Marshal.WriteByte(sourcePtr, bytes.Length, 0); // null-terminator на всякий случай

            var tree = TsNative.ts_parser_parse_string(
                _parser, IntPtr.Zero, sourcePtr, (uint)bytes.Length);
            if (tree == IntPtr.Zero)
                throw new InvalidOperationException("ts_parser_parse_string returned null");

            return new TsTree(tree, bytes.Length);
        }
        finally
        {
            Marshal.FreeHGlobal(sourcePtr);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_parser != IntPtr.Zero)
        {
            TsNative.ts_parser_delete(_parser);
            _parser = IntPtr.Zero;
        }
    }
}