using System.Diagnostics;
using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using LibGit2Sharp;

namespace AdventOfCode.NET.Services;

internal interface IGitService
{
    Branch CreateOrGetBranch(IRepository repository, int year, int day, out bool isNewBranch);
    string DiscoverRepositoryPath();
    void CheckoutBranch(IRepository repository, Branch branch);
    void StageAndCommitNewProblem(IRepository repository, int year, int day, Branch branch);
    void StageAndCommitProblemUpdate(IRepository repository, int year, int day, ProblemLevel level);
}

internal class GitService(IEnvironmentVariablesService envVariablesService) : IGitService
{
    public Branch CreateOrGetBranch(IRepository repository, int year, int day, out bool isNewBranch) {
        isNewBranch = false;
        var defaultBranch = GetGitDefaultBranch(repository);
        var newProblemBranchName = GetBranchNameOfProblem(year, day);
        var newProblemBranch = GetGitBranch(repository, newProblemBranchName);

        if (newProblemBranch != null) 
            return newProblemBranch;
        
        newProblemBranch = CreateGitBranch(repository, newProblemBranchName, defaultBranch.Tip);
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
        var commitMessage = AoCMessages.InfoGitCommitMessage(year, day);
        InternalStageAndCommitProblem(repository, year, day, branch, commitMessage);
    }

    public void StageAndCommitProblemUpdate(IRepository repository, int year, int day, ProblemLevel level) {
        var expectedBranchName = GetBranchNameOfProblem(year, day);
        var currentBranchName = repository.Head.FriendlyName;
        
        var branchExists = GetGitBranch(repository, expectedBranchName) != null;
        if (!branchExists) {
            // Problem was setup with --no-git flag, so we don't need to update the branch
            return;
        }

        if (currentBranchName.Equals(expectedBranchName, StringComparison.OrdinalIgnoreCase)) {
            throw new AoCException(AoCMessages.WarningProblemGitBranchNotCheckedOut(year, day, expectedBranchName));
        }

        // level can't be PartOne because we only update the problem branch after solving PartOne
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        var commitMessage = level switch {
            ProblemLevel.PartTwo => AoCMessages.InfoGitCommitMessagePartOneSolved(year, day),
            ProblemLevel.Finished => AoCMessages.InfoGitCommitMessagePartTwoSolved(year, day),
            _ => throw new UnreachableException($"Invalid problem level value: {level}")
        };
        
        InternalStageAndCommitProblem(repository, year, day, repository.Head, commitMessage);
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

    private static void InternalStageAndCommitProblem(IRepository repository, int year, int day, Branch branch, string commitMessage) {
        LibGit2Sharp.Commands.Stage(repository, ProblemService.GetProblemDirectory(year, day));
        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        if (signature == null) {
            TryDeleteGitBranch(repository, branch);
            throw new AoCException(AoCMessages.ErrorGitAuthorNotFound);
        }

        repository.Commit(commitMessage, signature, signature);
    }

    private static string GetBranchNameOfProblem(int year, int day) {
        return $"problem/Y{year}/Day{day:00}";
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