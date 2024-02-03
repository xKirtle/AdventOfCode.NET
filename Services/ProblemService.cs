using System.Text;
using System.Text.RegularExpressions;
using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using HtmlAgilityPack;
using LibGit2Sharp;
using Spectre.Console;

namespace AdventOfCode.NET.Services;

internal interface IProblemService
{
    Problem ParseProblem(int year, int day, HtmlNode problemNode, string problemInput);
    Task SetupProblemFiles(Problem problem);
    Task UpdateProblemFiles(Problem problem);
    void SetupGitForProblem(int year, int day);
    void TryUpdateGitForProblem(int year, int day, ProblemLevel level);
}

internal class ProblemService(IGitService gitService, IEnvironmentVariablesService envVariablesService) : IProblemService
{
    private readonly bool _verbose = envVariablesService.VerboseOutput;
    
    public Problem ParseProblem(int year, int day, HtmlNode problemNode, string problemInput) {
        // Extract logic to parse problem's markdown to its own method?
        var contentMarkdownStringBuilder = new StringBuilder();
        foreach (var article in problemNode.SelectNodes("//article")) {
            var parsedInnerHtml = ReplaceAoCRelativeUrls(article.InnerHtml)
                .Replace("<em", "<strong")
                .Replace("</em>", "</strong>");
            
            contentMarkdownStringBuilder.Append(parsedInnerHtml);
        }

        var answers = ParseProblemAnswers(problemNode);
        var level = ParseProblemLevel(problemNode);

        return new Problem(year, day, level, problemInput, answers, contentMarkdownStringBuilder.ToString());
    }

    public async Task SetupProblemFiles(Problem problem) {
        var tasks = new List<Task> {
            CreateProblemFile(problem.Year, problem.Day, "README.md", problem.ContentMarkdown),
            CreateProblemFile(problem.Year, problem.Day, "problem.in", problem.Input),
            CreateProblemFile(problem.Year, problem.Day, "Solution.cs", GetSolutionTemplate(problem.Year, problem.Day)),
            CreateProblemFile(problem.Year, problem.Day, "test.aoc", GetProblemTestTemplate, isTestFile: true)
        };

        var answers = problem.Answers.ToArray();
        if (answers.Length > 0) {
            // TODO: Reconsider design choice of keeping a problem.out, instead of the already existing test case solution
            tasks.Add(CreateProblemFile(problem.Year, problem.Day, "problem.out", string.Join(Environment.NewLine, answers)));
        }

        await Task.WhenAll(tasks);
    }

    public async Task UpdateProblemFiles(Problem problem) {
        var tasks = new List<Task> {
            CreateProblemFile(problem.Year, problem.Day, "README.md", problem.ContentMarkdown, overwrite: true),
            CreateProblemFile(problem.Year, problem.Day, "problem.in", problem.Input, overwrite: true)
        };
        
        var answers = problem.Answers.ToArray();
        if (answers.Length > 0) {
            tasks.Add(CreateProblemFile(problem.Year, problem.Day, "problem.out", string.Join(Environment.NewLine, answers), overwrite: true));
        }
        
        // we can infer at what stage the problem is solved by the number of answers present in the parsed problem
        // useful for Git commit messages
        
        // Create new test for main input for the part solved?
        
        await Task.WhenAll(tasks);
    }

    public void SetupGitForProblem(int year, int day) {
        var pathToRepo = gitService.DiscoverRepositoryPath();
        using var repo = new Repository(pathToRepo);
        
        var newProblemBranch = gitService.CreateOrGetBranch(repo, year, day, out var isNewBranch);
        
        try {
            gitService.CheckoutBranch(repo, newProblemBranch);
            
            if (isNewBranch) {
                gitService.StageProblemFiles(repo, year, day);
                var commitMessage = AoCMessages.InfoInitialGitCommitMessage(year, day);
                gitService.CommitProblemFiles(repo, commitMessage);
            
                // git reset problem folder...?
            }
            else {
                AnsiConsole.MarkupLine(AoCMessages.WarningGitProblemBranchAlreadyExists(newProblemBranch.FriendlyName));
            }
        } catch (CheckoutConflictException ex) {
            gitService.TryDeleteGitBranch(repo, newProblemBranch);
            throw new AoCException(AoCMessages.ErrorGitRepositoryNotClean, ex);
        } catch (Exception ex) {
            gitService.TryDeleteGitBranch(repo, newProblemBranch);
            throw new AoCException(AoCMessages.ErrorGitCheckoutFailed(newProblemBranch.FriendlyName), ex);
        }
        
        // Push to remote?
    }

