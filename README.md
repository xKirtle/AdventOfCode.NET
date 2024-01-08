# Program architecture design

All things are subject to be changed during development.
___

## Commands

Two notations are used: `<argument>` and `[argument]`.

- `<argument>` means the argument is **required**
- `[argument]` means the argument is **optional**

`[-a | --argument]` might also appear, this means that the command can be called by either its short or long format.
___

### Initialize [init]

Initialize any values required by the application.
- `<session>`
  - AoC session token.
- `[-b | --branch]`
  - Default values include `main` and `master`. If you use another name, set it with this argument.
- `[-r | --remote]`
  - Default values include `origin` and `upstream`. If you use another name, set it with this argument.


### Setup [setup]

Setup an Advent Of Code problem to solve it locally. 
This will create a folder for that day that includes a `README.md` with the problem, a `Solution.cs` with entry points to solve each part of the day and an example of how you can test your code against custom tests.
- `<date>`
  - Date must be in `YYYY/DD` format. For example, `2015/03`.
- `[--no-git]`
  - Flag to choose whether you want git operations to run. If true, no dedicated branch will be created for each day and no time spent on problem will be calculated once part two of the problem is solved.

### Solve [solve]

Attempt to solve a problem's solution. Before submiting an answer to Advent of Code servers, local tests will be ran (if any is defined).
- `<date>`
  - Date must be in `YYYY/DD` format. For example, `2015/03`.

## Services

Clear plan of where each bit of logic should be and how it can be grouped in a service.
Essentially just planning the interfaces of what each service should accomplish.

___

### HTTP Service

Should handle any HTTP related request to `https://adventofcode.com`. 
A **singleton** that appends the AoC session token in its headers in each request.

Functionality includes:
- Fetching a problem
- Fetching a problem's input
- Fetching a problem's level (which part is currently being solved)
- Fetching a problem's solution in a specific level (null if not yet solved)
- Submiting a solution to a level

### Problem Service

Should handle anything problem related such as parsing a problem from an HTTP response or creating the necessary files to setup a problem.

Functionality includes:
- Parsing a problem from an HTML document
- Creating problem's files (setting a problem up)
- Setup Git operations for:
    - Starting a problem
    - Solving a problem part
    - Finishing a problem (solving all parts)
    - Calculate time spent on a problem

### Solver Service

Should handle anything related to a problem's test cases (and potentially benchmarking the solution?).

Functionality includes:
- Creating an example test case (sadly, examples cannot be easily parsed from AoC's HTML, unless we hardcode them from past years)
- Solving a problem's test cases
- Benchmarking a solution?