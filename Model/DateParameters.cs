using System.Text.RegularExpressions;
using Cocona;

namespace AdventOfCode.NET.Model;

public class DateParameters : ICommandParameterSet
{
    public int Year { get; }
    public int Day { get; }
    
    public DateParameters([Argument] string date = "today") {
        if (date == "today") {
            (Year, Day) = (DateTime.Today.Year, DateTime.Today.Day);
            return;
        }

        if (!Regex.IsMatch(date, @"^\d{4}/\d{1,2}$")) {
            Console.Error.WriteLine("Error: Invalid date format. Please use 'YYYY/DD'.");
            Environment.Exit(1);
        }
        
        var dateParts = date.Split('/');
        (Year, Day) = (int.Parse(dateParts[0]), int.Parse(dateParts[1]));
        
        if (Year < 2015 || Year > DateTime.Today.Year || Day < 1 || Day > 31) {
            Console.Error.WriteLine("Error: Invalid date parameters. The year must be between 2015 and the current year, and the day must be between 1 and 31.");
            Environment.Exit(1);
        }
    }
}