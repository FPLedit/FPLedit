using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Templating
{
    internal readonly ref struct ArgsParser
    {
        private readonly Dictionary<string, string> parsedArgs;

        public ArgsParser(ReadOnlySpan<char> args)
        {
            parsedArgs = new Dictionary<string, string>(); //TODO: Somehow do not allocate dictionary?
            ParseArgs(args);
        }

        // Format name="value" name="value"
        private void ParseArgs(ReadOnlySpan<char> args)
        {
            bool isInString = false;
            bool hadEscapeCharacter = false;
            bool hadEquals = false;
            string currentName = "", currentValue = ""; //TODO: Do not allocate strings.

            for (int i = 0; i < args.Length; i++)
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
                        throw new FormatException("Unbekannte Escape-Sequenz: " + args.ToString());
                    currentValue += ch;
                }
            }

            if (currentValue != "")
                throw new FormatException("Nicht beendete Zeichenkette: " + args.ToString());
            if (currentName != "")
                throw new FormatException("Unvollständige Deklaration: " + args.ToString());
        }

        public bool Require(params string[] keys) => keys.Except(parsedArgs.Keys).Any();

        public string this[string idx] => parsedArgs[idx];
    }
}
