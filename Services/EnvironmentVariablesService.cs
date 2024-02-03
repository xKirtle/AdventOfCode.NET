using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;

namespace AdventOfCode.NET.Services;

internal interface IEnvironmentVariablesService
{
    string SessionCookie { get; }
    string GitDefaultBranch { get; }
    bool NoGit { get; }
    bool VerboseOutput { get; }
    bool TrySetVariable(EnvironmentVariables key, string? value);
}

internal class EnvironmentVariablesService : IEnvironmentVariablesService
{
    public string SessionCookie
    {
        get => GetVariable(EnvironmentVariables.SessionCookie) ??
               throw new AoCException(AoCMessages.ErrorSessionTokenNotFound);
        set => TrySetVariable(EnvironmentVariables.SessionCookie, value);
    }
    
    public string GitDefaultBranch
    {
        get => GetVariable(EnvironmentVariables.GitDefaultBranch) ?? "master";
        set => TrySetVariable(EnvironmentVariables.GitDefaultBranch, value);
    }
    
    public bool NoGit
    {
        get => bool.TryParse(GetVariable(EnvironmentVariables.NoGit), out var noGit) && noGit;
        set => TrySetVariable(EnvironmentVariables.NoGit, value.ToString());
    }
    
    public bool VerboseOutput
    {
        get => bool.TryParse(GetVariable(EnvironmentVariables.VerboseOutput), out var verbose) && verbose;
        set => TrySetVariable(EnvironmentVariables.VerboseOutput, value.ToString());
    }
    
    private static readonly Dictionary<EnvironmentVariables, string> EnvironmentVariableKeys = new() {
        { EnvironmentVariables.SessionCookie, "AOC_SESSION_COOKIE" },
        { EnvironmentVariables.GitDefaultBranch, "AOC_GIT_DEFAULT_BRANCH" },
        { EnvironmentVariables.NoGit, "AOC_NO_GIT" },
        { EnvironmentVariables.VerboseOutput, "AOC_VERBOSE_OUTPUT" }
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

    private static string? GetVariable(EnvironmentVariables key) {
        var keyName = EnvironmentVariableKeys[key];
        return Environment.GetEnvironmentVariable(keyName, Target);
    }

    private static bool IsValidNewValue(string? previousValue, string? newValue) {
        return !string.IsNullOrEmpty(newValue) && newValue != previousValue;
    }
}