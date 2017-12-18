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
    public class FilterRule
    {
        public string Pattern { get; private set; }

        public FilterRule(string pattern)
        {
            this.Pattern = pattern;
            if (pattern.Length < 2)
                throw new ArgumentException("Zu kurzes Pattern!");
        }

        public bool Matches(string s)
        {
            var type = Pattern[0];
            var rest = Pattern.Substring(1);

            switch(type)
            {
                case ' ':
                    return s.Contains(rest);
                case '=':
                    return s == rest;
                case '^':
                    return s.StartsWith(rest);
                case '$':
                    return s.EndsWith(rest);
                default:
                    throw new Exception("Unbekannter Regel-Typ: " + Pattern);
            }
        }

        public bool Matches(Train t)
            => Matches(t.TName);

        public bool Matches(Station s)
           => Matches(s.SName);
    }
}
