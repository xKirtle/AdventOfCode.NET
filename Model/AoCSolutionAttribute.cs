namespace AoC.NET.Model;

[AttributeUsage(AttributeTargets.Class)]
public class AoCSolutionAttribute : Attribute
{
    public int Year { get; }
    public int Day { get; }

    public AoCSolutionAttribute(int year, int day) {
        Year = year;
        Day = day;
    }
}
