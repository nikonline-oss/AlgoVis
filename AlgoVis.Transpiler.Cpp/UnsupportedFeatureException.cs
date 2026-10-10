namespace AlgoVis.Transpiler.Cpp;

public sealed class UnsupportedFeatureException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public UnsupportedFeatureException(string message, int line = 0, int column = 0)
        : base(line > 0 ? $"Строка {line + 1}:{column}: {message}" : message)
    {
        Line = line;
        Column = column;
    }
}
