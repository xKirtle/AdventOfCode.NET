namespace AdventOfCode.NET;

internal class AoCException(string message, Exception? ex = null) : Exception(message, ex);