using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using AoC.NET.Model;
using HtmlAgilityPack;
using LibGit2Sharp;
using Spectre.Console;

namespace AoC.NET.Services;

internal interface IProblemService
{
    Task<Problem> FetchAndParseProblem(int year, int day);
    Task CreateProblemFiles(Problem problem);
    void SetupGitForProblem(int year, int day);
}

internal class ProblemService : IProblemService
{
    private readonly IHttpService _httpService;
    private readonly ISolverService _solverService;
    
    public ProblemService(IHttpService httpService, ISolverService solverService) {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
        _solverService = solverService ?? throw new ArgumentNullException(nameof(solverService));
    }

    public async Task<Problem> FetchAndParseProblem(int year, int day) {
        // TODO: Ditch Problem class entirely? It's only used for the README.md file and the input file. Both of which could be generated without it.
        // TODO: Maybe just return a tuple of (string, string) for the README.md and input file content?
        
        var htmlContent = await _httpService.FetchProblem(year, day);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var problem = new Problem() {
            Year = year,
            Day = day
        };

        problem.Title = htmlDoc.DocumentNode.SelectSingleNode("//article[1]//h2").InnerText;
        problem.Input = await _httpService.FetchProblemInput(year, day);
        
        foreach (var article in htmlDoc.DocumentNode.SelectNodes("//article")) {
            problem.ContentMd += article.InnerHtml.Replace("<em", "<strong").Replace("</em>", "</strong>");
        }
        
        return problem;
    }
    
    public async Task CreateProblemFiles(Problem problem) {
        var tasks = new List<Task>() {
            CreateFileAsync(problem.Year, problem.Day, "README.md", problem.ContentMd),
            CreateFileAsync(problem.Year, problem.Day, "Solution.cs", GetSolutionTemplate(problem)),
            CreateFileAsync(problem.Year, problem.Day, "input.aoc", problem.Input),
            CreateProblemTestTemplate(problem.Year, problem.Day)
        };
        
        await Task.WhenAll(tasks);
    }
    public void SetupGitForProblem(int year, int day) {
        using var repo = new Repository(".git");
        var defaultBranch = repo.Branches[GetGitDefaultBranches().FirstOrDefault(x => repo.Branches[x] != null)];
        
        if (defaultBranch == null)
            throw new InvalidOperationException("Could not find a default branch of the repository. If you're using something other than 'main' or 'master', please set the AOC_GIT_DEFAULT_BRANCH environment variable with its name.");
        
        var newProblemBranch = repo.Branches[$"problems/Y{year}/D{day}"];
        var branchExists = newProblemBranch != null;
        newProblemBranch ??= repo.Branches.Add($"problems/Y{year}/D{day}", defaultBranch.Tip, allowOverwrite: true);
        LibGit2Sharp.Commands.Checkout(repo, newProblemBranch);

        if (!branchExists) {
            LibGit2Sharp.Commands.Stage(repo, year.ToString());
            var author = repo.Config.BuildSignature(DateTimeOffset.Now);

            if (author == null) {
                var msg = "Author information not found in global Git configuration. Please set your name and email with 'git config --global user.name \"Your Name\"' and 'git config --global user.email \"";
                throw new InvalidOperationException(msg);
            }
            
            repo.Commit($"Initial commit for Y{year}D{day}", author, author);
            repo.Tags.Add($"Y{year}D{day}", repo.Head.Tip);
        }
        else {
            AnsiConsole.MarkupLine($"[yellow]Branch {newProblemBranch.FriendlyName} already exists. Skipping git setup.[/]");
        }
        
        OpenJetBrainsRider([$"{year}/Day{day:00}/README.md", $"{year}/Day{day:00}/Solution.cs", $"{year}/Day{day:00}/test/test.aoc"]);
    }
    
    private string GetOrCreateProblemPath(int year, int day, bool includeTest = false) {
        var folder = Path.Combine(year.ToString(), $"Day{day:00}");

        if (includeTest)
            folder = Path.Combine(folder, "test");
            
        if (!Directory.Exists(folder)) {
            AnsiConsole.MarkupLine($"[green]Creating {folder}[/]");
            Directory.CreateDirectory(folder);
        }

        return folder;
    }
    
    private static void OpenJetBrainsRider(string[] args) 
    {
        string riderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Rider\bin\rider64.exe");
        string arguments = string.Join(" ", args);

        Process.Start(riderPath, arguments).WaitForExit();
    }

    private async Task CreateFileAsync(int year, int day, string fileNameWithExtension, string content) {
        var file = Path.Combine(GetOrCreateProblemPath(year, day), fileNameWithExtension);
        AnsiConsole.MarkupLine($"[green]Writing {file}[/]");
        await File.WriteAllTextAsync(file, content, Encoding.UTF8);
    }

    private async Task CreateProblemTestTemplate(int year, int day) {
        var sb = new StringBuilder()
            .AppendLine($"Part: [one/two]")
            .AppendLine("Input:")
            .AppendLine("# Your test input goes here")
            .AppendLine("# and also here, if multiline")
            .AppendLine("Output:")
            .AppendLine("# Your expected output goes here");
        
        // Ensure test path exists
        GetOrCreateProblemPath(year, day, includeTest: true);
        await CreateFileAsync(year, day, "test/test.aoc", sb.ToString());
    }

    private string GetSolutionTemplate(Problem problem) {
        return $@"using AoC.NET.Model;

namespace AoC.NET.Problems.Y{problem.Year}.Day{problem.Day:00};

[AoCSolution({problem.Year}, {problem.Day})]
public class Solution : ISolver
{{
    public object PartOne(string input) {{
        return 0;
    }}

    public object PartTwo(string input) {{
        return 0;
    }}
}}
";
    }
    
    private static List<string> GetGitRemoteNames() {
        var remoteName = Environment.GetEnvironmentVariable("AOC_GIT_REMOTE_NAME");

        if (!string.IsNullOrEmpty(remoteName))
            return new List<string> { remoteName };

        return new List<string> { "origin", "upstream" };
    }
    
    private static List<string> GetGitDefaultBranches() {
        var defaultBranch = Environment.GetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH");
        
        if (!string.IsNullOrEmpty(defaultBranch))
            return new List<string> { defaultBranch };

        return new List<string> { "main", "master" };
    }
}