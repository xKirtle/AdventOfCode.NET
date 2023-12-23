using System;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using System.Drawing;
using AdventOfCode.NET.Model;

[DebuggerStepThrough]
internal class AOCHttpClient
{
    private static readonly HttpClient Client;
    private static Uri AOCBaseAddress => new Uri("https://adventofcode.com/");

    static AOCHttpClient() {
        var handler = new HttpClientHandler {
            CookieContainer = new CookieContainer()
        };

        Client = new HttpClient(handler);
        var aocSessionToken = GetSessionCookie();
        
        if (string.IsNullOrEmpty(aocSessionToken))
            throw new InvalidOperationException("Session token not found. Please set the AOC_SESSION_TOKEN environment variable.");
        
        handler.CookieContainer.Add(AOCBaseAddress, new Cookie("session", aocSessionToken));
    }
    
    private static string GetSessionCookie() {
        var sessionCookie = Environment.GetEnvironmentVariable("AOC_SESSION_COOKIE");
        
        if (string.IsNullOrEmpty(sessionCookie))
            throw new InvalidOperationException("AOC_SESSION_COOKIE environment variable not set");
        
        return sessionCookie;
    }

    public static async Task<Problem> DownloadProblem(int year, int day) {
        var requestUri = new Uri(AOCBaseAddress, $"/{year}/day/{day}");
        Console.WriteLine("Updating " + requestUri, Color.Green);
        var problem = await Client.GetAsync(requestUri);
        
        if (!problem.IsSuccessStatusCode) {
            Console.WriteLine($"Error downloading problem: {problem.StatusCode} {problem.ReasonPhrase}", Color.Red);
            return null;
        }

        var input = await Client.GetAsync(requestUri + "/input");
        
        if (!input.IsSuccessStatusCode) {
            Console.WriteLine($"Error downloading input: {input.StatusCode} {input.ReasonPhrase}", Color.Red);
            return null;
        }
        
        // TODO: Parse problem description
        
        return await (Task<Problem>) Task.CompletedTask;
    }
}
