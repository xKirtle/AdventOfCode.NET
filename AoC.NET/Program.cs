using AoC.NET.Commands;
using AoC.NET.DependencyInjection;
using AoC.NET.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IHttpService, HttpService>();
serviceCollection.AddSingleton<IProblemService, ProblemService>();

var registrar = new TypeRegistrar(serviceCollection);
var app = new CommandApp(registrar);

app.Configure(config => {
    config.AddCommand<InitCommand>("init")
        .WithDescription("Initialize environment variables.");

    config.AddCommand<SetupCommand>("setup")
        .WithDescription("Setup a new problem.");
});
return app.Run(args);