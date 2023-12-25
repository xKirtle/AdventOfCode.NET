using AoC.NET.Model;
using HtmlAgilityPack;
using Spectre.Console;

namespace AoC.NET.Services;

internal interface IProblemService
{
    Task<Problem> SetupProblem(int year, int day);
}

internal class ProblemService : IProblemService
{
    private readonly IHttpService _httpService;
    public ProblemService(IHttpService httpService) {
        _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
    }
    public async Task<Problem> SetupProblem(int year, int day) {
        var htmlContent = await _httpService.FetchProblem(year, day);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        foreach (var article in htmlDoc.DocumentNode.SelectNodes("//article")) {
            var title = article.SelectSingleNode("//h2").InnerText;
            AnsiConsole.MarkupLine($"[green]Title: {title}[/]");
        }

        return new Problem {
            Title = "Test",
            Year = year,
            Day = day, 
        };
    }
}