using System.Text;
using AdventOfCode.NET.Model;
using HtmlAgilityPack;
using Spectre.Console;

namespace AdventOfCode.NET.Services;

internal interface IProblemService
{
    Problem ParseProblem(int year, int day, HtmlNode problemNode, string[] problemInput);
    Task SetupProblemFiles(Problem problem);
    void SetupGitForProblem(int year, int day);
    ProblemLevel ParseProblemLevel(HtmlNode problemNode);
    ProblemAnswers ParseProblemAnswers(HtmlNode problemNode);
}

internal class ProblemService : IProblemService
{
    public Problem ParseProblem(int year, int day, HtmlNode problemNode, string[] problemInput) {
        // Extract logic to parse problem's markdown to its own method?
        var contentMarkdownStringBuilder = new StringBuilder();
        foreach (var article in problemNode.SelectNodes("//article")) {
            contentMarkdownStringBuilder.Append(article.InnerHtml
                .Replace("<em", "<strong")
                .Replace("</em>", "</strong>"));
        }

        var answers = ParseProblemAnswers(problemNode);
        var level = ParseProblemLevel(problemNode);

        return new Problem(year, day, level, problemInput, answers, contentMarkdownStringBuilder.ToString());
    }

    public async Task SetupProblemFiles(Problem problem)
    {
        var tasks = new List<Task> {
            CreateProblemFile(problem.Year, problem.Day, "README.md", problem.ContentMarkdown),
            CreateProblemFile(problem.Year, problem.Day, "input.aoc", string.Join(Environment.NewLine, problem.Input))
        };

        await Task.WhenAll(tasks);
    }

    public void SetupGitForProblem(int year, int day)
    {
        throw new NotImplementedException();
    }
    
    public ProblemLevel ParseProblemLevel(HtmlNode problemNode) {
        return problemNode.SelectSingleNode("//form//input[1]")?.Attributes["value"]?.Value switch {
            "1" => ProblemLevel.PartOne,
            "2" => ProblemLevel.PartTwo,
            null => ProblemLevel.Finished,
            _ => throw new AoCException(AoCMessages.ErrorProblemLevelInvalid)
        };
    }

    public ProblemAnswers ParseProblemAnswers(HtmlNode problemNode) {
        var articleNodes = problemNode.SelectNodes("//main//article");

        if (articleNodes == null)
            throw new AoCException(AoCMessages.ErrorProblemNodeNotFound);

        var answers = articleNodes
            .Select(articleNode => articleNode.SelectSingleNode("following-sibling::p"))
            .Select(siblingParagraphNode => siblingParagraphNode?.SelectSingleNode(".//code"))
            .OfType<HtmlNode>()
            .Select(codeNode => codeNode.InnerText.Trim())
            .ToList();

        return new ProblemAnswers(
            answers.ElementAtOrDefault(0),
            answers.ElementAtOrDefault(1)
        );
    }
    
    public static string GetOrCreateProblemDirectory(int year, int day) {
        var problemPath = Path.Combine(year.ToString(), $"Day{day:00}");

        // ReSharper disable once InvertIf
        if (!Directory.Exists(problemPath)) {
            AnsiConsole.MarkupLine(AoCMessages.InfoCreatingProblemDirectory(problemPath));
            Directory.CreateDirectory(problemPath);
        }

        return problemPath;
    }
    
    public static async Task<string> CreateProblemFile(int year, int day, string fileNameWithExtension, string fileContent) {
        var filePath = Path.Combine(GetOrCreateProblemDirectory(year, day), fileNameWithExtension);

        if (File.Exists(filePath)) {
            AnsiConsole.Markup(AoCMessages.WarningPromptCreatingProblemFileOverriding(filePath));
            var keyInfo = AnsiConsole.Console.Input.ReadKey(true);
            
            if (keyInfo != null && keyInfo.Value.Key != ConsoleKey.Y)
                throw new AoCException(AoCMessages.InfoCreatingProblemFileOverridingSkipped(filePath));
        }

        AnsiConsole.MarkupLine(AoCMessages.InfoCreatingProblemFile(filePath));
        await File.WriteAllTextAsync(filePath, fileContent, Encoding.UTF8);

        return filePath;
    }
}