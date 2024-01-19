namespace AdventOfCode.NET.Exceptions;

public class AoCSolutionException(string message, Exception innerException) : AoCException(message, innerException)
{
    public override string? StackTrace {
        get {
            if (InnerException == null) 
                return base.StackTrace;
            
            // Extract and return the stack trace up to a certain point
            var stackTrace = InnerException.StackTrace;
            var relevantStackTrace = ExtractRelevantStackTrace(stackTrace);
            return relevantStackTrace;
        }
    }

    private static string? ExtractRelevantStackTrace(string? stackTrace) {
        if (string.IsNullOrEmpty(stackTrace))
            return stackTrace;

        var lines = stackTrace.Split([Environment.NewLine], StringSplitOptions.None);
        var filteredLines = lines.TakeWhile(line => !line.Contains("AdventOfCode.NET."));
        return string.Join(Environment.NewLine, filteredLines);
    }
}
