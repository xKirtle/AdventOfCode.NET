using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using AdventOfCode.NET.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli")]
internal sealed class SetupCommand(IHttpService httpService, IProblemService problemService) : Command<SetupCommand.Settings>
{
    public sealed class Settings : DateSettings
    {
        [Description("Ignore git operations.")]
        [CommandOption("--no-git")]
        [DefaultValue(false)]
        public required bool NoGit { get; init; } = false;
    }

    public override int Execute(CommandContext context, Settings settings) {
        var problemNode = httpService.FetchProblemAsync(settings.Year, settings.Day).GetAwaiter().GetResult();
        var problemInput = httpService.FetchProblemInputAsync(settings.Year, settings.Day).GetAwaiter().GetResult();
        var problemModel = problemService.ParseProblem(settings.Year, settings.Day, problemNode, problemInput);
        
        problemService.SetupProblemFiles(problemModel).GetAwaiter().GetResult();

        if (!settings.NoGit)
        {
            try {
                problemService.SetupGitForProblem(settings.Year, settings.Day);
            }
            catch (Exception ex) {
                AnsiConsole.MarkupLine(ex.Message);
            }
        }

        AnsiConsole.MarkupLine(AoCMessages.SuccessSetupCompleted(settings.Year, settings.Day));
        
        return 0;
    }
}