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
    string GetOrCreateProblemPath(int year, int day, bool includeTest = false);
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
            var exampleInput = Regex.Match(article.InnerHtml, @"<pre><code>([\s\S]*?)<\/code><\/pre>").Groups[1].Value;
            var exampleResult = Regex.Match(article.InnerHtml, @"<code><em>(.*?)<\/em><\/code>").Groups[1].Value;
            problem.Examples.Add((exampleInput, exampleResult));

            // TODO: Need to also parse part 1 (do I need part 2 as well?) result if already solved
            // Maybe include the <p> between articles containing the correct answer for the previous part?
            
            problem.ContentMd += article.InnerHtml;
        }
        
        return problem;
    }
    
    public async Task CreateProblemFiles(Problem problem) {
        var tasks = new List<Task>() {
            CreateFileAsync(problem, "README.md", problem.ContentMd),
            CreateFileAsync(problem, "Solution.cs", GetSolutionTemplate(problem)),
            CreateFileAsync(problem, "input.aoc", problem.Input),
            CreateProblemTests(problem)
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
            
            OpenJetBrainsRider([$"{year}/Day{day:00}/README.md", $"{year}/Day{day:00}/Solution.cs", $"{year}/Day{day:00}/test/test1.aoc"]);
        }
        else {
            AnsiConsole.MarkupLine($"[yellow]Branch {newProblemBranch.FriendlyName} already exists. Skipping git setup.[/]");
        }
    }
    
    public string GetOrCreateProblemPath(int year, int day, bool includeTest = false) {
        var folder = Path.Combine(year.ToString(), $"Day{day:00}");

        if (includeTest)
            folder = Path.Combine(folder, "test");
            
        if (!Directory.Exists(folder)) {
            AnsiConsole.MarkupLine($"[green]Creating {folder}[/]");
            Directory.CreateDirectory(folder);
        }

        return folder;
    }
    
    private static void OpenJetBrainsRider(ReadOnlySpan<string> args) 
    {
        string riderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Rider\bin\rider64.exe");
        string arguments = string.Join(" ", args.ToArray());

        Process.Start(riderPath, arguments).WaitForExit();
    }

    private async Task CreateFileAsync(Problem problem, string filename, string content) {
        var file = Path.Combine(GetOrCreateProblemPath(problem.Year, problem.Day), filename);
        AnsiConsole.MarkupLine($"[green]Writing {file}[/]");
        await File.WriteAllTextAsync(file, content, Encoding.UTF8);
    }

    private async Task CreateProblemTests(Problem problem) {
        for (int i = 0; i < problem.Examples.Count; i++) {
            var (input, output) = problem.Examples[i];
            await _solverService.CreateTest(problem.Year, problem.Day, $"test{i + 1}.aoc", input, output);
        }
    }

    private string GetSolutionTemplate(Problem problem) {
        return $@"using AoC.NET.Model;

namespace AoC.NET.Problems.Y{problem.Year}.Day{problem.Day:00};

[AocSolution({problem.Year}, {problem.Day})]
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
    
    // TODO: Maybe don't hardcode these and simply rely on the user's environment variables?
    private static List<string> GetGitRemoteNames() {
        var remoteName = Environment.GetEnvironmentVariable("AOC_GIT_REMOTE_NAME");
        return new List<string> { remoteName, "origin", "upstream" }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();
    }
    
    private static List<string> GetGitDefaultBranches() {
        var defaultBranch = Environment.GetEnvironmentVariable("AOC_GIT_DEFAULT_BRANCH");
        return new List<string> { defaultBranch, "main", "master" }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();
    }
}