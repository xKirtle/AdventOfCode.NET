namespace AdventOfCode.NET.Exceptions;

public class AoCException(string message, Exception? ex = null) : Exception(message, ex);