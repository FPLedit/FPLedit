using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Helpers;

/// <summary>
/// Helper class to replace string.Split and string.Join to deal with a delimeter that can be escaped by itself.
/// </summary>
internal class EscapeSplitHelper
{
    private readonly char delimiter;

    public EscapeSplitHelper(char delimiter)
    {
        this.delimiter = delimiter;
    }

    public IEnumerable<string> SplitEscaped(string s)
    {
        string lastValue = "";
        char last = '\0';
        bool hadLastEscape = false;
        var chars = s.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            var hle = hadLastEscape;
            hadLastEscape = false;
            if (last == delimiter && chars[i] == delimiter && !hle) // Escape sequence handling, only if we had not an escape sequence the last time.
            {
                lastValue += delimiter;
                hadLastEscape = true;
            }
            else if (last == delimiter && chars[i] != delimiter && !hle) // Normal end sequence.
            {
                if (!string.IsNullOrEmpty(lastValue))
                    yield return lastValue;
                lastValue = "";
            }

            if (chars[i] != delimiter)
                lastValue += chars[i];

            last = chars[i];
        }

        if (!string.IsNullOrEmpty(lastValue))
            yield return lastValue;
    }

    public string JoinEscaped(IEnumerable<string> parts)
    {
        var delimString = new string(delimiter, 1);
        var escapedParts = parts.Select(p => p.Replace(delimString, new string(delimiter, 2)));
        return string.Join(delimString, escapedParts);
    }
}