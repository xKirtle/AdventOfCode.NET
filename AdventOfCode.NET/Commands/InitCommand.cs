using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class InitCommand : Command<InitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("AoC session token.")]
        [CommandArgument(0, "<session>")]
        public required string Session { get; init; } = null!;
        
        [Description("Your repository's remote branch")]
        [CommandOption("-r|--remote")]
        [DefaultValue("origin")]
        public required string? RemoteName { get; init; } = null!;

        [Description("Your repostiroy's default branch")]
        [CommandOption("-b|--branch")]
        [DefaultValue("master")]
        public required string? DefaultBranch { get; init; } = null!;
    }

    public override int Execute(CommandContext context, Settings settings) {
        var previousSession = Environment.GetEnvironmentVariable("AOC_SESSION_COOKIE", EnvironmentVariableTarget.User);
        if (IsValidNewValue(previousSession, settings.Session)) {
            if (!IsValidSessionToken(settings.Session)) {
                AnsiConsole.MarkupLine("[red]Invalid session token.[/] Please check your session token and try again.");
                return 1;
            }
            
            AnsiConsole.MarkupLine($"[green]{settings.Session[..4]}...{settings.Session[^4..]}[/] saved as the session token.");
            Environment.SetEnvironmentVariable("AOC_SESSION_COOKIE", settings.Session, EnvironmentVariableTarget.User);
        }

        var previousRemoteName = Environment.GetEnvironmentVariable("AOC_GIT_REMOTE_NAME", EnvironmentVariableTarget.User);
        if (IsValidNewValue(previousRemoteName, settings.RemoteName)) {
            AnsiConsole.MarkupLine($"[green]{settings.RemoteName}[/] saved as the remote name.");
            Environment.SetEnvironmentVariable("AOC_GIT_REMOTE_NAME", settings.RemoteName, EnvironmentVariableTarget.User);
        }
        
        var previousDefaultBranch = Environment.GetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH", EnvironmentVariableTarget.User);
        if (IsValidNewValue(previousDefaultBranch, settings.DefaultBranch)) {
            AnsiConsole.MarkupLine($"[green]{settings.DefaultBranch}[/] saved as the default branch.");
            Environment.SetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH", settings.DefaultBranch, EnvironmentVariableTarget.User);
        }
        
        return 0;
    }
    
    private static bool IsValidNewValue(string? previousValue, string? newValue) {
        return !string.IsNullOrEmpty(newValue) && newValue != previousValue;
    }

    private static bool IsValidSessionToken(string session) {
        const string expectedPrefix = "53616c7465645f5f"; // "Salted__" in hex
        const int expectedSize = 128;
        
        return session.StartsWith(expectedPrefix) && session.Length == expectedSize;
    }
}