namespace AoC.NET.Model;

internal class Problem
{
    public string Title { get; set; }
    public string ContentMd { get; set; }
    public int Day { get; set; }
    public int Year { get; set; }
    public string Input { get; set; }
    public List<(string, string)> Examples { get; set; } = new List<(string, string)>();
}