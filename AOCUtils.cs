using System.Diagnostics;
using LibGit2Sharp;

internal static class AOCUtils
{
    public static void OpenJetBrainsRider(ReadOnlySpan<string> args) 
    {
        string riderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Rider\bin\rider64.exe");
        string arguments = string.Join(" ", args.ToArray());

        Process.Start(riderPath, arguments).WaitForExit();
    }
    
    public static async Task SetupProblemWithGit(int year, int day) {
        using var repo = new Repository(".git");
        var defaultBranch = repo.Branches[GetGitDefaultBranches().FirstOrDefault(x => repo.Branches[x] != null)];
        
        if (defaultBranch == null)
            throw new InvalidOperationException("Could not find a default branch of the repository. If you're using something other than 'main' or 'master', please set the AOC_GIT_DEFAULT_BRANCH environment variable with its name.");
        
        var newProblemBranch = repo.Branches[$"problems/Y{year}/D{day}"];
        var branchExists = newProblemBranch != null;
        newProblemBranch ??= repo.Branches.Add($"problems/Y{year}/D{day}", defaultBranch.Tip, allowOverwrite: true);

        if (!branchExists) {
            await SetupProblem(year, day);
            
            Commands.Stage(repo, year.ToString());
            repo.Reset(ResetMode.Mixed, "**/test/*");

            var author = repo.Config.BuildSignature(DateTimeOffset.Now);

            if (author == null) {
                var msg = "Author information not found in global Git configuration. Please set your name and email with 'git config --global user.name \"Your Name\"' and 'git config --global user.email \"";
                throw new InvalidOperationException(msg);
            }
            
            repo.Commit($"Initial commit for Y{year}D{day}", author, author);
            repo.Tags.Add($"Y{year}D{day}", repo.Head.Tip);
            
            OpenJetBrainsRider([$"{year}/Day{day:00}/README.md", $"{year}/Day{day:00}/Solution.cs", $"{year}/Day{day:00}/test/test1.in", $"{year}/Day{day:00}/test/test1.refout"]);
        }
        else {
            Console.WriteLine($"Branch {newProblemBranch.FriendlyName} already exists. Skipping.");
        }
    }
    
    public static async Task SetupProblem(int year, int day) {
        // 1. Create README, Solution.cs template, and test/ folder with test1.in and test1.refout
        // 2. Parse the problem's description and write it to the README
        // 3. Fetch the input and write it to the input file
        
        
    }

    public static void WriteFile(string file, string content) {
        Console.WriteLine($"Writing {file}");
        File.WriteAllText(file, content);
    }
    
    private static List<string> GetGitRemoteNames() {
        var remoteName = Environment.GetEnvironmentVariable("AOC_GIT_REMOTE_NAME");
        return new [] { remoteName, "origin", "upstream" }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();
    }
    
    private static List<string> GetGitDefaultBranches() {
        var defaultBranch = Environment.GetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH");
        return new [] { defaultBranch, "main", "master" }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();
    }
}