    public void TryUpdateGitForProblem(int year, int day, ProblemLevel level) {
        var pathToRepo = gitService.DiscoverRepositoryPath();
        using var repo = new Repository(pathToRepo);
        
        // Problem setup with --no-git flag?
        if (!gitService.IsProblemBranchCheckedOut(repo, year, day)) {
            var problemBranchName = gitService.GetBranchNameOfProblem(year, day);
            AnsiConsole.MarkupLine(AoCMessages.WarningProblemGitBranchNotCheckedOut(year, day, problemBranchName));
            return;
        }
        
        gitService.StageProblemFiles(repo, year, day);
        var commitMessage = gitService.GetProblemSolvedCommitMessage(year, day, level);
        gitService.CommitProblemFiles(repo, commitMessage);

        if (level == ProblemLevel.Finished) {
            gitService.MergeProblemBranchIntoDefaultBranch(repo, repo.Head);
        }
        
        // Push to remote?
    }

    public static string GetProblemDirectory(int year, int day, bool includeTest = false) {
        var problemPath = Path.Combine(year.ToString(), $"Day{day:00}");

        if (includeTest)
            problemPath = Path.Combine(problemPath, "test");
        
        return problemPath;
    }

    private static ProblemLevel ParseProblemLevel(HtmlNode problemNode) {
        return problemNode.SelectSingleNode("//form//input[1]")?.Attributes["value"]?.Value switch {
            "1" => ProblemLevel.PartOne,
            "2" => ProblemLevel.PartTwo,
            null => ProblemLevel.Finished,
            _ => throw new AoCException(AoCMessages.ErrorProblemLevelInvalid)
        };
    }

    private static ProblemAnswers ParseProblemAnswers(HtmlNode problemNode) {
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
    
    private static string ReplaceAoCRelativeUrls(string htmlContent) {
        const string aocUrl = "https://adventofcode.com/";
        const string relativeUrlPattern = "(href|src)=\"/(?![/])(?!http:)(?!https:)(.*?)\"";
        var regex = new Regex(relativeUrlPattern, RegexOptions.IgnoreCase);
        
        return regex.Replace(htmlContent, $"$1=\"{aocUrl}$2\"");
    }

    private string GetOrCreateProblemDirectory(int year, int day, bool includeTest = false) {
        var problemPath = GetProblemDirectory(year, day, includeTest);

        if (Directory.Exists(problemPath)) 
            return problemPath;

        if (_verbose)
            AnsiConsole.MarkupLine(AoCMessages.InfoCreatingProblemDirectory(problemPath));

        Directory.CreateDirectory(problemPath);

        return problemPath;
    }
    
    private async Task<string> CreateProblemFile(int year, int day, string fileNameWithExtension, string fileContent, bool overwrite = false, bool isTestFile = false) {
        var filePath = Path.Combine(GetOrCreateProblemDirectory(year, day, isTestFile), fileNameWithExtension);

        if (File.Exists(filePath) && !overwrite) {
            AnsiConsole.Markup(AoCMessages.WarningPromptCreatingProblemFileOverriding(filePath));
            var keyInfo = AnsiConsole.Console.Input.ReadKey(true);
            AnsiConsole.MarkupLine(string.Empty);

            if (keyInfo is { Key: not ConsoleKey.Y and not ConsoleKey.Enter }) {
                AnsiConsole.MarkupLine(AoCMessages.InfoCreatingProblemFileOverridingSkipped(filePath));
                return filePath;
            }
        }

        if (_verbose)
            AnsiConsole.MarkupLine(AoCMessages.InfoCreatingProblemFile(filePath));
        
        await File.WriteAllTextAsync(filePath, fileContent, Encoding.UTF8);

        return filePath;
    }
    
    private static string GetSolutionTemplate(int year, int day) => 
        $$"""
          using AdventOfCode.NET.Model;
          using AdventOfCode.NET.Utils;
          
          // ReSharper disable CheckNamespace
          // ReSharper disable once UnusedType.Global

          namespace AdventOfCode.NET.Y{{year}}.Day{{day:00}};

          [AoCSolution({{year}}, {{day:00}})]
          public class Solution : ISolver
          {
              public object PartOne(string input) {
                  return 0;
              }
          
              public object PartTwo(string input) {
                  return 0;
              }
          }

          """;
    
    public static string GetProblemTestTemplate => 
        """
        Part: one/two
        Input:
        # Your test input goes here
        # and also here, if multiline
        Output:
        # Your expected output goes here
        """;
}