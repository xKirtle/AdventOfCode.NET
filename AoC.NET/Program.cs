using AoC.NET.Commands;
using AoC.NET.DependencyInjection;
using AoC.NET.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AoC.NET;

public partial class Program
{
    public static void Main(string[] args) {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IHttpService, HttpService>();
        serviceCollection.AddSingleton<IProblemService, ProblemService>();
        serviceCollection.AddSingleton<ISolverService, SolverService>();

        var registrar = new TypeRegistrar(serviceCollection);
        var app = new CommandApp(registrar);

        app.Configure(config => {
            config.AddCommand<InitCommand>("init")
                .WithDescription("Initialize environment variables.");

            config.AddCommand<SetupCommand>("setup")
                .WithDescription("Setup a new problem.");

            config.AddCommand<SolveCommand>("solve")
                .WithDescription("Solve a problem.");
        });
        
        app.Run(args);
    }
}