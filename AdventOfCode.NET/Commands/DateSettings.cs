using System.ComponentModel;
using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.Commands;

public class DateSettings : CommandSettings
{
    [CommandArgument(0, "<date>")]
    [Description("The date in YYYY/DD format.")]
    public string Date { get; set; }
    
    internal int Year { get; private set; }
    internal int Day { get; private set; }

    public override ValidationResult Validate() {
        if (!Regex.IsMatch(Date, @"^\d{4}\/\d{2}$")) {
            return ValidationResult.Error("Invalid date format. Please specify the date in the YYYY/DD format.");
        }
        
        var dateParts = Date.Split('/');
        Year = int.TryParse(dateParts[0], out var year) ? year : -1;
        Day = int.TryParse(dateParts[1], out var day) ? day : -1;
        
        if (Year == -1 || Day == -1) {
            return ValidationResult.Error("You must specify both year and day.");
        }
        else if (Year < 2015 || Year > DateTime.Today.Year || Day < 1 || Day > 25) {
            return ValidationResult.Error("Invalid year or day. Please specify a year between 2015 and the current year, and a day between 1 and 25.");
        }
        
        return base.Validate();
    }
}