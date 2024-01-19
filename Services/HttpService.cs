using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Model;
using HtmlAgilityPack;
using Spectre.Console;

namespace AdventOfCode.NET.Services;

internal interface IHttpService
{
    Task<HtmlNode> FetchProblemAsync(int year, int day);
    Task<string> FetchProblemInputAsync(int year, int day);
    Task<HtmlNode> SubmitSolutionAsync(int year, int day, ProblemLevel level, string answer);
    string ParseSubmissionResponse(HtmlNode responseDocument);
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

    public async Task<HtmlNode> FetchProblemAsync(int year, int day) {
        var content = await FetchContentAsync($"{year}/day/{day}");
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(content);

        return htmlDoc.DocumentNode;
    }

    public async Task<string> FetchProblemInputAsync(int year, int day) {
        var content = await FetchContentAsync($"{year}/day/{day}/input");
        
        // Standardize to LF, then replace LF with Environment.NewLine
        return content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim();
    }

    public async Task<HtmlNode> SubmitSolutionAsync(int year, int day, ProblemLevel level, string answer) {
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
        var sessionCookie = Environment.GetEnvironmentVariable("AOC_SESSION_COOKIE", EnvironmentVariableTarget.User);

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
    
    [SuppressMessage("Performance", "SYSLIB1045:Convert to \'GeneratedRegexAttribute\'.")]
    public string ParseSubmissionResponse(HtmlNode responseDocument) {
        var sb = new StringBuilder();
        var responseNode = responseDocument.SelectSingleNode("//article//p[1]");

        if (responseNode?.ChildNodes?.Any(child => child.HasClass("day-success")) ?? false) {
            sb.Append("That's the right answer!");
            
            var daySuccessNode = responseDocument.SelectSingleNode("//article//p[2]");
            if (daySuccessNode == null) 
                return sb.ToString();
            
            var match = Regex.Match(daySuccessNode.InnerText, "You have completed Day \\d+!");
            if (match.Success)
                sb.Append(' ').Append(match.Groups[0].Value);
        }
        else {
            // Removing the [Return to Day X] link at the end of the error message
            var errorMessage = responseNode?.InnerText?.Split('[')[0].Trim() ?? "Unknown error!";

            var match = Regex.Match(errorMessage, "[Pp]lease wait (.*?) before trying again");
            if (match.Success) {
                errorMessage = errorMessage.Replace(match.Groups[1].Value, "[red]" + match.Groups[1].Value + "[/]");
            }

            match = Regex.Match(errorMessage, "have ((?:(?!to wait).)*?) left to wait");
            if (match.Success) {
                errorMessage = errorMessage.Replace(match.Groups[1].Value, "[red]" + match.Groups[1].Value + "[/]");
            }
            
            sb.Append(errorMessage);
        }
        
        return sb.Replace("  ", " ").ToString();
    }
}