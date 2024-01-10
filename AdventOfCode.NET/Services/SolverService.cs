using System.Diagnostics;
using System.Text;
using AdventOfCode.NET.Model;
using Spectre.Console;

namespace AdventOfCode.NET.Services;

internal interface ISolverService
{
    Task<bool> TrySolveProblemTests(int year, int day);
}

internal class SolverService : ISolverService
{
    public async Task<bool> TrySolveProblemTests(int year, int day)
    {
        var solver = ISolver.GetSolverInstance(year, day);
        var problemTestPath = ProblemService.GetProblemDirectory(year, day, includeTest: true);

        if (!Directory.Exists(problemTestPath)) {
            AnsiConsole.MarkupLine(AoCMessages.WarningNoTestsDirectoryFound(year, day));
            return true;
        }
        
        var sw = Stopwatch.StartNew();
        foreach (var filePath in Directory.GetFiles(problemTestPath, "*.aoc")) {
            var testStartTime = sw.ElapsedMilliseconds;
            var testCase = await ParseTestCase(filePath);
            
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            var result = testCase.Level switch {
                ProblemLevel.PartOne => solver.PartOne(testCase.Input),
                ProblemLevel.PartTwo => solver.PartTwo(testCase.Input),
                _ => throw new UnreachableException($"Invalid problem level value: {testCase.Level}")
            };

            if (result == null) {
                throw new AoCException(AoCMessages.ErrorProblemSolutionIsNull(filePath));
            }

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

    private static async Task<ProblemTestCase> ParseTestCase(string filePath)
    {
        var fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        var testParts = fileContent.Split(["Part:", "Input:", "Output:"], StringSplitOptions.RemoveEmptyEntries);

        if (testParts.Length < 3)
            throw new AoCException(AoCMessages.ErrorInvalidTestCase(filePath));
        
        var (level, input, output) = (testParts[0].Trim(), testParts[1].Trim(), testParts[2].Trim());

        var testFields = (string.IsNullOrEmpty(level), string.IsNullOrEmpty(input), string.IsNullOrEmpty(output));

         _ = testFields switch {
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