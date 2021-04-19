using System;

namespace FPLedit.Templating
{
    /// <summary>
    /// Parses argument strings like <c>name1="value1" name2="value2"</c>
    /// </summary>
    internal ref struct ArgsParser
    {
        private readonly ReadOnlySpan<char> args;
        private readonly string[] required;

        private int startIndex;
        private readonly Span<bool> found;

        public (string name, string value) Current { get; private set; }
        
        public ArgsParser GetEnumerator() => this;

        public ArgsParser(ReadOnlySpan<char> args, params string[] required)
        {
            this.args = args;
            this.required = required;
            startIndex = 0;
            
            found = new bool[required.Length];
            Current = ("", "");
        }

        public bool FoundAll()
        {
            bool ret = true;
            for (int bi = 0; bi < found.Length; bi++)
                ret &= found[bi];
            return ret;
        }

        public bool MoveNext()
        {
            bool isInString = false;
            bool hadEscapeCharacter = false;
            bool hadEquals = false;
            string currentName = "", currentValue = "";

            for (int i = startIndex; i < args.Length; i++)
            {
                char ch = args[i];
                if (!isInString)
                {
                    if (char.IsWhiteSpace(ch))
                    {
                        if (currentName != "")
                            throw new FormatException("Keine Leerzeichen in Namen erlaubt: " + args.ToString());
                        continue;
                    }
                    if (ch == '=')
                    {
                        hadEquals = true;
                        continue;
                    }
                    if (ch == '"')
                    {
                        if (!hadEquals)
                            throw new FormatException("Kein = vor dem Wert vorhanden: " + args.ToString());
                        isInString = true;
                        hadEquals = false;
                        continue;
                    }
                    currentName += ch;
                }
                else
                {
                    if (ch == '"' && !hadEscapeCharacter) // String-Ende
                    {
                        Current = (currentName, currentValue);
                        var foundIndex = Array.IndexOf(required, currentName);
                        if (foundIndex >= 0)
                            found[foundIndex] = true;
                        startIndex = i + 1;
                        return true;
                    }
                    if (ch == '\\' && !hadEscapeCharacter)
                    {
                        hadEscapeCharacter = true;
                        continue;
                    }
                    if ((ch == '"' || ch == '\\') && hadEscapeCharacter)
                        hadEscapeCharacter = false;
                    if (hadEscapeCharacter)
                        throw new FormatException("Unbekannte Escape-Sequenz: " + args.ToString());
                    currentValue += ch;
                }
            }

            if (currentValue != "")
                throw new FormatException("Nicht beendete Zeichenkette: " + args.ToString());
            if (currentName != "")
                throw new FormatException("Unvollständige Deklaration: " + args.ToString());
            return false;
        }
    }
}
