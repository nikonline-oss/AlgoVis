using AlgoVis.Parser.Native;

namespace AlgoVis.Parser.Parsing;

public sealed class TsTree : IDisposable
{
    private IntPtr _tree;
    private bool _disposed;

    public int SourceByteLength { get; }

    internal TsTree(IntPtr tree, int sourceByteLength)
    {
        _tree = tree;
        SourceByteLength = sourceByteLength;
    }

    public TsNode Root
    {
        get
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TsTree));
            return new TsNode(TsNative.ts_tree_root_node(_tree));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_tree != IntPtr.Zero)
        {
            TsNative.ts_tree_delete(_tree);
            _tree = IntPtr.Zero;
        }
    }
}