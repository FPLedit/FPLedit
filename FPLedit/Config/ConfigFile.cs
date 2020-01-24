using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FPLedit.Config
{
    //TODO: One loophole remaining: the config file could be made resad-only after openening FPLedit.
    internal sealed class ConfigFile
    {
        private readonly List<ILine> lines;
        private readonly string filename;

        public ConfigFile(string filename)
        {
            lines = new List<ILine>();
            this.filename = filename;

            Parse();
        }
        
        public ConfigFile()
        {
            lines = new List<ILine>();
        }

        private void Parse()
        {
            if (!File.Exists(filename))
                return;

            var ll = File.ReadAllLines(filename);
            foreach (var l in ll)
            {
                if (l.Trim() == "")
                    lines.Add(new EmptyLine());
                else if (l.Trim().StartsWith("#"))
                    lines.Add(new CommentLine() {Comment = l});
                else
                    lines.Add(new ValueLine(l));
            }
        }

        public string Get(string key)
            => lines.OfType<ValueLine>().FirstOrDefault(vl => vl.Key == key)?.Value;

        public void Set(string key, string value)
        {
            if (KeyExists(key))
                lines.OfType<ValueLine>().First(vl => vl.Key == key).Value = value;
            else
                lines.Add(new ValueLine(key, value));
        }

        public bool KeyExists(string key)
            => lines.OfType<ValueLine>().Any(vl => vl.Key == key);

        public void Remove(string key)
        {
            var ll = lines.OfType<ValueLine>().Where(vl => vl.Key == key).ToArray();
            foreach (var l in ll)
                lines.Remove(l);
        }

        public void Save()
        {
            if (filename == null)
                return; // We hav no backing file.
            var text = string.Join("\n", lines.Select(l => l.Output));
            File.WriteAllText(filename, text);
        }

        private interface ILine
        {
            string Output { get; }
        }

        private class ValueLine : ILine
        {
            public string Key { get; }
            public string Value { get; set; }
            public string Output => $"{Key}=\"{Value}\"";

            public ValueLine(string line)
            {
                var parts = line.Trim().Split('=');
                if (parts.Length != 2 || !parts[1].StartsWith("\"") || !parts[1].EndsWith("\""))
                    throw new FormatException("Konfiguration: Falsches Format in Zeile: " + line);
                Key = parts[0];
                Value = parts[1].Substring(1, parts[1].Length - 2);
            }

            public ValueLine(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }

        private class CommentLine : ILine
        {
            public string Comment { get; set; }
            public string Output => Comment;
        }

        private class EmptyLine : ILine
        {
            public string Output => "";
        }
    }
}