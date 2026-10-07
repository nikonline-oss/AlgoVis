namespace AlgoVis.Parser.Model;

public readonly record struct CodePoint(int Line, int Column)
{
    public override string ToString() => $"{Line}:{Column}";
}

public readonly record struct CodeSpan(CodePoint Start, CodePoint End)
{
    public static CodeSpan Empty => new(new CodePoint(0, 0), new CodePoint(0, 0));
    public override string ToString() => $"{Start}..{End}";
}