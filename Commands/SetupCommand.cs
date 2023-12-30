using AoC.NET.Services;
using Spectre.Console.Cli;

namespace AoC.NET.Commands;

internal sealed class SetupCommand : Command<SetupCommand.Settings>
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
        var (contentMd, input) = _problemService.FetchAndParseProblem(settings.Year, settings.Day).GetAwaiter().GetResult();
        _problemService.CreateProblemFiles(settings.Year, settings.Day, contentMd, input).GetAwaiter().GetResult();

        if (!settings.NoGit ?? true)
            _problemService.SetupGitForProblem(settings.Year, settings.Day);
        
        return 0;
    }
}