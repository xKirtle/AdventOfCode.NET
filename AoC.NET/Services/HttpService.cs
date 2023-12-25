using System.Net;
using Spectre.Console;

namespace AoC.NET.Services;

public interface IHttpService
{
    Task<string> FetchProblem(int year, int day);
    Task<string> FetchProblemInput(int year, int day);
}

public class HttpService : IHttpService
{
    private readonly HttpClient _client;
    private readonly Uri _aocBaseAddress;
        
    public HttpService() {
        _aocBaseAddress = new Uri("https://adventofcode.com/");
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
        var requestUri = new Uri(_aocBaseAddress + $"/{year}/day/{day}");
        AnsiConsole.MarkupLine($"[green]Updating {requestUri}[/]");
        var responseMessage = await _client.GetAsync(requestUri);
        
        if (!responseMessage.IsSuccessStatusCode) {
            AnsiConsole.MarkupLine($"[red]Error downloading problem: {{problem.StatusCode}} {{problem.ReasonPhrase}}[/]");
            return null;
        }

        return await responseMessage.Content.ReadAsStringAsync();
    }
    public async Task<string> FetchProblemInput(int year, int day) {
        // TODO: Fetch problem input and parse response into a string
        
        // var input = await _client.GetAsync(requestUri + "/input");
        //
        // if (!input.IsSuccessStatusCode) {
        //     // Console.WriteLine($"Error downloading input: {input.StatusCode} {input.ReasonPhrase}", Color.Red);
        //     return null;
        // }
        
        return await (Task<string>) Task.CompletedTask;
    }

    private static string GetSessionCookie() {
        var sessionCookie = Environment.GetEnvironmentVariable("AOC_SESSION_COOKIE");
        
        if (string.IsNullOrEmpty(sessionCookie))
            throw new InvalidOperationException("AOC_SESSION_COOKIE environment variable not set");
        
        return sessionCookie;
    }
}