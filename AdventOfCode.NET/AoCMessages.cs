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
    
    public static string ErrorFetchingContent(string url, int statusCode, string? reason) => 
        $"[red]Error fetching content from {url}[/]. HTTP status code: [red]{statusCode}[/]. Reason: [blue]{reason}[/].";
}