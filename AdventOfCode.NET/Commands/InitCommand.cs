using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;

internal sealed class InitCommand : Command<InitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("AoC session token.")]
        [CommandArgument(0, "<session>")]
        public string Session { get; init; }
        
        [Description("Your repository's remote branch")]
        [CommandOption("-r|--remote")]
        [DefaultValue("origin")]
        public string RemoteName { get; init; }

        [Description("Your repostiroy's default branch")]
        [CommandOption("-b|--branch")]
        [DefaultValue("master")]
        public string DefaultBranch { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings) {
        if (!string.IsNullOrEmpty(settings.Session))
            AnsiConsole.MarkupLine($"[green]{settings.Session[..4]}...{settings.Session[^4..]}[/] saved as the session token.");
        
        Environment.SetEnvironmentVariable("AOC_SESSION_COOKIE", settings.Session, EnvironmentVariableTarget.User);
        
        var previousRemoteName = Environment.GetEnvironmentVariable("AOC_GIT_REMOTE_NAME", EnvironmentVariableTarget.User);
        if (!string.IsNullOrEmpty(settings.RemoteName) && settings.RemoteName != previousRemoteName) {
            AnsiConsole.MarkupLine($"[green]{settings.RemoteName}[/] saved as the remote name.");
            Environment.SetEnvironmentVariable("AOC_GIT_REMOTE_NAME", settings.RemoteName, EnvironmentVariableTarget.User);
        }
        
        var previousDefaultBranch = Environment.GetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH", EnvironmentVariableTarget.User);
        if (!string.IsNullOrEmpty(settings.DefaultBranch) && settings.DefaultBranch != previousDefaultBranch) {
            AnsiConsole.MarkupLine($"[green]{settings.DefaultBranch}[/] saved as the default branch.");
            Environment.SetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH", settings.DefaultBranch, EnvironmentVariableTarget.User);
        }
        
        return 0;
    }
}