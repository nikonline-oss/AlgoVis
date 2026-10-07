namespace AlgoVis.Yawa.Runtime;

public sealed class InterpreterOptions
{
    public int MaxSteps { get; set; } = 100_000;
    public int MaxDepth { get; set; } = 1_000;
    public double MaxSeconds { get; set; } = 10.0;
    public int SnapshotEvery { get; set; } = 0;
    public int MaxArraySize { get; set; } = 10_000;
    public bool CollectSteps { get; set; } = true;
}