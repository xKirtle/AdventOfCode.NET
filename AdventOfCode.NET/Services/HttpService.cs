using System.Net;
using AdventOfCode.NET.Model;
using HtmlAgilityPack;
using Spectre.Console;

namespace AdventOfCode.NET.Services;

internal interface IHttpService
{
    Task<HtmlNode> FetchProblem(int year, int day);
    Task<string[]> FetchProblemInput(int year, int day);
    Task<ProblemLevel> FetchProblemLevel(int year, int day);
    ProblemLevel FetchProblemLevel(HtmlNode problemNode);
    Task<string?> FetchProblemSolution(int year, int day, ProblemLevel level);
    string? FetchProblemSolution(HtmlNode problemNode, ProblemLevel level);
    Task SubmitSolution(int year, int day, ProblemLevel level, string answer);
}

internal class HttpService : IHttpService
{
    private readonly HttpClient _client;
    private readonly Uri _aocBaseAddress;
    
    public HttpService() {
        _aocBaseAddress = new Uri("https://adventofcode.com");
        var handler = new HttpClientHandler {
            CookieContainer = new CookieContainer()
        };

        _client = new HttpClient(handler);
        var aocSessionToken = GetSessionCookie();
        
        handler.CookieContainer.Add(_aocBaseAddress, new Cookie("session", aocSessionToken));
    }
    
    public async Task<HtmlNode> FetchProblem(int year, int day) {
        var content = await FetchContentAsync($"{year}/day/{day}");
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(content);

        return htmlDoc.DocumentNode;
    }

    public async Task<string[]> FetchProblemInput(int year, int day) {
        var content = await FetchContentAsync($"{year}/day/{day}/input");
        return content.Split(["\r\n", "\n"], StringSplitOptions.None);
    }

    public async Task<ProblemLevel> FetchProblemLevel(int year, int day) {
        var problemNode = await FetchProblem(year, day);
        return FetchProblemLevel(problemNode);
    }

    public ProblemLevel FetchProblemLevel(HtmlNode problemNode) {
        return problemNode.SelectSingleNode("//form//input[1]")?.Attributes["value"].Value switch {
            "1" => ProblemLevel.PartOne,
            "2" => ProblemLevel.PartTwo,
            _ => ProblemLevel.Finished
        };
    }

    // TODO: Instead of specifying the level, we should just retrieve all the answers present on the page
    public async Task<string?> FetchProblemSolution(int year, int day, ProblemLevel level) {
        var problemNode = await FetchProblem(year, day);
        return FetchProblemSolution(problemNode, level);
    }

    public string? FetchProblemSolution(HtmlNode problemNode, ProblemLevel level)
    {
        return null;
    }

    public Task SubmitSolution(int year, int day, ProblemLevel level, string answer)
    {
        throw new NotImplementedException();
    }
    
    private static string GetSessionCookie() {
        var sessionCookie = Environment.GetEnvironmentVariable("AOC_SESSION_COOKIE");

        if (string.IsNullOrEmpty(sessionCookie))
            throw new AoCException(AoCMessages.ErrorSessionTokenNotFound);
        
        return sessionCookie;
    }
    
    private async Task<string> FetchContentAsync(string relativeUri) {
        var requestUri = new Uri(_aocBaseAddress, relativeUri);
        AnsiConsole.MarkupLine(AoCMessages.InfoFetchingContent(requestUri.ToString()));
        
        var response = await _client.GetAsync(requestUri);
        
        if (!response.IsSuccessStatusCode)
            throw new AoCException(AoCMessages.ErrorFetchingContent(requestUri.ToString(), (int)response.StatusCode, response.ReasonPhrase));
        
        return await response.Content.ReadAsStringAsync();
    }
}