using System.Text.RegularExpressions;

namespace FPLedit.Shared.Helpers;

/// <summary>
/// Helper class to normalize user-typed <see cref="TimeEntry"/>s.
/// </summary>
public sealed class TimeNormalizer
{
    private readonly Regex verifyRegex;

    public TimeNormalizer()
    {
        //                        hh:mm, h:mm, h:m, hh:m, h:, :m, :mm, hh:                                                                  hhmm, hmm, mm                         m
        verifyRegex = new Regex(@"^ (?<hr>\d{1,2})? : (?<min>\d{1,2})? (?: (?: : (?<sec>\d{1,2})) | (?: , (?<dec>\d{1,2}) ) )? $ | ^ (?<hr>\d{1,2})? (?<min>\d{2}) $ | ^ (?<min>\d{1}) $",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
    }

    /// <summary>
    /// Normalizes the input supplied as parameter.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="allowOverflow">
    /// Specifies, whether hours and minutes can overflow (e.g. hours > 24 and minutes > 59). This can be used for serializing <see cref="TimeEntry"/>s.
    /// </param>
    /// <returns></returns>
    /// <remarks>
    /// The following formats are recognized, each letter represents one digit:
    /// <list type="bullet">
    /// <item><description>hh:mm</description></item>
    /// <item><description>h:mm</description></item>
    /// <item><description>h:m</description></item>
    /// <item><description>hh:m</description></item>
    /// <item><description>h(h):m(m):s(s)</description></item>
    /// <item><description>h:</description></item>
    /// <item><description>:m</description></item>
    /// <item><description>:mm</description></item>
    /// <item><description>hh:</description></item>
    /// <item><description>hhmm</description></item>
    /// <item><description>hmm</description></item>
    /// <item><description>mm</description></item>
    /// <item><description>m</description></item>
    /// </list>
    /// </remarks>
    public string? Normalize(string input, bool allowOverflow = false)
    {
        var m = verifyRegex.Matches(input);
        if (m.Count == 1)
        {
            var hours = m[0].Groups["hr"].Value.PadLeft(2, '0');
            var minutes = m[0].Groups["min"].Value.PadLeft(2, '0');
            var seconds = m[0].Groups["sec"].Value.PadLeft(2, '0');
            var decimals = m[0].Groups["dec"].Value.PadRight(2, '0');

            int hr = int.Parse(hours);
            int mi = int.Parse(minutes);
            int sc = int.Parse(seconds);
            int dc = int.Parse(decimals);
            if ((!allowOverflow && (hr > 24 || mi > 59)) || sc < 0 || sc > 59 || dc > 99)
                return null;

            if (sc > 0)
                return hours + ":" + minutes + ":" + seconds;
            if (dc > 0)
                return hours + ":" + minutes + "," + decimals;
            return hours + ":" + minutes;
        }
        return null;
    }

    /// <summary>
    /// Parses the given input string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="allowOverflow">
    /// Specifies, whether hours and minutes can overflow (e.g. hours > 24 and minutes > 59). This can be used for serializing <see cref="TimeEntry"/>s.
    /// </param>
    /// <returns></returns>
    /// <remarks>
    /// The following formats are recognized, each letter represents one digit:
    /// <list type="bullet">
    /// <item><description>hh:mm</description></item>
    /// <item><description>h:mm</description></item>
    /// <item><description>h:m</description></item>
    /// <item><description>h(h):m(m):s(s)</description></item>
    /// <item><description>hh:m</description></item>
    /// <item><description>h:</description></item>
    /// <item><description>:m</description></item>
    /// <item><description>:mm</description></item>
    /// <item><description>hh:</description></item>
    /// <item><description>hhmm</description></item>
    /// <item><description>hmm</description></item>
    /// <item><description>mm</description></item>
    /// <item><description>m</description></item>
    /// </list>
    /// </remarks>
    public (int hours, int minutes, int seconds, int decimals)? ParseTime(string input, bool allowOverflow = false)
    {
        var m = verifyRegex.Matches(input);
        if (m.Count == 1)
        {
            var hours = m[0].Groups["hr"].Value.PadLeft(2, '0');
            var minutes = m[0].Groups["min"].Value.PadLeft(2, '0');
            var seconds = m[0].Groups["sec"].Value.PadLeft(2, '0');
            var decimals = m[0].Groups["dec"].Value.PadRight(2, '0');

            int hr = int.Parse(hours);
            int mi = int.Parse(minutes);
            int sc = int.Parse(seconds);
            int dc = int.Parse(decimals);
            if ((!allowOverflow && (hr > 24 || mi > 59)) || sc < 0 || sc > 59 || dc > 99)
                return null;

            return (hr, mi, sc, dc);
        }
        return null;
    }
}