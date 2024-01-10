namespace AdventOfCode.NET.Model;

public readonly struct ProblemTestCase(ProblemLevel level, string input, string output)
{
    public ProblemLevel Level { get; } = level;
    public string Input { get; } = input;
    public string Output { get; } = output;
}