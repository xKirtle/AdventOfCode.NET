# AdventOfCode.NET

All things in this document are subject to change during development.

## How to setup

Simply create a new .NET Core console application, import the `AdventOfCode.NET` NuGet package and add the following to your entry point:

```csharp
using AdventOfCode.NET;
Startup.InitializeFramework(args);
```

Then, you'll have to call the `init` command to initialize the application with your Advent of Code session token.
You can do so by executing `dotnet run init <session>`. For example, `dotnet run init 53616c...f37200`.

## Commands

From your terminal, call `dotnet run` to get a list of commands available. This should print something like this:

```
USAGE:
    [YourProject].dll [OPTIONS] <COMMAND>

OPTIONS:
    -h, --help       Prints help information   
    -v, --version    Prints version information

COMMANDS:
    init <session>    Initialize environment variables
    setup <date>      Setup a new problem
    solve <date>      Solve a problem's test cases
```

___

Two notations are used: `<argument>` and `[argument]`.

- `<argument>` means the argument is **required**
- `[argument]` means the argument is **optional**

`[-a | --argument]` might also appear, this means that the command can be called by either its short or long format.

### Initialize [init]

Initialize any values required by the application.
- `<session>`
  - AoC session token.
- `[-b | --branch]`
  - Defines your default branch's name. Default value is `master`. If you use another name, set it with this argument.
- `[--no-git]`
  - Flag to choose whether you want git operations to run. If true, no dedicated branch will be created for each day and no time spent on problem will be calculated once part two of the problem is solved.
- `[--silent]`
  - Flag to choose whether you want to see a lot of verbosity in the output of any command. If true, only key information will be printed. (Mostly for debug purposes).


### Setup [setup]

Setup an Advent Of Code problem to solve it locally. 
This will create a folder for that day that includes a `README.md` with the problem, a `Solution.cs` with entry points to solve each part of the day and an example of how you can test your code against custom tests.
- `<date>`
  - Date must be in `YYYY/DD` format. For example, `2015/03`.
- `[--no-git]`
  - Flag to choose whether you want git operations to run. If true, no dedicated branch will be created for each day and no time spent on problem will be calculated once part two of the problem is solved.

### Solve [solve]

Attempt to solve a problem's solution. Before submitting an answer to Advent of Code servers, local tests will be ran (if any is defined).
- `<date>`
  - Date must be in `YYYY/DD` format. For example, `2015/03`.

## How to debug your code

Since this library is designed to be used through a CLI, you'll have to modify your IDE's run configuration to pass the required arguments to the application. 

For example, in JetBrains Rider, you can set up a run configuration and set the arguments in the `Run/Debug Configurations` window.

<img src="https://i.imgur.com/n3YTf92.png" alt="asd"/>

Alternatively, you can create a `launchSettings.json` file for your project and set the arguments there. This file should look like this:

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "YourProjectName": {
      "commandName": "Project",
      "commandLineArgs": "solve 2024/01"
      // ...
    }
  }
}

```