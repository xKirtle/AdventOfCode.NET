using System.Diagnostics;
using System.Runtime.InteropServices;
using AdventOfCode.NET.Model;
using Cocona;

namespace AdventOfCode.NET;

[DebuggerStepThrough]
internal class Commands
{
    [Command(Description = "Initialize environment variables.")]
    public void Init(
        [Argument(Description = "AoC's session token.")] string session,
        [Option("remote", ['r'], Description = "Your repository's remote branch.")] string remoteName = "origin", 
        [Option("branch", ['b'], Description = "Your repository's default branch.")] string defaultBranch = "main"
    )
    {
        Environment.SetEnvironmentVariable("AOC_SESSION_COOKIE", session);
        
        // TODO: Store these in an config file
        Environment.SetEnvironmentVariable("AOC_GIT_REMOTE_NAME", remoteName);
        Environment.SetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH", defaultBranch);
    }
    
    [Command]
    public static Task Setup(DateParameters date, [Option("no-git")] bool noGit = false) {
        return AOCUtils.SetupProblem(date.Year, date.Day, !noGit);
    }

    [Command]
    public static void Other() {
        
    }
}