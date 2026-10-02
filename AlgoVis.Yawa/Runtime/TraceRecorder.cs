using AlgoVis.Yawa.Trace;
using AlgoVis.Yawa.Yawa.Statements;

namespace AlgoVis.Yawa.Runtime;

/// <summary>
/// Записывает шаги исполнения в TraceSession.
/// </summary>
public sealed class TraceRecorder
{
    private readonly TraceSession _session;
    private readonly InterpreterOptions _options;
    private readonly List<string> _currentHighlights = new();
    private string? _pendingAnnotation;

    public Statistics Stats { get; } = new();

    public TraceRecorder(string yawaVersion, Dictionary<string, object?> metadata, InterpreterOptions options)
    {
        _options = options;
        _session = new TraceSession
        {
            YawaVersion = yawaVersion,
            Metadata = metadata
        };
    }

    public TraceSession Session => _session;

    public void AddHighlight(string target) => _currentHighlights.Add(target);

    public void SetAnnotation(string text) => _pendingAnnotation = text;

    public void ClearHighlights() => _currentHighlights.Clear();

    /// <summary>
    /// Записать шаг. target/old/new — для diff; kind — "assign", "compare", "swap" и т.д.
    /// </summary>
    public void Record(
        string kind,
        string? nodeId,
        IEnumerable<StateChange>? diff = null,
        bool consumeHighlights = true,
        bool consumeAnnotation = true)
    {
        var step = new TraceStep
        {
            N = _session.Steps.Count,
            Kind = kind,
            NodeId = nodeId,
            Stats = new TraceStepStats
            {
                Comparisons = Stats.Comparisons,
                Swaps = Stats.Swaps,
                MemoryAccesses = Stats.MemoryAccesses,
                StepsTotal = _session.Steps.Count + 1,
                UserCounters = new Dictionary<string, long>(Stats.UserCounters)
            }
        };

        if (diff is not null)
            step.Diff.AddRange(diff);

        if (consumeHighlights)
        {
            step.Highlight.AddRange(_currentHighlights);
            _currentHighlights.Clear();
        }

        if (consumeAnnotation)
        {
            step.Annotation = _pendingAnnotation;
            _pendingAnnotation = null;
        }

        _session.Steps.Add(step);
    }

    /// <summary>Записать полный snapshot состояния.</summary>
    public void RecordSnapshot(Frame root, string? label = null)
    {
        var snapshot = SnapshotState(root);
        var step = new TraceStep
        {
            N = _session.Steps.Count,
            Kind = "snapshot",
            NodeId = null,
            Annotation = label,
            Snapshot = snapshot,
            Stats = new TraceStepStats
            {
                Comparisons = Stats.Comparisons,
                Swaps = Stats.Swaps,
                MemoryAccesses = Stats.MemoryAccesses,
                StepsTotal = _session.Steps.Count + 1,
                UserCounters = new Dictionary<string, long>(Stats.UserCounters)
            }
        };
        _session.Steps.Add(step);
    }

    public void Finalize(Frame root, RuntimeValue? returnValue)
    {
        _session.FinalState = SnapshotState(root);
        _session.Statistics = new TraceStatistics
        {
            TotalSteps = _session.Steps.Count,
            Comparisons = Stats.Comparisons,
            Swaps = Stats.Swaps,
            MemoryAccesses = Stats.MemoryAccesses,
            UserCounters = new Dictionary<string, long>(Stats.UserCounters),
            StructureSizes = CollectStructureSizes(root)
        };
    }

    private Dictionary<string, int> CollectStructureSizes(Frame root)
    {
        var sizes = new Dictionary<string, int>();
        foreach (var (k, v) in root.AllVisible())
        {
            if (k.StartsWith("__")) continue;
            if (v is ArrayValue arr) sizes[k] = arr.Count;
            else if (v is TreeValue tree) sizes[k] = CountTreeNodes(tree.Root);
            else if (v is GraphValue g) sizes[k] = g.Nodes.Count;
        }
        return sizes;
    }

    private static int CountTreeNodes(TreeNodeValue? node)
    {
        if (node is null) return 0;
        return 1 + CountTreeNodes(node.Left) + CountTreeNodes(node.Right);
    }

    private Dictionary<string, object?> SnapshotState(Frame frame)
    {
        var result = new Dictionary<string, object?>();
        foreach (var (k, v) in frame.AllVisible())
            result[k] = v.ToJson();
        return result;
    }

    public static void SetStructureDescriptor(TraceSession session, Frame root)
    {
        foreach (var (k, v) in root.AllVisible())
        {
            if (k.StartsWith("__")) continue;
            session.Structure.Variables.Add(new VariableDescriptor
            {
                Name = k,
                Type = v.TypeName,
                VisualKind = v switch
                {
                    ArrayValue => "array",
                    TreeValue => "tree",
                    GraphValue => "graph",
                    ObjectValue => "object",
                    _ => "scalar"
                }
            });
        }
    }
}