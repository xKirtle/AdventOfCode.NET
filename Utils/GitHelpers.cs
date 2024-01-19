using AdventOfCode.NET.Exceptions;
using LibGit2Sharp;

namespace AdventOfCode.NET.Utils;

internal static class GitHelpers
{
    private static string GetGitDefaultBranchName() {
        var defaultBranch = Environment.GetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH", EnvironmentVariableTarget.User);
        return !string.IsNullOrEmpty(defaultBranch) ? defaultBranch : "master";
    }
    
    public static Branch GetGitDefaultBranch(Repository repository) {
        var defaultBranchName = GetGitDefaultBranchName();
        var defaultBranch = repository.Branches[defaultBranchName];

        if (defaultBranch == null)
            throw new AoCException(AoCMessages.ErrorGitDefaultBranchNotFound(defaultBranchName));

        return defaultBranch;
    }
    
    public static Branch? GetGitBranch(Repository repository, string branchName) {
        return repository.Branches[branchName];
    }
    
    public static Branch CreateGitBranch(Repository repository, string branchName, Commit commit) {
        return repository.Branches.Add(branchName, commit, allowOverwrite: true);
    }
    
    public static void TryDeleteGitBranch(Repository repository, string branchName) {
        var branch = GetGitBranch(repository, branchName);
        if (branch == null)
            return;
        
        repository.Branches.Remove(branch);
    }
}