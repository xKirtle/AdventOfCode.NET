namespace AdventOfCode.NET;

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
    
    public static string ErrorProblemNodeNotFound => "[red]Error fetching problem node.[/] Please check your session token and try again.";
    
    public static string ErrorProblemInputInvalid => "[red]Error fetching problem input.[/] Please check your session token and try again.";
    
    public static string HttpErrorSubmittingSolution(string url, int statusCode, string? reason) =>
        $"[red]Error submitting solution to {url}[/]. HTTP status code: [red]{statusCode}[/]. Reason: [blue]{reason}[/].";
    
    public static string HttpErrorSubmittingSolution(string url) =>
        $"[red]Unknown error submitting solution to {url}[/].";
    
    public static string WarningSubmittingSolutionInvalidLevel(string level) =>
        $"[yellow]Warning: [/]Invalid problem level: [yellow]{level}[/]. Expected '1' or '2'. Skipping submission...";
}