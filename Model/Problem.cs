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

    public Problem(int year, int day) {
        Year = year;
        Day = day;
    }
    
    public void CreateOrUpdateReadme() {
        var file = Path.Combine(Environment.CurrentDirectory, $"{Year}Day{Day:00}", "README.md");
        AOCUtils.WriteFile(file, Description);
    }

    public async Task<Problem> DownloadProblem() {
        return await AOCHttpClient.DownloadProblem(Year, Day);
    } 
}