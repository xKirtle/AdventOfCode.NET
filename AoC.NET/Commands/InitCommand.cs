using System.ComponentModel;
using Spectre.Console.Cli;

namespace AoC.NET.Commands;

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
        [DefaultValue("main")]
        public string DefaultBranch { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings) {
        Environment.SetEnvironmentVariable("AOC_SESSION_COOKIE", settings.Session);
        Environment.SetEnvironmentVariable("AOC_GIT_REMOTE_NAME", settings.RemoteName);
        Environment.SetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH", settings.DefaultBranch);
        
        return 0;
    }
}