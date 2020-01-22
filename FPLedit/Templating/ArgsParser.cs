using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Templating
{
    internal sealed class ArgsParser
    {
        private readonly string args;
        private readonly char[] chars;
        private readonly Dictionary<string, string> parsedArgs;

        public ArgsParser(string args)
        {
            this.args = args;
            chars = args.ToCharArray();

            parsedArgs = new Dictionary<string, string>();
            ParseArgs();
        }

        // Format name="value" name="value"
        private void ParseArgs()
        {
            bool isInString = false;
            bool hadEscapeCharacter = false;
            bool hadEquals = false;
            string currentName = "", currentValue = "";

            foreach (var ch in chars)
            {
                if (!isInString)
                {
                    if (char.IsWhiteSpace(ch))
                    {
                        if (currentName != "")
                            throw new FormatException("Keine Leerzeichen in Namen erlaubt: " + args);
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
                            throw new FormatException("Kein = vor dem Wert vorhanden: " + args);
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
                        isInString = false;
                        parsedArgs.Add(currentName, currentValue);
                        currentName = "";
                        currentValue = "";
                        continue;
                    }
                    if (ch == '\\' && !hadEscapeCharacter)
                    {
                        hadEscapeCharacter = true;
                        continue;
                    }
                    if ((ch == '"' || ch == '\\') && hadEscapeCharacter)
                        hadEscapeCharacter = false;
                    if (hadEscapeCharacter)
                        throw new FormatException("Unbekannte Escape-Sequenz: " + args);
                    currentValue += ch;
                }
            }

            if (currentValue != "")
                throw new FormatException("Nicht beendete Zeichenkette: " + args);
            if (currentName != "")
                throw new FormatException("Unvollständige Deklaration: " + args);
        }

        public bool Require(params string[] keys) => keys.Any(k => !parsedArgs.ContainsKey(k));

        public string this[string idx] => parsedArgs[idx];
    }
}
