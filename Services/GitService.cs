using System.Diagnostics;
using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using LibGit2Sharp;

namespace AdventOfCode.NET.Services;

internal interface IGitService
{
    Branch CreateOrGetBranch(IRepository repository, int year, int day, out bool isNewBranch);
    string DiscoverRepositoryPath();
    void MergeProblemBranchIntoDefaultBranch(Repository repository, Branch problemBranch);
    void TryDeleteGitBranch(IRepository repository, Branch branch);
    bool IsProblemBranchCheckedOut(IRepository repository, int year, int day);
    string GetBranchNameOfProblem(int year, int day);
    void CheckoutBranch(IRepository repository, Branch branch);
    void CommitProblemFiles(IRepository repository, int year, int day, string commitMessage, Branch problemBranch);
    string GetProblemSolvedCommitMessage(int year, int day, ProblemLevel level);
    void StageProblemFiles(IRepository repository, int year, int day);
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
    
    public bool IsProblemBranchCheckedOut(IRepository repository, int year, int day) {
        var branchName = GetBranchNameOfProblem(year, day);
        return repository.Head.FriendlyName.Equals(branchName, StringComparison.OrdinalIgnoreCase);
    }

    public void StageAndCommitNewProblem(IRepository repository, int year, int day, Branch branch) {
        var commitMessage = AoCMessages.InfoInitialGitCommitMessage(year, day);
        InternalStageAndCommitProblem(repository, year, day, branch, commitMessage);
    }
    
    public void StageProblemFiles(IRepository repository, int year, int day) {
        LibGit2Sharp.Commands.Stage(repository, ProblemService.GetProblemDirectory(year, day));
    }
    
    public string GetProblemSolvedCommitMessage(int year, int day, ProblemLevel level) {
        // level can't be PartOne because we only update the problem branch after solving PartOne
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return level switch {
            ProblemLevel.PartTwo => AoCMessages.InfoGitCommitMessagePartOneSolved(year, day),
            ProblemLevel.Finished => AoCMessages.InfoGitCommitMessagePartTwoSolved(year, day),
            _ => throw new UnreachableException($"Invalid problem level value: {level}")
        };
    }
    
    public void CommitProblemFiles(IRepository repository, int year, int day, string commitMessage, Branch problemBranch) {
        var signature = GetSignature(repository);
        repository.Commit(commitMessage, signature, signature);
    }

    public void CheckoutBranch(IRepository repository, Branch branch) {
        LibGit2Sharp.Commands.Checkout(repository, branch);
    }

    public void MergeProblemBranchIntoDefaultBranch(Repository repository, Branch problemBranch) {
        // Fetch remote branch?
        
        var defaultBranch = GetGitDefaultBranch(repository);
        CheckoutBranch(repository, defaultBranch);
        
        var signature = GetSignature(repository);
        var mergeResult = repository.Merge(problemBranch, signature);
        
        switch (mergeResult.Status) {
            case MergeStatus.Conflicts:
                throw new AoCException(AoCMessages.ErrorGitMergeConflict(problemBranch.FriendlyName));
            
            case MergeStatus.NonFastForward:
                repository.Commit($"Merge {problemBranch.FriendlyName} into {defaultBranch.FriendlyName}", signature, signature);
                break;

            // Nothing to do, branches were already in sync or merge was trivial
            case MergeStatus.FastForward:
            case MergeStatus.UpToDate:
                break;

            default:
                throw new AoCException(AoCMessages.ErrorGitMergeFailed(problemBranch.FriendlyName, defaultBranch.FriendlyName, mergeResult.Status));
        }
        
        // Prompt user to delete problem branch?
        // repository.Branches.Remove(problemBranch);
        repository.Network.Push(defaultBranch);
    }

    public void TryDeleteGitBranch(IRepository repository, Branch branch) {
        if (repository.Branches.Contains(branch))
            repository.Branches.Remove(branch);
    }

    public string GetBranchNameOfProblem(int year, int day) {
        return $"problem/Y{year}/Day{day:00}";
    }

    private void InternalStageAndCommitProblem(IRepository repository, int year, int day, Branch branch, string commitMessage) {
        LibGit2Sharp.Commands.Stage(repository, ProblemService.GetProblemDirectory(year, day));
        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        if (signature == null) {
            TryDeleteGitBranch(repository, branch);
            throw new AoCException(AoCMessages.ErrorGitAuthorNotFound);
        }

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

    private static Signature GetSignature(IRepository repository) {
        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        if (signature == null) {
            throw new AoCException(AoCMessages.ErrorGitAuthorNotFound);
        }

        return signature;
    }

    private static Branch? GetGitBranch(IRepository repository, string branchName) {
        return repository.Branches[branchName];
    }

    private static Branch CreateGitBranch(IRepository repository, string branchName, Commit commit) {
        return repository.Branches.Add(branchName, commit, allowOverwrite: true);
    }
}