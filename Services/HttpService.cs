using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Spectre.Console;

namespace AoC.NET.Services;

internal interface IHttpService
{
    Task<string> FetchProblem(int year, int day);
    Task<string> FetchProblemInput(int year, int day);
    Task<string?> FetchProblemLevel(int year, int day);
    Task SubmitSolution(int year, int day, string level, string answer);
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
        
        if (string.IsNullOrEmpty(aocSessionToken))
            throw new InvalidOperationException("Session token not found. Please set the AOC_SESSION_TOKEN environment variable.");
        
        handler.CookieContainer.Add(_aocBaseAddress, new Cookie("session", aocSessionToken));
    }
    
    public async Task<string> FetchProblem(int year, int day) {
        return await FetchContentAsync($"{year}/day/{day}");
    }

    public async Task<string> FetchProblemInput(int year, int day) {
        var content = await FetchContentAsync($"{year}/day/{day}/input");
        
        // Standardize to LF, then replace LF with Environment.NewLine
        return content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
    }
    
    public async Task<string?> FetchProblemLevel(int year, int day) {
        var htmlContent = await FetchProblem(year, day);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        
        return htmlDoc.DocumentNode.SelectSingleNode("//form//input[1]")?.Attributes["value"].Value;
    }

    public async Task SubmitSolution(int year, int day, string level, string answer) {
        var requestUri = new Uri(_aocBaseAddress, $"{year}/day/{day}/answer");
        var content = new FormUrlEncodedContent(new Dictionary<string, string> {
            { "level", level },
            { "answer", answer }
        });
        
        AnsiConsole.MarkupLine($"Submitting [green]{answer}[/] as the solution for problem [green]{year}/{day}[/] - [green]part {(level == "1" ? "one" : "two")}[/]");
        var responseMessage = await _client.PostAsync(requestUri, content);
        
        if (!responseMessage.IsSuccessStatusCode) {
            AnsiConsole.MarkupLine($"[red]Error submitting solution: {responseMessage.StatusCode} {responseMessage.ReasonPhrase}[/]");
            return;
        }
        
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var parsedResponse = await ParseSubmitResponseAsync(responseContent);
        
        AnsiConsole.MarkupLine($"[green]Response:[/] {parsedResponse}");
    }

    private async Task<string> ParseSubmitResponseAsync(string responseContent) {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseContent);
        
        var sb = new StringBuilder();
        var responseNode = htmlDoc.DocumentNode.SelectSingleNode("//article//p[1]");

        if (responseNode?.ChildNodes?.Any(child => child.HasClass("day-success")) ?? false) {
            sb.Append("That's the right answer!");
            
            var daySuccessNode = htmlDoc.DocumentNode.SelectSingleNode("//article//p[2]");
            if (daySuccessNode != null) {
                var match = Regex.Match(daySuccessNode.InnerText, @"You have completed Day \d+!");
                if (match.Success)
                    sb.Append(" ").Append(match.Groups[0].Value);
            }
        }
        else {
            // Removing the [Return to Day X] link at the end of the error message
            var errorMessage = responseNode?.InnerText?.Split('[')[0].Trim() ?? "Unknown error!";

            var match = Regex.Match(errorMessage, @"[Pp]lease wait (.*?) before trying again");
            if (match.Success) {
                errorMessage = errorMessage.Replace(match.Groups[1].Value, "[red]" + match.Groups[1].Value + "[/]");
            }

            match = Regex.Match(errorMessage, @"have ((?:(?!to wait).)*?) left to wait");
            if (match.Success) {
                errorMessage = errorMessage.Replace(match.Groups[1].Value, "[red]" + match.Groups[1].Value + "[/]");
            }
            
            sb.Append(errorMessage);
        }
        
        return sb.Replace("  ", " ").ToString();
    }
    
    private async Task<string> FetchContentAsync(string path) {
        var requestUri = new Uri(_aocBaseAddress + path);
        AnsiConsole.MarkupLine($"[green]Fetching from {requestUri}[/]");
        var responseMessage = await _client.GetAsync(requestUri);

        if (!responseMessage.IsSuccessStatusCode) {
            AnsiConsole.MarkupLine($"[red]Error downloading content: {responseMessage.StatusCode} {responseMessage.ReasonPhrase}[/]");
            return null;
        }

        return await responseMessage.Content.ReadAsStringAsync();
    }

    private static string GetSessionCookie() {
        var sessionCookie = Environment.GetEnvironmentVariable("AOC_SESSION_COOKIE");
        
        if (string.IsNullOrEmpty(sessionCookie))
            throw new InvalidOperationException("AOC_SESSION_COOKIE environment variable not set");
        
        return sessionCookie;
    }
}