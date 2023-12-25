using System.Text;
using Spectre.Console;

namespace AoC.NET.Services;

interface ITestRunnerService
{
    Task CreateTest(string input, string output, string filePath);
    Task<(string input, string output)> ParseTestFile(string filePath);
}

public class TestRunnerService : ITestRunnerService
{
    public async Task CreateTest(string input, string output, string filePath) {
        var sb = new StringBuilder()
            .AppendLine("Input:")
            .AppendLine(input.Trim())
            .AppendLine()
            .AppendLine("Expected Output:")
            .AppendLine(output.Trim());

        AnsiConsole.MarkupLine($"[green]Writing {filePath}[/]");
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
    
    public async Task<(string input, string output)> ParseTestFile(string filePath) {
        string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        string[] parts = fileContent.Split(new[] { "Input:", "Expected Output:" }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            throw new InvalidOperationException("Test file format is incorrect.");

        return (parts[0].Trim(), parts[1].Trim());
    }
}