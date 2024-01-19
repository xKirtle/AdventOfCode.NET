using AdventOfCode.NET.Model;

namespace AdventOfCode.NET.Services;

internal interface IEnvironmentVariablesService
{
    string? SessionCookie { get; set; }
    string? GitDefaultBranch { get; set; }
    bool NoGit { get; set; }
    bool TrySetVariable(EnvironmentVariables key, string? value);
    string? GetVariable(EnvironmentVariables key);
}

internal class EnvironmentVariablesService : IEnvironmentVariablesService
{
    public string? SessionCookie
    {
        get => GetVariable(EnvironmentVariables.SessionCookie);
        set => TrySetVariable(EnvironmentVariables.SessionCookie, value);
    }
    
    public string? GitDefaultBranch
    {
        get => GetVariable(EnvironmentVariables.GitDefaultBranch);
        set => TrySetVariable(EnvironmentVariables.GitDefaultBranch, value);
    }
    
    public bool NoGit
    {
        get => bool.TryParse(GetVariable(EnvironmentVariables.NoGit), out var noGit) && noGit;
        set => TrySetVariable(EnvironmentVariables.NoGit, value.ToString());
    }
    
    private static readonly Dictionary<EnvironmentVariables, string> EnvironmentVariableKeys = new() {
        { EnvironmentVariables.SessionCookie, "AOC_SESSION_COOKIE" },
        { EnvironmentVariables.GitDefaultBranch, "AOC_GIT_DEFAULT_BRANCH" },
        { EnvironmentVariables.NoGit, "AOC_NO_GIT" }
    };

    private const EnvironmentVariableTarget Target = EnvironmentVariableTarget.User;

    public bool TrySetVariable(EnvironmentVariables key, string? value) {
        if (value == null)
            return false;
        
        var keyName = EnvironmentVariableKeys[key];
        var previousValue = Environment.GetEnvironmentVariable(keyName, Target);

        if (!IsValidNewValue(previousValue, value))
            return false;
        
        Environment.SetEnvironmentVariable(keyName, value, Target);
        return true;
    }

    public string? GetVariable(EnvironmentVariables key) {
        var keyName = EnvironmentVariableKeys[key];
        return Environment.GetEnvironmentVariable(keyName, Target);
    }

    private static bool IsValidNewValue(string? previousValue, string? newValue) {
        return !string.IsNullOrEmpty(newValue) && newValue != previousValue;
    }
}