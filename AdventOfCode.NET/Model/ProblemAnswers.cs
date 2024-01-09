namespace AdventOfCode.NET.Model;

public readonly struct ProblemAnswers(string? partOne, string? partTwo)
{
    public string? PartOne { get; init; } = partOne;
    public string? PartTwo { get; init; } = partTwo;
}