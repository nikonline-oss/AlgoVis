using AlgoVis.Yawa.Trace;
using AlgoVis.Yawa.Yawa.Statements;

namespace AlgoVis.Yawa.Runtime;

public sealed class TraceRecorder
{
    private readonly TraceSession _session;
    private readonly InterpreterOptions _options;
    private readonly List<string> _currentHighlights = new();
    private string? _pendingAnnotation;
    private Frame? _currentFrame;

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

    public void SetFrame(Frame frame) => _currentFrame = frame;

    public void AddHighlight(string target) => _currentHighlights.Add(target);
    public void SetAnnotation(string text) => _pendingAnnotation = text;
    public void ClearHighlights() => _currentHighlights.Clear();

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

        // Сжатый снимок переменных — на каждом шаге.
        if (_currentFrame is not null)
            step.Vars = SnapshotVars(_currentFrame);

        _session.Steps.Add(step);
    }

    public void RecordSnapshot(Frame root, string? label = null)
    {
        var snapshot = FullSnapshot(root);
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
        if (_currentFrame is not null)
            step.Vars = SnapshotVars(_currentFrame);
        _session.Steps.Add(step);
    }

    public void Finalize(Frame root, RuntimeValue? returnValue)
    {
        _session.FinalState = FullSnapshot(root);
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

    // ───────── Helpers ─────────

    /// <summary>Сжатый снимок: скаляры полностью, массивы/объекты коротко.</summary>
    private Dictionary<string, object?> SnapshotVars(Frame frame)
    {
        var d = new Dictionary<string, object?>();
        foreach (var (k, v) in frame.AllVisible())
        {
            if (k.StartsWith("__") && k != "__return__") continue;
            if (k.EndsWith("_k") || k.EndsWith("_v")) continue;
            d[k] = ToCompact(v);
        }
        return d;
    }

    private object? ToCompact(RuntimeValue v)
    {
        switch (v)
        {
            case IntValue i: return i.Value;
            case FloatValue f: return f.Value;
            case StringValue s: return s.Value;
            case BoolValue b: return b.Value;
            case NullValue: return null;

            case ArrayValue a:
                if (a.Count <= 16)
                    return new Dictionary<string, object?>
                    {
                        ["__type"] = a.ElementType is null ? "array" : $"array<{a.ElementType}>",
                        ["items"] = a.Items.Select(x => x.ToJson()).ToList()
                    };
                return new Dictionary<string, object?>
                {
                    ["__type"] = a.ElementType is null ? "array" : $"array<{a.ElementType}>",
                    ["length"] = a.Count
                };

            case TupleValue tup:
                return new Dictionary<string, object?>
                {
                    ["__type"] = "tuple",
                    ["items"] = tup.Items.Select(x => x.ToJson()).ToList()
                };

            case SetValue s:
                if (s.Count <= 16)
                    return new Dictionary<string, object?>
                    {
                        ["__type"] = "set",
                        ["items"] = s.Items.Select(x => x.ToJson()).ToList()
                    };
                return new Dictionary<string, object?>
                {
                    ["__type"] = "set",
                    ["length"] = s.Count
                };

            case ObjectValue o:
                // Если объект похож на узел бинарного дерева — отдаём рекурсивно.
                if (o.HasField("value") && (o.HasField("left") || o.HasField("right")))
                    return ObjectAsTreeNode(o);
                return new Dictionary<string, object?>
                {
                    ["__type"] = o.ClassName ?? "object",
                    ["fields"] = o.Fields.Count
                };

            case TreeValue t:
                return new Dictionary<string, object?>
                {
                    ["__type"] = "tree",
                    ["size"] = CountTreeNodes(t.Root),
                    ["root"] = TreeNodeToJson(t.Root)
                };

            case TreeNodeValue tn:
                return new Dictionary<string, object?>
                {
                    ["__type"] = "tree_node",
                    ["value"] = tn.Value.ToJson()
                };

            case GraphValue g:
                return new Dictionary<string, object?>
                {
                    ["__type"] = "graph",
                    ["nodes"] = g.Nodes.Count,
                    ["edges"] = g.Edges.Count
                };

            default:
                return v.ToString();
        }
    }

    private Dictionary<string, object?> FullSnapshot(Frame frame)
    {
        var result = new Dictionary<string, object?>();
        foreach (var (k, v) in frame.AllVisible())
        {
            // Оставляем __return__, скрываем остальные __*
            if (k.StartsWith("__") && k != "__return__") continue;
            // Скрываем внутренние имена foreach_pair: it_k, it_v и подобные
            if (k.EndsWith("_k") || k.EndsWith("_v")) continue;
            result[k] = v.ToJson();
        }
        return result;
    }

    private static object? ObjectAsTreeNode(ObjectValue o)
    {
        var value = o.HasField("value") ? o.GetField("value") : NullValue.Instance;

        object? leftJson = null;
        if (o.HasField("left"))
        {
            var lv = o.GetField("left");
            if (lv is ObjectValue lo) leftJson = ObjectAsTreeNode(lo);
        }

        object? rightJson = null;
        if (o.HasField("right"))
        {
            var rv = o.GetField("right");
            if (rv is ObjectValue ro) rightJson = ObjectAsTreeNode(ro);
        }

        return new Dictionary<string, object?>
        {
            ["__type"] = "tree_node",
            ["value"] = value.ToJson(),
            ["left"] = leftJson,
            ["right"] = rightJson
        };
    }

    private Dictionary<string, int> CollectStructureSizes(Frame root)
    {
        var sizes = new Dictionary<string, int>();
        foreach (var (k, v) in root.AllVisible())
        {
            if (k.StartsWith("__") && k != "__return__") continue;
            if (v is ArrayValue arr) sizes[k] = arr.Count;
            else if (v is TreeValue tree) sizes[k] = CountTreeNodes(tree.Root);
            else if (v is GraphValue g) sizes[k] = g.Nodes.Count;
            else if (v is SetValue st) sizes[k] = st.Count;
        }
        return sizes;
    }
    private static int CountTreeNodes(TreeNodeValue? node)
    {
        if (node is null) return 0;
        return 1 + CountTreeNodes(node.Left) + CountTreeNodes(node.Right);
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
    private static object? TreeNodeToJson(TreeNodeValue? node)
    {
        if (node is null) return null;
        return new Dictionary<string, object?>
        {
            ["value"] = node.Value.ToJson(),
            ["left"] = TreeNodeToJson(node.Left),
            ["right"] = TreeNodeToJson(node.Right),
            ["color"] = node.HighlightColor
        };
    }
}