using System.Net;
using Spectre.Console;

namespace AoC.NET.Services;

internal interface IHttpService
{
    Task<string> FetchProblem(int year, int day);
    Task<string> FetchProblemInput(int year, int day);
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
        return await FetchContentAsync($"{year}/day/{day}/input");
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