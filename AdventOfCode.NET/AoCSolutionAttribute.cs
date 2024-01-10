namespace AdventOfCode.NET;

[AttributeUsage(AttributeTargets.Class)]
public class AoCSolutionAttribute(int year, int day) : Attribute
{
    public int Year { get; } = year;
    public int Day { get; } = day;
}