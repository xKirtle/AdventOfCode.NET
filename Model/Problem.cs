using System.Diagnostics;

namespace AdventOfCode.NET.Model;

[DebuggerStepThrough]
internal class Problem
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int Day { get; private set; }
    public int Year { get; private set; }
    public string Input { get; private set; }
    public string[] Answers { get; private set; }
}