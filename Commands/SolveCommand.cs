using AoC.NET.Services;
using Spectre.Console.Cli;

namespace AoC.NET.Commands;

internal class SolveCommand : Command<SolveCommand.Settings>
{
    private readonly ISolverService _solverService;
    
    public sealed class Settings : DateSettings { }

    public SolveCommand(ISolverService solverService) {
        _solverService = solverService ?? throw new ArgumentNullException(nameof(solverService));
    }

    public override int Execute(CommandContext context, Settings settings) {
        
        
        return 0;
    }
}