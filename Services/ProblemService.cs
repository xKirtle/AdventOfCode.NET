using System.Text;
using System.Text.RegularExpressions;
using AoC.NET.Model;
using HtmlAgilityPack;
using Spectre.Console;

namespace AoC.NET.Services;

internal interface IProblemService
{
    Task<Problem> FetchAndParseProblem(int year, int day);
    Task CreateProblemFiles(Problem problem);
}

internal class ProblemService : IProblemService
{
    private readonly IHttpService _httpService;
    private readonly ITestRunnerService _testRunnerService;
    
    public ProblemService(IHttpService httpService, ITestRunnerService testRunnerService) {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
        _testRunnerService = testRunnerService ?? throw new ArgumentNullException(nameof(testRunnerService));
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
    
    private string GetOrCreateProblemPath(Problem problem, bool includeTest = false) {
        var folder = Path.Combine(problem.Year.ToString(), $"Day{problem.Day:00}");

        if (includeTest)
            folder = Path.Combine(folder, "test");
            
        if (!Directory.Exists(folder)) {
            AnsiConsole.MarkupLine($"[green]Creating {folder}[/]");
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    private async Task CreateFileAsync(Problem problem, string filename, string content) {
        var file = Path.Combine(GetOrCreateProblemPath(problem), filename);
        AnsiConsole.MarkupLine($"[green]Writing {file}[/]");
        await File.WriteAllTextAsync(file, content, Encoding.UTF8);
    }

    private async Task CreateProblemTests(Problem problem) {
        for (int i = 0; i < problem.Examples.Count; i++) {
            var (input, output) = problem.Examples[i];
            var filePath = Path.Combine(GetOrCreateProblemPath(problem, includeTest: true), $"test{i + 1}.aoc");
            await _testRunnerService.CreateTest(input, output, filePath);
        }
    }

    private string GetSolutionTemplate(Problem problem) {
        return $@"using AoC.NET.Model;

namespace AoC.NET.Problems.Y{problem.Year}.Day{problem.Day:00};

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
}