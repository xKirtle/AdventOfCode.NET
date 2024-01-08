using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli")]
internal sealed class SetupCommand : Command<SetupCommand.Settings>
{
    public sealed class Settings : DateSettings
    {
        [Description("Ignore git operations.")]
        [CommandOption("--no-git")]
        [DefaultValue(false)]
        public required bool? NoGit { get; init; } = null!;
    }

    public override int Execute(CommandContext context, Settings settings) {
        
        return 0;
    }
}