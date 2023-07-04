using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FPLedit.Shared;

namespace FPLedit.Config
{
    internal class ConfigFile : IDisposable
    {
        private readonly List<ILine> lines;

        private Stream? stream;
        private readonly bool leaveOpen;

        public bool IsReadonly => stream == null || !stream.CanWrite;

        public ConfigFile(Stream stream, bool leaveOpen)
        {
            lines = new List<ILine>();
            this.stream = stream;
            this.leaveOpen = leaveOpen;

            Parse();
        }

        public ConfigFile()
        {
            lines = new List<ILine>();
        }

        private void Parse()
        {
            if (stream == null)
                return;

            if (stream.Position != 0L && stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            using var sr = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Trim() == "")
                    lines.Add(new EmptyLine());
                else if (line.Trim().StartsWith("#"))
                    lines.Add(new CommentLine(line));
                else
                    lines.Add(ValueLine.Parse(line));
            }
        }

        public string? Get(string key)
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
            if (stream == null)
                return; // We hav no backing file.
            var text = string.Join("\n", lines.Select(l => l.Output));
            stream.SetLength(0);
            using var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true);
            sw.Write(text);
        }

        private interface ILine
        {
            string Output { get; }
        }

        private record ValueLine(string Key, string Value) : ILine
        {
            public string Value { get; set; } = Value;
            public string Output => $"{Key}=\"{Value}\"";

            public static ValueLine Parse(string line)
            {
                var parts = line.Trim().Split('=');
                if (parts.Length != 2 || !parts[1].StartsWith("\"") || !parts[1].EndsWith("\""))
                    throw new FormatException(T._("Konfiguration: Falsches Format in Zeile: {0}", line));
                return new ValueLine(parts[0], parts[1].Substring(1, parts[1].Length - 2));
            }
        }

        private record CommentLine(string Comment) : ILine
        {
            public string Output => Comment;
        }

        private record EmptyLine() : ILine
        {
            public string Output => "";
        }

#pragma warning disable CA1816
        public void Dispose(bool disposeStream)
        {
            if (!leaveOpen && disposeStream)
            {
                stream?.Close();
                stream?.Dispose();
                stream = null;
            }

            GC.SuppressFinalize(this);
        }

        public void Dispose()
            => Dispose(true);
#pragma warning restore CA1816

        ~ConfigFile()
        {
            Dispose();
        }
    }
}