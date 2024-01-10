namespace AdventOfCode.NET.Model;

public readonly struct ProblemAnswers(string? partOne, string? partTwo)
{
    public string? PartOne { get; } = partOne;
    public string? PartTwo { get; } = partTwo;

    public string[] ToArray() {
        var answers = new List<string>();

        if (PartOne == null)
            return [];
        
        answers.Add(PartOne);
        
        if (PartTwo != null)
            answers.Add(PartTwo);
        
        return answers.ToArray();
    }
}