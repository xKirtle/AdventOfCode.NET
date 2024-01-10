using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli")]
internal sealed class InitCommand : Command<InitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("AoC session token.")]
        [CommandArgument(0, "<session>")]
        public required string Session { get; init; } = null!;

        [Description("Your repostiroy's default branch")]
        [CommandOption("-b|--branch")]
        [DefaultValue("master")]
        public required string DefaultBranch { get; init; } = null!;
    }

    public override int Execute(CommandContext context, Settings settings) {
        var previousSession = Environment.GetEnvironmentVariable("AOC_SESSION_COOKIE", EnvironmentVariableTarget.User);
        if (IsValidNewValue(previousSession, settings.Session)) {
            if (!IsValidSessionToken(settings.Session))
                throw new AoCException(AoCMessages.ErrorSessionTokenInvalid);
            
            AnsiConsole.MarkupLine(AoCMessages.SuccessSessionTokenSaved(settings.Session));
            Environment.SetEnvironmentVariable("AOC_SESSION_COOKIE", settings.Session, EnvironmentVariableTarget.User);
        }
        
        var previousDefaultBranch = Environment.GetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH", EnvironmentVariableTarget.User);
        if (IsValidNewValue(previousDefaultBranch, settings.DefaultBranch)) {
            AnsiConsole.MarkupLine(AoCMessages.SuccessDefaultBranchSaved(settings.DefaultBranch));
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