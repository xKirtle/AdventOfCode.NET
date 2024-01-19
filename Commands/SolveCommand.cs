﻿using System.Diagnostics.CodeAnalysis;
using AdventOfCode.NET.Exceptions;
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
        
        var problem = FetchAndParseProblem(settings.Year, settings.Day);

        if (problem.Level == ProblemLevel.Finished) {
            AnsiConsole.MarkupLine(AoCMessages.WarningProblemAlreadySolved(settings.Year, settings.Day));
            return 0;
        }

        // if an exception happens here, we want to return it to the user
        object result;
        try {
            result = solverService.GetSolutionResult(settings.Year, settings.Day, problem.Level, problem.Input);
        }
        catch (Exception ex) {
            throw new AoCSolutionException(ex.Message, ex);
        }
        
        var responseDocument = httpService.SubmitSolutionAsync(settings.Year, settings.Day, problem.Level, result.ToString()!).GetAwaiter().GetResult();
        
        var response = httpService.ParseSubmissionResponse(responseDocument);
        AnsiConsole.MarkupLine(response);

        return 0;
    }
    
    private Problem FetchAndParseProblem(int year, int day) {
        var problemNode = httpService.FetchProblemAsync(year, day).GetAwaiter().GetResult();
        var problemInput = httpService.FetchProblemInputAsync(year, day).GetAwaiter().GetResult();
        var problem = problemService.ParseProblem(year, day, problemNode, problemInput);
        
        return problem;
    }
}