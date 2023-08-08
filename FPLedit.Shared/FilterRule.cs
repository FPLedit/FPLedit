using System;
using System.Diagnostics;

namespace FPLedit.Shared;

/// <summary>
/// This class provides a (non-object-model) way to filter Stations and Trains (and strings in general) following
/// specified patterns.
/// </summary>
/// <remarks>
/// <para>Patterns consist of a pattern type and a search string. The following pattern types sre recognised:</para>
/// <list type="table">
///    <item><term><c> ...</c></term><description>Contains.</description></item>
///    <item><term><c>^...</c></term><description>Starts with.</description></item>
///    <item><term><c>$...</c></term><description>Ends with.</description></item>
///    <item><term><c>=...</c></term><description>Equals.</description></item>
///    <item><term><c>#...</c></term><description>Equals to the Stations <see cref="Station.StationType"/>. Only applicable to stations..</description></item>
///    <item><term><c>!pattern</c></term><description>Negate the pattern <c>pattern</c></description></item>
/// </list>
/// </remarks>
[Templating.TemplateSafe]
[DebuggerDisplay("{" + nameof(Pattern) + "}")]
public sealed class FilterRule
{
    public string Pattern { get; }

    public FilterRule(string pattern)
    {
        Pattern = pattern;
        if (pattern.Length < 2 || (pattern.Length < 3 && pattern[0] == '!'))
            throw new ArgumentException("Pattern is too short!");
    }

    /// <summary>
    /// Returns if a given string matches this pattern.
    /// </summary>
    public bool Matches(string s)
    {
        var type = Pattern[0];
        var negate = type == '!';
        if (negate)
            type = Pattern[1];
        var rest = Pattern.Substring(negate ? 2 : 1);

        switch ((FilterType)type)
        {
            case FilterType.Contains:
                return negate ^ s.Contains(rest);
            case FilterType.Equals:
            case FilterType.StationType:
                return negate ^ s == rest;
            case FilterType.StartsWith:
                return negate ^ s.StartsWith(rest);
            case FilterType.EndsWidth:
                return negate ^ s.EndsWith(rest);
            default:
                throw new FormatException("Unbekannter Regel-Typ: " + Pattern);
        }
    }

    public FilterType FilterType
    {
        get
        {
            var type = Pattern[0];
            var negate = type == '!';
            if (negate)
                type = Pattern[1];
            return (FilterType)type;
        }
    }

    public bool Negate => Pattern[0] == '!';

    public string SearchString => Pattern.Substring((Pattern[0] == '!') ? 2 : 1);

    /// <summary>
    /// Returns if a given train matches this pattern.
    /// </summary>
    /// <exception cref="InvalidOperationException">The type of this FilterRule is "StationType" (#). </exception>
    public bool Matches(ITrain t)
    {
        if (FilterType == FilterType.StationType)
            throw new InvalidOperationException("Cannot apply StationType filter to train!");
        return Matches(t.TName);
    }

    /// <summary>
    /// Returns if a given station matches this pattern.
    /// </summary>
    public bool Matches(Station s)
    {
        if (FilterType == FilterType.StationType)
            return Matches(s.StationType);
        return Matches(s.SName);
    }
}

/// <summary>
/// Denotes the pattern matching type.
/// </summary>
/// <seealso cref="FilterRule">
/// For documentation on available filter types.
/// </seealso>
public enum FilterType
{
    Contains = ' ',
    Equals = '=',
    StartsWith = '^',
    EndsWidth = '$',
    StationType = '#',
}

/// <summary>
/// Lists all default targets for pattern evaluation.
/// </summary>
public enum FilterTarget
{
    Train,
    Station,
}