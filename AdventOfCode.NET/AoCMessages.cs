﻿namespace AdventOfCode.NET;

/// <summary>
/// Messages used throughout the application formatted with Spectre.Console markup.
/// </summary>
internal static class AoCMessages
{
    // Naming convention: <type><context><reason>
    
    public static string ErrorSessionTokenInvalid =>
        "[red]Invalid session token.[/] Please check your session token and try again.";
    
    public static string ErrorSessionTokenNotFound =>
        "[red]Session token not found.[/] Please set the AOC_SESSION_TOKEN environment variable.";
    
    public static string SuccessSessionTokenSaved(string session) =>
        $"[blue]{session[..4]}...{session[^4..]}[/] saved as the session token.";
    
    public static string SuccessRemoteNameSaved(string remoteName) =>
        $"[blue]{remoteName}[/] saved as the remote name.";
    
    public static string SuccessDefaultBranchSaved(string defaultBranch) =>
        $"[blue]{defaultBranch}[/] saved as the default branch.";
    
    public static string InfoFetchingContent(string url) => $"Fetching content from [blue]{url}[/]...";

    public static string HttpErrorFetchingContent(string url, int statusCode, string? reason) =>
        $"[red]Error fetching content from {url}[/]. HTTP status code: [red]{statusCode}[/]. Reason: [blue]{reason}[/].";

    public static string HttpErrorFetchingContent(string url) =>
        $"[red]Unknown error fetching content from {url}[/].";

    public static string ErrorProblemNodeNotFound =>
        "[red]Error parsing problem node.[/] Could not find the problem node in the HTML document.";

    public static string ErrorProblemLevelInvalid =>
        "[red]Error parsing problem level.[/] Could not parse the problem level from the HTML document.";

    public static string HttpErrorSubmittingSolution(string url, int statusCode, string? reason) =>
        $"[red]Error submitting solution to {url}[/]. HTTP status code: [red]{statusCode}[/]. Reason: [blue]{reason}[/].";
    
    public static string HttpErrorSubmittingSolution(string url) =>
        $"[red]Unknown error submitting solution to {url}[/].";
    
    public static string WarningSubmittingSolutionInvalidLevel(string level) =>
        $"[yellow]Warning: [/]Invalid problem level: [yellow]{level}[/]. Expected '1' or '2'. Skipping submission...";

    public static string InfoCreatingProblemDirectory(string path) =>
        $"Creating problem directory at [blue]{path}[/]...";

    public static string InfoCreatingProblemFile(string path) => $"Creating problem file at [blue]{path}[/]...";

    public static string WarningPromptCreatingProblemFileOverriding(string path) =>
        $"[yellow]Warning: [/]File already exists at [yellow]{path}[/]. Overwrite? [green](Y/n)[/] ";
    
    public static string InfoCreatingProblemFileOverridingSkipped(string path) =>
        $"[yellow]Skipping creation of file at [yellow]{path}[/]...";
    
    public static string ErrorGitRemoteNotFound(string remoteName) =>
        $"[red]Error: [/]Remote [blue]{remoteName}[/] not found. Please set the AOC_GIT_REMOTE_NAME environment variable with the name of your remote.";
    
    public static string ErrorGitDefaultBranchNotFound(string defaultBranchName) =>
        $"[red]Error: [/]Default branch [blue]{defaultBranchName}[/] not found. Please set the AOC_GIT_DEFAULT_BRANCH environment variable with the name of your default branch.";
    
    public static string WarningGitProblemBranchAlreadyExists(string branchName) =>
        $"[yellow]Warning: [/]Branch [yellow]{branchName}[/] already exists. Skipping git setup.";
    
    public static string InfoGitCommitMessage(int year, int day) => $"Initial commit for Y{year}D{day}";

    public static string ErrorGitAuthorNotFound =>
        "[red]Error: [/]Author information not found in global Git configuration. Please set your name and email with 'git config --global user.name \"Your Name\"' and 'git config --global user.email \"";
    
    public static string ErrorGitRepositoryNotFound =>
        "[red]Error: [/]Could not find a Git repository. Please run this command from the root of your repository.";
}