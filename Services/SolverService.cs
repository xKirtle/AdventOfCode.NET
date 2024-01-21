using System.Diagnostics;
using System.Text;
using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using Spectre.Console;

namespace AdventOfCode.NET.Services;

internal interface ISolverService
{
    Task<bool> TrySolveProblemTests(int year, int day);
    object GetSolutionResult(int year, int day, ProblemLevel level, string input);
}

internal class SolverService : ISolverService
{
    public async Task<bool> TrySolveProblemTests(int year, int day) {
        var problemTestPath = ProblemService.GetProblemDirectory(year, day, includeTest: true);

        if (!Directory.Exists(problemTestPath)) {
            AnsiConsole.MarkupLine(AoCMessages.WarningNoTestsDirectoryFound(year, day));
            return true;
        }
        
        var sw = Stopwatch.StartNew();
        foreach (var filePath in Directory.GetFiles(problemTestPath, "*.aoc")) {
            var testStartTime = sw.ElapsedMilliseconds;
            var testCase = await ParseTestCase(filePath);

            var result = GetSolutionResult(year, day, testCase.Level, testCase.Input);
            var testTotalTime = sw.ElapsedMilliseconds - testStartTime;
            
            var colorTag = testTotalTime switch {
                > 1000 => "red",
                > 500 => "yellow",
                _ => "green"
            };

            if (result.ToString() != testCase.Output) {
                AnsiConsole.MarkupLine(AoCMessages.ErrorTestCaseFailed(filePath, testCase.Output, result.ToString()!));
                return false;
            }

            AnsiConsole.MarkupLine(AoCMessages.SuccessTestCasePassed(filePath, testTotalTime, colorTag));
        }
        
        return true;
    }

    public object GetSolutionResult(int year, int day, ProblemLevel level, string input) {
        var solver = ISolver.GetSolverInstance(year, day);

        object result;
        try {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            result = level switch {
                ProblemLevel.PartOne => solver.PartOne(input),
                ProblemLevel.PartTwo => solver.PartTwo(input),
                _ => throw new UnreachableException($"Invalid problem level value: {level}")
            };
        }
        catch (Exception ex) {
            throw new AoCSolutionException(ex.Message, ex);
        }
        
        if (result == null) {
            throw new AoCException(AoCMessages.ErrorProblemSolutionIsNull(year, day, level));
        }
        
        return result;
    }

    private static async Task<ProblemTestCase> ParseTestCase(string filePath) {
        var fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        var testParts = fileContent.Split(["Part:", "Input:", "Output:"], StringSplitOptions.RemoveEmptyEntries);
        
        if (testParts.Length < 3)
            throw new AoCException(AoCMessages.ErrorInvalidTestCase(filePath));
        
        var (level, input, output) = (testParts[0].Trim(), testParts[1].Trim(), testParts[2].Trim());
        
         _ = (string.IsNullOrEmpty(level), string.IsNullOrEmpty(input), string.IsNullOrEmpty(output)) switch {
            (true, _, _) => throw new AoCException(AoCMessages.ErrorParsingTestCasePart(filePath)),
            (_, true, _) => throw new AoCException(AoCMessages.ErrorParsingTestCaseInput(filePath)),
            (_, _, true) => throw new AoCException(AoCMessages.ErrorParsingTestCaseOutput(filePath)),
            _ => string.Empty
        };
        
        var problemLevel = level switch {
            "one" => ProblemLevel.PartOne,
            "two" => ProblemLevel.PartTwo,
            _ => throw new AoCException(AoCMessages.ErrorParsingTestCasePart(filePath))
        };
        
        return new ProblemTestCase(problemLevel, input, output);
    }
}