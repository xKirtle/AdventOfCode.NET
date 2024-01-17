using AdventOfCode.NET.Commands;
using AdventOfCode.NET.DependencyInjection;
using AdventOfCode.NET.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AdventOfCode.NET;

public static class AoCExtensions
{
    public static IServiceCollection AddAdventOfCodeServices(this IServiceCollection services) {
        services.AddSingleton<IHttpService, HttpService>();
        services.AddSingleton<IProblemService, ProblemService>();
        services.AddSingleton<ISolverService, SolverService>();

        return services;
    }
    
    public static IServiceCollection GetAdventOfCodeCommandApp(this IServiceCollection serviceCollection, out ICommandApp app) {
        var registrar = new TypeRegistrar(serviceCollection);
        app = new CommandApp(registrar);

        app.Configure(config => {
            config.AddCommand<InitCommand>("init")
                .WithDescription("Initialize environment variables.");

            config.AddCommand<SetupCommand>("setup")
                .WithDescription("Setup a new problem.");
    
            config.AddCommand<SolveCommand>("solve")
                .WithDescription("Solve a problem's test cases.");

            config.PropagateExceptions();
            
            // Reminder: If exceptions are set to propagate, this method will never be called
            config.SetExceptionHandler(Startup.HandleExceptions);
        });

        return serviceCollection;
    }
    
}