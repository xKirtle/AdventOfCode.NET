using System.Net;
using AdventOfCode.NET.Model;
using HtmlAgilityPack;
using Spectre.Console;

namespace AdventOfCode.NET.Services;

internal interface IHttpService
{
    Task<HtmlNode> FetchProblem(int year, int day);
    Task<string[]> FetchProblemInput(int year, int day);
    ProblemLevel FetchProblemLevel(HtmlNode problemNode);
    ProblemAnswers FetchProblemAnswers(HtmlNode problemNode);
    Task<HtmlNode> SubmitSolution(int year, int day, ProblemLevel level, string answer);
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

    public ProblemLevel FetchProblemLevel(HtmlNode problemNode) {
        return problemNode.SelectSingleNode("//form//input[1]")?.Attributes["value"]?.Value switch {
            "1" => ProblemLevel.PartOne,
            "2" => ProblemLevel.PartTwo,
            null => ProblemLevel.Finished,
            _ => throw new AoCException(AoCMessages.ErrorProblemInputInvalid)
        };
    }

    public ProblemAnswers FetchProblemAnswers(HtmlNode problemNode) {
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

    public async Task<HtmlNode> SubmitSolution(int year, int day, ProblemLevel level, string answer) {
        var requestUri = new Uri(_aocBaseAddress, $"{year}/day/{day}/answer");

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        var problemLevel = level switch {
            ProblemLevel.PartOne => "1",
            ProblemLevel.PartTwo => "2",
            _ => throw new AoCException(AoCMessages.WarningSubmittingSolutionInvalidLevel(level.ToString()))
        };

        var content = new FormUrlEncodedContent(new Dictionary<string, string> {
            { "level", problemLevel },
            { "answer", answer }
        });

        HttpResponseMessage? response = null;
        
        try {
            response = await _client.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
                throw new AoCException(AoCMessages.HttpErrorSubmittingSolution(requestUri.ToString(), 
                    (int)response.StatusCode, response.ReasonPhrase));
        }
        catch (Exception ex) {
            if (response == null)
                throw new AoCException(AoCMessages.HttpErrorSubmittingSolution(requestUri.ToString()), ex);
            
            throw new AoCException(AoCMessages.HttpErrorSubmittingSolution(requestUri.ToString(), 
                (int)response.StatusCode, response.ReasonPhrase), ex);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseContent);
        
        return htmlDoc.DocumentNode;
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

        HttpResponseMessage? response = null;

        try {
            response = await _client.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                throw new AoCException(AoCMessages.HttpErrorFetchingContent(requestUri.ToString(), 
                    (int)response.StatusCode, response.ReasonPhrase));
        }
        catch (Exception ex) {
            if (response == null)
                throw new AoCException(AoCMessages.HttpErrorFetchingContent(requestUri.ToString()), ex);
            
            throw new AoCException(AoCMessages.HttpErrorFetchingContent(requestUri.ToString(), 
                (int)response.StatusCode, response.ReasonPhrase), ex);
        }

        return await response.Content.ReadAsStringAsync();
    }
}