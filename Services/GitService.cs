using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using LibGit2Sharp;

namespace AdventOfCode.NET.Services;

internal interface IGitService
{
    Branch CreateOrGetBranch(IRepository repository, string branchName, out bool isNewBranch);
    string DiscoverRepositoryPath();
    void CheckoutBranch(IRepository repository, Branch branch);
    void StageAndCommitNewProblem(IRepository repository, int year, int day, Branch branch);
}

internal class GitService(IEnvironmentVariablesService envVariablesService) : IGitService
{
    public Branch CreateOrGetBranch(IRepository repository, string branchName, out bool isNewBranch) {
        isNewBranch = false;
        var defaultBranch = GetGitDefaultBranch(repository);
        var newProblemBranch = GetGitBranch(repository, branchName);

        if (newProblemBranch != null) 
            return newProblemBranch;
        
        newProblemBranch = CreateGitBranch(repository, branchName, defaultBranch.Tip);
        isNewBranch = true;

        return newProblemBranch;
    }

    public string DiscoverRepositoryPath() {
        var pathToRepo = Repository.Discover(".\\");
        
        if (string.IsNullOrEmpty(pathToRepo))
            throw new AoCException(AoCMessages.ErrorGitRepositoryNotFound);
        
        return pathToRepo;
    }

    public void CheckoutBranch(IRepository repository, Branch branch) {
        try {
            LibGit2Sharp.Commands.Checkout(repository, branch);
        } catch (CheckoutConflictException ex) {
            TryDeleteGitBranch(repository, branch);
            throw new AoCException(AoCMessages.ErrorGitRepositoryNotClean, ex);
        } catch (Exception ex) {
            TryDeleteGitBranch(repository, branch);
            throw new AoCException(AoCMessages.ErrorGitCheckoutFailed(branch.FriendlyName), ex);
        }
    }

    public void StageAndCommitNewProblem(IRepository repository, int year, int day, Branch branch) {
        LibGit2Sharp.Commands.Stage(repository, year.ToString());
        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        if (signature == null) {
            TryDeleteGitBranch(repository, branch);
            throw new AoCException(AoCMessages.ErrorGitAuthorNotFound);
        }

        var commitMessage = AoCMessages.InfoGitCommitMessage(year, day);
        repository.Commit(commitMessage, signature, signature);
    }

    private string GetGitDefaultBranchName() {
        var defaultBranch = envVariablesService.GetVariable(EnvironmentVariables.GitDefaultBranch);
        return !string.IsNullOrEmpty(defaultBranch) ? defaultBranch : "master";
    }

    private Branch GetGitDefaultBranch(IRepository repository) {
        var defaultBranchName = GetGitDefaultBranchName();
        var defaultBranch = repository.Branches[defaultBranchName];

        if (defaultBranch == null)
            throw new AoCException(AoCMessages.ErrorGitDefaultBranchNotFound(defaultBranchName));

        return defaultBranch;
    }

    private static void TryDeleteGitBranch(IRepository repository, Branch branch) {
        if (repository.Branches.Contains(branch))
            repository.Branches.Remove(branch);
    }

    private static Branch? GetGitBranch(IRepository repository, string branchName) {
        return repository.Branches[branchName];
    }

    private static Branch CreateGitBranch(IRepository repository, string branchName, Commit commit) {
        return repository.Branches.Add(branchName, commit, allowOverwrite: true);
    }
}