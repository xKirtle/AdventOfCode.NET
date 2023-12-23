using System;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using AdventOfCode.NET.Model;

[DebuggerStepThrough]
public class AOCHttpClient
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

    internal static Task<Problem> DownloadProblem(int year, int day) {
        
        
        return (Task<Problem>) Task.CompletedTask;
    }
}
