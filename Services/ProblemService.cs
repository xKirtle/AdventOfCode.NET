﻿using System.Text;
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
}

internal class ProblemService(IGitService gitService) : IProblemService
{
    public Problem ParseProblem(int year, int day, HtmlNode problemNode, string problemInput) {
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
        
        // Create new test for main input for the part solved?
        
        await Task.WhenAll(tasks);
    }

    public void SetupGitForProblem(int year, int day) {
        var pathToRepo = gitService.DiscoverRepositoryPath();
        using var repo = new Repository(pathToRepo);
        
        var newProblemBranchName = $"problem/{year}/day/{day}";
        var newProblemBranch = gitService.CreateOrGetBranch(repo, newProblemBranchName, out var isNewBranch);
        gitService.CheckoutBranch(repo, newProblemBranch);

        if (isNewBranch) {
            gitService.StageAndCommitNewProblem(repo, year, day, newProblemBranch);
        }
        else {
            AnsiConsole.MarkupLine(AoCMessages.WarningGitProblemBranchAlreadyExists(newProblemBranchName));
        }
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

    private static string GetOrCreateProblemDirectory(int year, int day, bool includeTest = false) {
        var problemPath = GetProblemDirectory(year, day, includeTest);
        
        // ReSharper disable once InvertIf
        if (!Directory.Exists(problemPath)) {
            AnsiConsole.MarkupLine(AoCMessages.InfoCreatingProblemDirectory(problemPath));
            Directory.CreateDirectory(problemPath);
        }

        return problemPath;
    }
    
    private static async Task<string> CreateProblemFile(int year, int day, string fileNameWithExtension, string fileContent, bool overwrite = false, bool isTestFile = false) {
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