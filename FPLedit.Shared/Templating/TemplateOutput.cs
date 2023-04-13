using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace FPLedit.Shared.Templating
{
    /// <summary>
    /// Helper class for sanitizing and checking output in templates. Those functions are available as aliases in templates.
    /// </summary>
    public static class TemplateOutput
    {
        /// <summary>
        /// Returns a sanitized version of the given string that can safely be used in HTML output.
        /// </summary>
        public static string SafeHtml(string s) => HttpUtility.HtmlEncode(s);

        /// <summary>
        /// Checks whether the given string is safe to use as css string, and throws otherwise.
        /// The check is quite narrow and might not allow all allowed css strings.
        /// </summary>
        /// <remarks>
        /// <para>The user of this function is responsible for quoting the string.</para>
        /// <para>Templates can use this as &quot;safe_css_str&quot;</para>
        /// </remarks>
        /// <exception cref="TemplateOutputException">If the given string is not valid as safe css string.</exception>
        public static string SafeCssStr(string s)
        {
            if (!Regex.IsMatch(s, @"^[a-zA-Z0-9 ._-]+$"))
                throw new TemplateOutputException("Invalid (unsafe) string passed to " + nameof(SafeCssStr));
            return s;
        }

        /// <summary>
        /// Checks whether the given string is safe to use as css font name, and throws otherwise.
        /// The check is quite narrow and might not allow all allowed font names.
        /// </summary>
        /// <remarks>
        /// <para>This function quotes the given font name, but only if necessary - fonts can also be CSS tokens (e.g. serif)</para>
        /// <para>Templates can use this as &quot;safe_css_font&quot;</para>
        /// </remarks>
        /// <exception cref="TemplateOutputException">If the given string is not valid as safe css font name.</exception>
        public static string SafeCssFont(string s)
        {
            if (!Regex.IsMatch(s, @"^[a-zA-Z0-9 ._-]+$"))
                throw new TemplateOutputException("Invalid (unsafe) string passed to " + nameof(SafeCssFont));
            if (new[] { "serif", "sans-serif", "monospace", "cursive", "fantasy", "system-ui" }.Contains(s))
                return s; // CSS-token, not string.
            return "\"" + s + "\""; // literal font name, 
        }
        
        /// <summary>
        /// Checks whether the given string is safe to use inside a css block, and throws otherwise.
        /// The check is quite narrow and might not allow all allowed css block constents.
        /// </summary>
        /// <remarks>Templates can use this as &quot;safe_css_block&quot;</remarks>
        /// <exception cref="TemplateOutputException">If the given string is not valid as safe css block.</exception>
        public static string SafeCssBlock(string s)
        {
            if (s.Contains("</"))
                throw new TemplateOutputException("Invalid (unsafe) string passed to " + nameof(SafeCssBlock));
            return s;
        }
        
        /// <summary>
        /// Creates a prefixed html id or class name from the given name. This function removes all characters that
        /// are not alphanumeric nor a hyphen.
        /// </summary>
        /// <remarks>
        /// <para>It is neither guaranteed that the returned value is actually a valid html
        /// identifier nor that the identifier is unique.</para>
        /// <para>Templates can use this as &quot;html_name&quot;</para>
        /// </remarks>
        public static string HtmlName(string name, string prefix)
        {
            var n = prefix + name.Replace("#", "").ToLower();
            n = Regex.Replace(n, @"/[^a-z0-9-]/", "-");
            return SafeHtml(n);
        }
    }

    /// <summary>
    /// Exception that will be thrown on a validation failure in <see cref="TemplateOutput"/>.
    /// </summary>
    public class TemplateOutputException : Exception
    {
        public TemplateOutputException(string? message) : base(message) { }
    }
}
