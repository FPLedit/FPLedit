using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    // Pattern:
    //    " ..." -> enthält
    //    "^..." -> beginnt mit
    //    "$..." -> endet mit
    //    "=..." -> gleich
    //    "!<pattern>" -> negiert
    [Templating.TemplateSafe]
    public sealed class FilterRule
    {
        public string Pattern { get; }

        public FilterRule(string pattern)
        {
            Pattern = pattern;
            if (pattern.Length < 2 || (pattern.Length < 3 && pattern[0] == '!'))
                throw new ArgumentException("Zu kurzes Pattern!");
        }

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

        public bool Matches(Train t)
        {
            if (FilterType == FilterType.StationType)
                throw new InvalidOperationException("Cannot apply StationType filter to train!");
            return Matches(t.TName);
        }

        public bool Matches(Station s)
        {
            if (FilterType == FilterType.StationType)
                return Matches(s.StationType);
            return Matches(s.SName);
        }
    }

    public enum FilterType
    {
        Contains = ' ',
        Equals = '=',
        StartsWith = '^',
        EndsWidth = '$',
        StationType = '#',
    }

    public enum FilterTarget
    {
        Train,
        Station,
    }
}
