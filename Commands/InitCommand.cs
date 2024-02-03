using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using AdventOfCode.NET.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli")]
internal sealed class InitCommand(IEnvironmentVariablesService envVariablesService) : Command<InitCommand.Settings>
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
        
        [Description("Ignore git operations for all commands.")]
        [CommandOption("--no-git")]
        [DefaultValue(false)]
        public required bool NoGit { get; init; } = false;
        
        [Description("Enable silent output.")]
        [CommandOption("--silent")]
        [DefaultValue(false)]
        public required bool SilentOutput { get; init; } = false;
    }

    public override int Execute(CommandContext context, Settings settings) {
        if (!IsValidSessionToken(settings.Session))
            throw new AoCException(AoCMessages.ErrorSessionTokenInvalid);

        AnsiConsole.MarkupLine(
            envVariablesService.TrySetVariable(EnvironmentVariables.SessionCookie, settings.Session)
                ? AoCMessages.SuccessSessionTokenSaved(settings.Session)
                : AoCMessages.InfoSessionTokenUnmodified);
        
        if (envVariablesService.TrySetVariable(EnvironmentVariables.GitDefaultBranch, settings.DefaultBranch))
            AnsiConsole.MarkupLine(AoCMessages.SuccessDefaultBranchSaved(settings.DefaultBranch));
        
        if (envVariablesService.TrySetVariable(EnvironmentVariables.NoGit, settings.NoGit.ToString()))
            AnsiConsole.MarkupLine(AoCMessages.SuccessNoGitSaved(settings.NoGit.ToString()));
        
        if (envVariablesService.TrySetVariable(EnvironmentVariables.SilentOutput, settings.SilentOutput.ToString()))
            AnsiConsole.MarkupLine(AoCMessages.SuccessSilentOutputSaved(settings.SilentOutput.ToString()));
        
        return 0;
    }

    private static bool IsValidSessionToken(string session) {
        const string expectedPrefix = "53616c7465645f5f"; // "Salted__" in hex
        const int expectedSize = 128;
        
        return !string.IsNullOrEmpty(session) && session.StartsWith(expectedPrefix) && session.Length == expectedSize;
    }
}