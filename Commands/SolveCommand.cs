using System.Diagnostics.CodeAnalysis;
using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using AdventOfCode.NET.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;


[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli")]
internal sealed class SolveCommand(ISolverService solverService, IHttpService httpService, IProblemService problemService, IEnvironmentVariablesService envVariablesService) : Command<DateSettings>
{
    public override int Execute(CommandContext context, DateSettings settings) {
        var testCasesPassed = solverService.TrySolveProblemTests(settings.Year, settings.Day).GetAwaiter().GetResult();
        if (!testCasesPassed) {
            return 0;
        }

        var problem = FetchAndParseProblem(settings.Year, settings.Day);
        
        if (problem.Level == ProblemLevel.Finished) {
            AnsiConsole.MarkupLine(AoCMessages.WarningProblemAlreadySolved(settings.Year, settings.Day));
            return 0;
        }
        
        var result = solverService.GetSolutionResult(settings.Year, settings.Day, problem.Level, problem.Input);
        var responseDocument = httpService.SubmitSolutionAsync(settings.Year, settings.Day, problem.Level, result.ToString()!).GetAwaiter().GetResult();
        
        var response = httpService.ParseSubmissionResponse(responseDocument, out var correctAnswer);
        AnsiConsole.MarkupLine(response);
        
        if (!correctAnswer)
            return 0;
        
        AnsiConsole.MarkupLine(AoCMessages.InfoUpdatingProblemFiles(settings.Year, settings.Day));
        var updatedProblem = FetchAndParseProblem(settings.Year, settings.Day);
        problemService.UpdateProblemFiles(updatedProblem).GetAwaiter().GetResult();

        if (!envVariablesService.NoGit) {
            problemService.TryUpdateGitForProblem(settings.Year, settings.Day, updatedProblem.Level);
        }
        
        if (updatedProblem.Level == ProblemLevel.Finished) {
            // Ask user if they want to benchmark their solution?
        }
        
        return 0;
    }
    
    private Problem FetchAndParseProblem(int year, int day) {
        var problemNode = httpService.FetchProblemAsync(year, day).GetAwaiter().GetResult();
        var problemInput = httpService.FetchProblemInputAsync(year, day).GetAwaiter().GetResult();
        var problem = problemService.ParseProblem(year, day, problemNode, problemInput);
        
        return problem;
    }
}