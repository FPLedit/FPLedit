using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Templating
{
    internal static class ArgsParser
    {
        //TODO: Make this ugly code better ;-)
        public static Dictionary<string, string> ParseArgs(string args)
        {
            Dictionary<string, string> kv = new Dictionary<string, string>();
            bool isInString = false;
            bool hadEscapeCharacter = false;
            bool hadEquals = false;
            string tmp_name = "", tmp_val = "";

            var chars = args.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (!isInString)
                {
                    if (char.IsWhiteSpace(chars[i]))
                    {
                        if (tmp_name != "")
                            throw new FormatException("Keine Leerzeichen in Namen erlaubt: " + args);
                        continue;
                    }
                    if (chars[i] == '=')
                    {
                        hadEquals = true;
                        continue;
                    }
                    if (chars[i] == '"')
                    {
                        if (!hadEquals)
                            throw new FormatException("Kein = vor dem Wert vorhanden: " + args);
                        isInString = true;
                        hadEquals = false;
                        continue;
                    }
                    tmp_name += chars[i];
                }
                else
                {
                    if (chars[i] == '"' && !hadEscapeCharacter)
                    {
                        isInString = false;
                        kv.Add(tmp_name, tmp_val);
                        tmp_name = "";
                        tmp_val = "";
                        continue;
                    }
                    if (chars[i] == '\\' && !hadEscapeCharacter)
                    {
                        hadEscapeCharacter = true;
                        continue;
                    }
                    if ((chars[i] == '"' || chars[i] == '\\') && hadEscapeCharacter)
                        hadEscapeCharacter = false;
                    if (hadEscapeCharacter)
                        throw new FormatException("Unbekannte Escape-Sequenz: " + args);
                    tmp_val += chars[i];
                }
            }

            if (tmp_val != "")
                throw new FormatException("Nicht beendete Zeichenkette: " + args);
            if (tmp_name != "")
                throw new FormatException("Unvollständige Deklaration: " + args);

            return kv;
        }
    }
}
