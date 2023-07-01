using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FPLedit.GTFS.Model;

public readonly struct GtfsDays
{
    public bool IsRange { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateOnly[] IrregularDays { get; init; }

    private static readonly Regex rangeRegex = new(@"^(\d{4}-\d{2}-\d{2})--(\d{4}-\d{2}-\d{2})$");
    private static readonly Regex singleDayRegex = new(@"^(?:(\d{4}-\d{2}-\d{2}),)+(\d{4}-\d{2}-\d{2})?$");
    public static Regex FullDateRegex { get; } = new($"{rangeRegex}|{singleDayRegex}");

    public static GtfsDays? Parse(string days)
    {
        const string dateFormat = "yyyy-MM-dd";

        var rangeMatch = rangeRegex.Match(days);
        if (rangeMatch.Success)
        {
            var d1 = DateOnly.ParseExact(rangeMatch.Groups[1].ValueSpan, dateFormat);
            var d2 = DateOnly.ParseExact(rangeMatch.Groups[2].ValueSpan, dateFormat);
            
            return new GtfsDays { StartDate = d1, EndDate = d2, IsRange = true, IrregularDays = Array.Empty<DateOnly>() };
        }

        var singleMatch = singleDayRegex.Match(days);
        if (singleMatch.Success)
        {
            var dates = singleMatch.Groups.Values.Skip(1)
                .Select(g => DateOnly.ParseExact(g.ValueSpan, dateFormat)).ToArray();
            return new GtfsDays { StartDate = dates.Min(), EndDate = dates.Max(), IsRange = false, IrregularDays = dates };
        }

        return null;
    }

    public static GtfsDays Empty => new (){ StartDate = DateOnly.MinValue, EndDate = DateOnly.MaxValue, IsRange = true, IrregularDays = Array.Empty<DateOnly>() };
}