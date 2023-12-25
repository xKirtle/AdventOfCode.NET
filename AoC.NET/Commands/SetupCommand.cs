using AoC.NET.Services;
using Spectre.Console.Cli;

namespace AoC.NET.Commands;

internal class SetupCommand : Command<SetupCommand.Settings>
{
    private readonly IProblemService _problemService;
    
    public sealed class Settings : DateSettings
    {
        [CommandOption("--no-git")]
        public bool? NoGit { get; set; }
    }

    public SetupCommand(IProblemService problemService) {
        _problemService = problemService ?? throw new ArgumentNullException(nameof(problemService));
    }

    public override int Execute(CommandContext context, Settings settings) {
        var problem = _problemService.FetchAndParseProblem(settings.Year, settings.Day).GetAwaiter().GetResult();
        
        // Create README, Solution.cs template, and test/ folder with test1.in and test1.refout
        _problemService.CreateProblemFiles(problem);
        
        return 0;
    }
}