namespace AdventOfCode.NET.Model;

public interface ISolver
{
    public object PartOne(string input);
    public object PartTwo(string input);
    
    public static ISolver GetSolverInstance(int year, int day) {
        Type? foundType = null;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes()))
        {
            // Ignore classes that don't implement ISolver
            if (!typeof(ISolver).IsAssignableFrom(type) || type.IsAbstract)
                continue;
            
            var attribute = type.GetCustomAttributes(typeof(AoCSolutionAttribute), false).FirstOrDefault() as AoCSolutionAttribute;
            if (attribute == null || attribute.Year != year || attribute.Day != day)
                continue;

            if (foundType != null)
                throw new AoCException(AoCMessages.ErrorMultipleProblemsFound(year, day));
            
            foundType = type;
        }

        if (foundType == null)
            throw new AoCException(AoCMessages.ErrorNoProblemFound(year, day));
        
        ISolver solverInstance;
        
        try {
            solverInstance = (Activator.CreateInstance(foundType) as ISolver)!;
        }
        catch (Exception ex) {
            throw new AoCException(AoCMessages.ErrorSolutionInstantiationFailed(year, day), ex);
        }

        return solverInstance;
    }
}