using LibGit2Sharp;

internal static class AOCUtils
{
    private static async Task SetupProblemWithGit(int year, int day) {
        // using var repo = new Repository(".git");
        // var remote = repo.Network.Remotes["origin"];
        // var branch = repo.Branches[$"problems/Y{year}/D{day}"];
        // var branchExists = branch != null;
        //
        // branch ??= repo.Branches.Add($"problems/Y{year}/D{day}", $"origin/problems/Y{year}/D{day}", allowOverwrite: true);
        // Commands.Checkout(repo, branch);
        //
        // if (!branchExists) {
        //     // ...
        // }
    }
    
    public static async Task SetupProblem(int year, int day, bool useGit = false) {
        if (useGit)
            await SetupProblemWithGit(year, day);
        
        //...
    }
}