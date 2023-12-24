using AoC.NET.Commands;
using AoC.NET.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IHttpService, HttpService>();
var serviceProvider = serviceCollection.BuildServiceProvider();

var app = new CommandApp();
app.Configure(config => {
    config.AddCommand<InitCommand>("setup")
        .WithDescription("Setup a new problem")
        .WithExample("setup", "2020", "15")
        .WithExample("setup", "2020", "15", "--no-git", "true");
});
return app.Run(args);