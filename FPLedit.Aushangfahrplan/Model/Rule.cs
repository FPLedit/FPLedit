using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.AushangfahrplanExport.Model
{
    // Pattern:
    //    " ..." -> enthält
    //    "^..." -> beginnt mit
    //    "$..." -> endet mit
    //    "=..." -> gleich
    internal class Rule
    {
        private string pattern;

        public Rule(string pattern)
        {
            this.pattern = pattern;
            if (pattern.Length < 2)
                throw new ArgumentException("Zu kurzes Pattern!");
        }

        public bool Matches(string s)
        {
            var type = pattern[0];
            var rest = pattern.Substring(1);

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
                    throw new Exception("Unbekannter Regel-Typ: " + pattern);
            }
        }

        public bool Matches(Train t)
            => Matches(t.TName);

        public bool Matches(Station s)
           => Matches(s.SName);
    }
}
