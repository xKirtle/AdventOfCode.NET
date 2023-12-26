﻿using System.Text;
using AoC.NET.Model;
using Spectre.Console;

namespace AoC.NET.Services;

internal interface ISolverService
{
    Task CreateTest(int year, int day, string fileNameWithExtension, string input, string output);
    Task SolveProblem(int year, int day);
}

internal class SolverService : ISolverService
{
    public async Task CreateTest(int year, int day, string fileNameWithExtension, string input, string output) {
        var filePath = Path.Combine(GetProblemPath(year, day, includeTest: true), fileNameWithExtension);
        
        var sb = new StringBuilder()
            .AppendLine("Input:")
            .AppendLine(input.Trim())
            .AppendLine()
            .AppendLine("Expected Output:")
            .AppendLine(output.Trim());

        AnsiConsole.MarkupLine($"[green]Writing {filePath}[/]");
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
    public async Task SolveProblem(int year, int day) {
        var solutionType = FindSolutionType(year, day);
        var solution = (ISolver)Activator.CreateInstance(solutionType);
        
        var testDirectory = GetProblemPath(year, day, includeTest: true);

        foreach (var file in Directory.GetFiles(testDirectory, "*.aoc")) {
            var (input, output) = await ParseTestFile(year, day, Path.GetFileName(file));
            var result = solution.PartOne(input);
            
            if (result.ToString() != output)
                throw new InvalidOperationException($"Test failed for {file}. Expected {output}, got {result}.");
        }
        
        // Invoke solution.PartOne and solution.PartTwo here...
    }

    private async Task<(string input, string output)> ParseTestFile(int year, int day, string fileNameWithExtension) {
        var filePath = Path.Combine(GetProblemPath(year, day, includeTest: true), fileNameWithExtension);
        
        string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        string[] parts = fileContent.Split(new[] { "Input:", "Expected Output:" }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            throw new InvalidOperationException("Test file format is incorrect.");

        return (parts[0].Trim(), parts[1].Trim());
    }
    
    private Type FindSolutionType(int year, int day) {
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
        
        return foundType;
    }
    
    private string GetProblemPath(int year, int day, bool includeTest = false) {
        var folder = Path.Combine(year.ToString(), $"Day{day:00}");

        if (includeTest)
            folder = Path.Combine(folder, "test");
        
        return folder;
    }
}