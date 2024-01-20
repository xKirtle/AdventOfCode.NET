using AdventOfCode.NET.Model;
using AdventOfCode.NET.Services;

namespace AdventOfCode.NET.Exceptions;

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
        $"""
         [yellow]Warning: [/]Invalid problem level: [yellow]{level}[/]. Expected '1' or '2'.
         Skipping submission...
         """;

    public static string InfoCreatingProblemDirectory(string path) =>
        $"Creating problem directory at [blue]{path}[/]...";

    public static string InfoCreatingProblemFile(string path) => $"Creating problem file at [blue]{path}[/]...";

    public static string WarningPromptCreatingProblemFileOverriding(string path) =>
        $"[yellow]Warning: [/]File already exists at [yellow]{path}[/]. Overwrite? [green](Y/n)[/] ";
    
    public static string InfoCreatingProblemFileOverridingSkipped(string path) =>
        $"Skipping creation of file at [blue]{path}[/]...";

    public static string ErrorGitDefaultBranchNotFound(string defaultBranchName) =>
        $"""
         [red]Error: [/]Default branch [blue]{defaultBranchName}[/] not found.
         Please set the AOC_GIT_DEFAULT_BRANCH environment variable with the name of your default branch.";
         """;
    
    public static string WarningGitProblemBranchAlreadyExists(string branchName) =>
        $"""
         [yellow]Warning: [/]Branch [yellow]{branchName}[/] already exists.
         [yellow]Skipping Git setup...[/]
         """;
    
    public static string InfoGitCommitMessage(int year, int day) => $"Initial commit for Y{year}D{day}";

    public static string ErrorGitAuthorNotFound =>
        """
        [red]Error: [/]Author information not found in global Git configuration.
        Please set your name and email with 'git config --global user.name \"Your Name\"' and 'git config --global user.email \".
        [yellow]Aborting Git setup...[/]
        """;
    
    public static string ErrorGitRepositoryNotFound =>
        """
        [red]Error: [/]Could not find a Git repository. Please run this command from the root of your git repository.
        Aborting Git setup...
        """;
    
    public static string ErrorGitRepositoryNotClean =>
        """
        [red]Error: [/]Git repository is not clean. 
        Please commit or stash your changes before running this command.
        Alternatively, you can use the [blue]--no-git[/] flag to skip Git setup.
        [yellow]Aborting Git setup...[/]
        """;
    
    public static string ErrorGitCheckoutFailed(string branchName) =>
        $"""
         [red]Error: [/]Could not checkout branch [blue]{branchName}[/].
         [yellow]Aborting Git setup...[/]
         """;
    
    public static string ErrorMultipleProblemsFound(int year, int day) =>
        $"""
         [red]Error: [/]Multiple problem solutions found for Y{year}D{day}.
         Please ensure only one class is marked with [blue]AoCSolutionAttribute[/] for each day.
         """;
    
    public static string ErrorNoProblemFound(int year, int day) =>
        $"""
         [red]Error: [/]No problem solution found for Y{year}D{day}.
         Please ensure a class is marked with [blue]AoCSolutionAttribute[/] for each day.
         """;
    
    public static string ErrorSolutionInstantiationFailed(int year, int day) =>
        $"""
         [red]Error: [/]Could not instantiate solution for Y{year}D{day}.
         Please ensure the solution class has a public parameterless constructor.
         """;
    
    public static string WarningNoTestsDirectoryFound(int year, int day) =>
        $"""
         [yellow]Warning: [/]No tests directory found for Y{year}D{day}.
         [yellow]Skipping test execution...[/]
         """;
    
    public static string ErrorInvalidTestCase(string filePath) =>
        $"""
         [red]Error: [/]Invalid test case file: [blue]{filePath}[/].
         Please ensure the test case file follows the template:
         {ProblemService.GetProblemTestTemplate}
         """;
    
    public static string ErrorParsingTestCasePart(string filePath) =>
        $"""
         [red]Error: [/]Could not parse problem part from test case file: [blue]{filePath}[/].
         Please ensure the test case file follows the template:
         {ProblemService.GetProblemTestTemplate}
         """;
    
    public static string ErrorParsingTestCaseInput(string filePath) =>
        $"""
         [red]Error: [/]Could not parse problem input from test case file: [blue]{filePath}[/].
         Please ensure the test case file follows the template:
         {ProblemService.GetProblemTestTemplate}
         """;
    
    public static string ErrorParsingTestCaseOutput(string filePath) =>
        $"""
         [red]Error: [/]Could not parse problem output from test case file: [blue]{filePath}[/].
         Please ensure the test case file follows the template:
         {ProblemService.GetProblemTestTemplate}
         """;
    
    public static string ErrorTestCaseFailed(string filePath, string expected, string actual) =>
        $"""
         [red]Error: [/]Test failed for [blue]{filePath}[/].
         Expected [blue]{expected}[/], but got [blue]{actual}[/] instead.
         """;
    
    public static string SuccessTestCasePassed(string filePath, long time, string timeColorTag) =>
        $"Test passed for [blue]{filePath}[/] in [{timeColorTag}]{time}ms[/].";
    
    public static string ErrorProblemSolutionIsNull(int year, int day, ProblemLevel level) =>
        $"""
         [red]Error: [/]Solution for Y{year}D{day} part {(level == ProblemLevel.PartOne ? "one" : "two")} is [yellow]null[/].
         Please ensure the solution does not return a nullable value.
         """;
    
    public static string WarningProblemAlreadySolved(int year, int day) =>
        $"[yellow]Warning: [/]Problem Y{year}D{day} is already solved in AoC. Skipping submission...";
    
    public static string SuccessSetupCompleted(int year, int day) =>
        $"[green]Success:[/] Setup completed for [blue]Y{year}/D{day}[/]!";
    
    public static string InfoSessionTokenUnmodified =>
        "New session token is the same as the previous one. Skipping update...";
    
    public static string SuccessNoGitSaved(string noGit) =>
        $"[blue]{noGit}[/] saved as the no-git flag.";
    
    public static string InfoUpdatingProblemFiles(int year, int day) =>
        $"Updating problem files for [blue]Y{year}/D{day}[/]...";
}