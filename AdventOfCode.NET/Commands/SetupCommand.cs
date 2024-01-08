using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;

internal sealed class SetupCommand : Command<SetupCommand.Settings>
{
    public sealed class Settings : DateSettings
    {
        [CommandOption("--no-git")]
        public bool? NoGit { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings) {
        return 0;
    }
}