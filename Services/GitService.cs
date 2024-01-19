using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using LibGit2Sharp;

namespace AdventOfCode.NET.Services;

internal interface IGitService
{
    Branch GetGitDefaultBranch(Repository repository);
    Branch? GetGitBranch(Repository repository, string branchName);
    Branch CreateGitBranch(Repository repository, string branchName, Commit commit);
    void TryDeleteGitBranch(Repository repository, string branchName);
}

internal class GitService(IEnvironmentVariablesService envVariablesService) : IGitService
{
    public Branch GetGitDefaultBranch(Repository repository) {
        var defaultBranchName = GetGitDefaultBranchName();
        var defaultBranch = repository.Branches[defaultBranchName];

        if (defaultBranch == null)
            throw new AoCException(AoCMessages.ErrorGitDefaultBranchNotFound(defaultBranchName));

        return defaultBranch;
    }

    public Branch? GetGitBranch(Repository repository, string branchName) {
        return repository.Branches[branchName];
    }

    public Branch CreateGitBranch(Repository repository, string branchName, Commit commit) {
        return repository.Branches.Add(branchName, commit, allowOverwrite: true);
    }

    public void TryDeleteGitBranch(Repository repository, string branchName) {
        var branch = GetGitBranch(repository, branchName);
        if (branch == null)
            return;
        
        repository.Branches.Remove(branch);
    }
    
    private string GetGitDefaultBranchName() {
        var defaultBranch = envVariablesService.GetVariable(EnvironmentVariables.GitDefaultBranch);
        return !string.IsNullOrEmpty(defaultBranch) ? defaultBranch : "master";
    }
}