using System.Diagnostics.CodeAnalysis;
using AdventOfCode.NET.Exceptions;
using AdventOfCode.NET.Utils;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Startup
{
    public static void InitializeFramework(params string[] args) {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddAdventOfCodeServices();
        serviceCollection.GetAdventOfCodeCommandApp(out var app);
        
        try {
            app.Run(args);
        }
        catch (Exception ex) {
            HandleExceptions(ex);
        }
    }

    internal static void HandleExceptions(Exception ex) {
        switch (ex) {
            case AoCSolutionException:
                throw ex;
            case AoCException or CommandRuntimeException:
                AnsiConsole.MarkupLine(ex.Message);
                break;
            default: {
                if (ex.InnerException is AoCException)
                    AnsiConsole.MarkupLine(ex.InnerException.Message);
                // else
                // Log all exceptions not thrown by AdventOfCode.NET
                break;
            }
        }
    }
}