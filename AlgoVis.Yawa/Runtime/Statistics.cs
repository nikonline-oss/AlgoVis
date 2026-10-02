namespace AlgoVis.Yawa.Runtime;

public sealed class Statistics
{
    public int Comparisons { get; set; }
    public int Swaps { get; set; }
    public int MemoryAccesses { get; set; }
    public Dictionary<string, long> UserCounters { get; } = new();

    public void Count(string name, int delta)
    {
        if (!UserCounters.TryGetValue(name, out var val)) val = 0;
        UserCounters[name] = val + delta;
    }

    public Statistics Clone()
    {
        var copy = new Statistics
        {
            Comparisons = Comparisons,
            Swaps = Swaps,
            MemoryAccesses = MemoryAccesses
        };
        foreach (var (k, v) in UserCounters) copy.UserCounters[k] = v;
        return copy;
    }
}