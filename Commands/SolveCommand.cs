using AoC.NET.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AoC.NET.Commands;

internal sealed class SolveCommand : Command<SolveCommand.Settings>
{
    private readonly ISolverService _solverService;
    
    public sealed class Settings : DateSettings { }

    public SolveCommand(ISolverService solverService) {
        _solverService = solverService ?? throw new ArgumentNullException(nameof(solverService));
    }

    public override int Execute(CommandContext context, Settings settings) {
        _solverService.SolveProblemTests(settings.Year, settings.Day).GetAwaiter().GetResult();
        
        // Solve the problem if all tests have passed
        _solverService.SolveProblem(settings.Year, settings.Day).GetAwaiter().GetResult();
        
        return 0;
    }
}