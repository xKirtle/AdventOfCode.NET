using System.Diagnostics;
using System.Text;
using AoC.NET.Model;
using Spectre.Console;

namespace AoC.NET.Services;

internal interface ISolverService
{
    Task CreateTestTemplate(int year, int day);
    Task SolveProblemTests(int year, int day);
    Task SolveProblem(int year, int day);
}

internal class SolverService : ISolverService
{
    private readonly IHttpService _httpService;
    
    public SolverService(IHttpService httpService) {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }
    
    public async Task CreateTestTemplate(int year, int day) {
        var filePath = Path.Combine(GetProblemPath(year, day, includeTest: true), $"test0.conf");
        
        var sb = new StringBuilder()
            .AppendLine($"Part: [one/two]")
            .AppendLine("Input:")
            .AppendLine("# Your test input goes here")
            .AppendLine("# and also here, if multiline")
            .AppendLine("Output:")
            .AppendLine("# Your expected output goes here");

        AnsiConsole.MarkupLine($"[green]Writing {filePath}[/]");
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
    
    public async Task SolveProblemTests(int year, int day) {
        var solution = GetSolutionInstance(year, day);
        var testDirectory = GetProblemPath(year, day, includeTest: true);

        var sw = Stopwatch.StartNew();
        foreach (var file in Directory.GetFiles(testDirectory, "*.conf")) {
            var testStartTime = sw.ElapsedMilliseconds;
            
            var (problemPart, input, output) = await ParseTestFile(year, day, Path.GetFileName(file));
            
            var result = problemPart switch {
                "one" => solution.PartOne(input),
                "two" => solution.PartTwo(input),
                _ => throw new InvalidOperationException($"Invalid problem part in {Path.GetFileName(file)}. Expected 'one' or 'two' but got '{problemPart}'")
            };
            
            if (result.ToString() != output)
                throw new InvalidOperationException($"Test failed for {Path.GetFileName(file)}. Expected {output}, got {result}.");

            var testTotalTime = sw.ElapsedMilliseconds - testStartTime;
            var colorTag = testTotalTime > 1000 ? "red" : testTotalTime > 500 ? "yellow" : "green";
            
            AnsiConsole.MarkupLine($"[green]Test passed for {Path.GetFileName(file)}[/] in [{colorTag}]{testTotalTime}ms[/]");
        }
    }
    
    public async Task SolveProblem(int year, int day) {
        var solution = GetSolutionInstance(year, day);
        var input = await File.ReadAllTextAsync(Path.Combine(GetProblemPath(year, day), "input.aoc"), Encoding.UTF8);
        var level = await _httpService.FetchProblemLevel(year, day);
        var solutionResult = level switch {
            "1" => solution.PartOne(input),
            "2" => solution.PartTwo(input),
            null => null,
            // null => throw new NotImplementedException("Problem was already solved in AoC."), // TODO: Decide what to do with solved problems
            _ => throw new InvalidOperationException($"Invalid problem part: {level}.")
        };
        
        await _httpService.SubmitSolution(year, day, level, solutionResult?.ToString() ?? "");
    }

    private async Task<(string problemPart, string input, string output)> ParseTestFile(int year, int day, string fileNameWithExtension) {
        var filePath = Path.Combine(GetProblemPath(year, day, includeTest: true), fileNameWithExtension);
        
        string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        string[] parts = fileContent.Split(new[] { "Part:", "Input:", "Output:" }, StringSplitOptions.RemoveEmptyEntries);

        var problemPart = parts[0].Trim();
        var input = parts[1].Trim();
        var output = parts[2].Trim();
        
        if (parts.Length != 3 || string.IsNullOrWhiteSpace(problemPart) || string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(output))
            throw new InvalidOperationException("Test file format is incorrect or input/output is missing.");

        return (problemPart, input, output);
    }
    
    
    private ISolver GetSolutionInstance(int year, int day) {
        Type foundType = null;
        
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            foreach (var type in assembly.GetTypes()) {
                // Ignore classes that don't implement ISolver
                if (!typeof(ISolver).IsAssignableFrom(type))
                    continue;
                
                var attributes = type.GetCustomAttributes(typeof(AoCSolutionAttribute), false);
                if (attributes.FirstOrDefault() is AoCSolutionAttribute attribute && attribute.Year == year && attribute.Day == day) {
                    if (foundType != null)
                        throw new InvalidOperationException($"Multiple solution classes found for Year {year}, Day {day}. Ensure only one class is marked with AoCSolutionAttribute for each day.");
                    
                    foundType = type;
                }
            }
        }
        
        if (foundType == null)
            throw new InvalidOperationException($"No solution class found for Year {year}, Day {day}.");
        
        return (ISolver)Activator.CreateInstance(foundType);
    }
    
    private string GetProblemPath(int year, int day, bool includeTest = false) {
        var folder = Path.Combine(year.ToString(), $"Day{day:00}");

        if (includeTest)
            folder = Path.Combine(folder, "test");
        
        return folder;
    }
}