using System.Text;
using AoC.NET.Model;
using Spectre.Console;

namespace AoC.NET.Services;

internal interface ISolverService
{
    Task CreateTest(int year, int day, int exampleNumber, string input, string output);
    Task SolveProblemTests(int year, int day);
    Task SolveProblem(int year, int day);
}

internal class SolverService : ISolverService
{
    private readonly IHttpService _httpService;
    
    public SolverService(IHttpService httpService) {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }
    
    public async Task CreateTest(int year, int day, int exampleNumber, string input, string output) {
        var filePath = Path.Combine(GetProblemPath(year, day, includeTest: true), $"test{exampleNumber}.aoc");
        
        var sb = new StringBuilder()
            .AppendLine($"Part: {exampleNumber + 1}")
            .AppendLine()
            .AppendLine("Input:")
            .AppendLine(input.Trim())
            .AppendLine()
            .AppendLine("Expected Output:")
            .AppendLine(output.Trim());

        AnsiConsole.MarkupLine($"[green]Writing {filePath}[/]");
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
    
    public async Task SolveProblemTests(int year, int day) {
        var solution = GetSolutionInstance(year, day);
        var testDirectory = GetProblemPath(year, day, includeTest: true);

        foreach (var file in Directory.GetFiles(testDirectory, "*.aoc")) {
            var (problemPart, input, output) = await ParseTestFile(year, day, Path.GetFileName(file));

            if (problemPart != 1 && problemPart != 2)
                throw new InvalidOperationException($"Invalid problem part {problemPart}.");
            
            var result = problemPart == 1 ? solution.PartOne(input) : solution.PartTwo(input);
            
            // TODO: Handle wrong answers without throwing an exception... Properly format the output?
            if (result.ToString() != output)
                throw new InvalidOperationException($"Test failed for {Path.GetFileName(file)}. Expected {output}, got {result}.");
            
            AnsiConsole.MarkupLine($"[green]Test passed for {Path.GetFileName(file)}[/]");
        }
    }
    
    public async Task SolveProblem(int year, int day) {
        var solution = GetSolutionInstance(year, day);
        var input = await File.ReadAllTextAsync(Path.Combine(GetProblemPath(year, day), "input.aoc"), Encoding.UTF8);
        var level = await _httpService.FetchProblemLevel(year, day);
        var solutionResult = level == "1" ? solution.PartOne(input) : solution.PartTwo(input);
        
        await _httpService.SubmitSolution(year, day, level, solutionResult.ToString());
    }

    private async Task<(int problemPart, string input, string output)> ParseTestFile(int year, int day, string fileNameWithExtension) {
        var filePath = Path.Combine(GetProblemPath(year, day, includeTest: true), fileNameWithExtension);
        
        string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        string[] parts = fileContent.Split(new[] { "Part:", "Input:", "Expected Output:" }, StringSplitOptions.RemoveEmptyEntries);

        var problemPart = parts[0].Trim();
        var input = parts[1].Trim();
        var output = parts[2].Trim();
        
        if (parts.Length != 3 || !int.TryParse(problemPart, out int problemPartNumber) || string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(output))
            throw new InvalidOperationException("Test file format is incorrect or input/output is missing.");

        return (problemPartNumber, input, output);
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