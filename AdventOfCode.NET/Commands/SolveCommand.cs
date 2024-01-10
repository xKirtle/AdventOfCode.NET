using System.Diagnostics.CodeAnalysis;
using AdventOfCode.NET.Services;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;


[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli")]
internal sealed class SolveCommand(ISolverService solverService, IHttpService httpService) : Command<DateSettings>
{
    public override int Execute(CommandContext context, DateSettings settings) {
        var testCasesPassed = solverService.TrySolveProblemTests(settings.Year, settings.Day).GetAwaiter().GetResult();
        
        if (testCasesPassed) {
            // Submission flow here
            // 1. Fetch problem
            // 2. Parse current level
            // if current level is not null...
            // 3. Solve problem and save result
            // 4. Submit solution
            // 5. Parse response for success/failure
        }
        
        return 0;
    }
}