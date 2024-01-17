using System.Diagnostics.CodeAnalysis;
using AdventOfCode.NET.Model;
using AdventOfCode.NET.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;


[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli")]
internal sealed class SolveCommand(ISolverService solverService, IHttpService httpService, IProblemService problemService) : Command<DateSettings>
{
    public override int Execute(CommandContext context, DateSettings settings) {
        var testCasesPassed = solverService.TrySolveProblemTests(settings.Year, settings.Day).GetAwaiter().GetResult();
        if (!testCasesPassed) 
            return 0;
        
        var problemNode = httpService.FetchProblemAsync(settings.Year, settings.Day).GetAwaiter().GetResult();
        var problemInput = httpService.FetchProblemInputAsync(settings.Year, settings.Day).GetAwaiter().GetResult();
        var problem = problemService.ParseProblem(settings.Year, settings.Day, problemNode, problemInput);

        if (problem.Level == ProblemLevel.Finished) {
            AnsiConsole.MarkupLine(AoCMessages.WarningProblemAlreadySolved(settings.Year, settings.Day));
            return 0;
        }

        var result = solverService.GetSolutionResult(settings.Year, settings.Day, problem.Level, problem.Input);
        var responseDocument = httpService.SubmitSolutionAsync(settings.Year, settings.Day, problem.Level, result.ToString()!).GetAwaiter().GetResult();
        
        var response = httpService.ParseSubmissionResponse(responseDocument);
        AnsiConsole.MarkupLine(response);

        return 0;
    }
}