# First time setup

To get started with this framework, all you need to do is execute one command.

`
dotnet run init <session> -r|--remote [remote] -b|--branch [branch]
`

Where session is your AoC session token. You also have two optional arguments:
- `[remote]` is your default remote path. This value is usually `origin` but if you use something else, you can specify it with it.
- `[branch]` is your default branch name. This value is usually `main|master` but you can also set it to whatever you use.