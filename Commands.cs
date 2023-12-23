using System.Diagnostics;
using System.Runtime.InteropServices;
using AdventOfCode.NET.Model;
using Cocona;

namespace AdventOfCode.NET;

[DebuggerStepThrough]
internal class Commands
{
    [Command]
    public static void Setup(DateParameters date, [Option("no-git")] bool noGit) {
        Console.WriteLine($"{date.Year} {date.Day} {noGit}");
    }

    [Command]
    public static void Other() {
        
    }
}