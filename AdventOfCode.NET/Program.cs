using System.Runtime.CompilerServices;
using AdventOfCode.NET;
using AdventOfCode.NET.Commands;
using AdventOfCode.NET.DependencyInjection;
using AdventOfCode.NET.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

[assembly: InternalsVisibleTo("AdventOfCode.NET.Tests")]

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IHttpService, HttpService>();
serviceCollection.AddSingleton<IProblemService, ProblemService>();
serviceCollection.AddSingleton<ISolverService, SolverService>();

var registrar = new TypeRegistrar(serviceCollection);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddCommand<InitCommand>("init")
        .WithDescription("Initialize environment variables.");

    config.AddCommand<SetupCommand>("setup")
        .WithDescription("Setup a new problem.");
    
    config.AddCommand<SolveCommand>("solve")
        .WithDescription("Solve a problem's test cases.");

    // Override the default exception handler and only print the exception message, assuming it's already in markup.
    config.SetExceptionHandler(ex => {
        if (ex is AoCException)
            AnsiConsole.MarkupLine(ex.Message);
        else if (ex.InnerException is AoCException)
            AnsiConsole.MarkupLine(ex.InnerException.Message);
        else
        {
            // Log all exceptions not thrown by AdventOfCode.NET
        }
        
        return 1;
    });
});

return app.Run(args);