namespace AoC.NET.Model;

internal class Problem
{
    public string Title { get; set; } = string.Empty;
    public string ContentMd { get; set; } = string.Empty;
    public int Day { get; set; }
    public int Year { get; set; }
    public string Input { get; set; } = string.Empty;
    public List<(string, string)> Examples { get; set; } = new List<(string, string)>();
}