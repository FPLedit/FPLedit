using System;
using System.Text.RegularExpressions;
using System.Web;

namespace FPLedit.Shared.Templating
{
    /// <summary>
    /// Helper class for sanitizing and checking output in templates.
    /// </summary>
    public static class TemplateOutput
    {
        public static string SafeHtml(string s) => HttpUtility.HtmlEncode(s);

        public static string SafeCssStr(string s)
        {
            if (!Regex.IsMatch(s, @"^[a-zA-Z0-9 ._-]+$"))
                throw new ArgumentException("Invalid (unsafe) string passed to " + nameof(SafeCssStr));
            return s;
        }
        
        public static string SafeCssBlock(string s)
        {
            if (s.Contains("</"))
                throw new ArgumentException("Invalid (unsafe) string passed to " + nameof(SafeCssBlock));
            return s; //TODO: is html escape still needed in this case?
        }
    }
}
