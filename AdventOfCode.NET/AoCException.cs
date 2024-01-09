namespace AdventOfCode.NET;

public class AoCException(string message, Exception? ex = null) : Exception(message, ex);