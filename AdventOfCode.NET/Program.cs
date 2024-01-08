using AdventOfCode.NET.Commands;
using AdventOfCode.NET.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var serviceCollection = new ServiceCollection();
// service initialization here

var registrar = new TypeRegistrar(serviceCollection);
var app = new CommandApp(registrar);

app.Configure(config => {
    config.AddCommand<InitCommand>("init")
        .WithDescription("Initialize environment variables.");
    
    config.AddCommand<SetupCommand>("setup")
        .WithDescription("Setup a new problem.");
});
        
return app.Run(args);