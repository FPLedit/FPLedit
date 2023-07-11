using System;
using System.IO;

namespace FPLedit.DebugDump
{
    internal sealed class DumpWriter : IDisposable
    {
        private readonly BinaryWriter writer;
        private readonly FileStream fs;

        public DumpWriter(string filename)
        {
            fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            writer = new BinaryWriter(fs);
            writer.Write("FPLDUMP1");
        }

        public void WriteEvent(DumpEventType type, params string[] data)
        {
            var time = DateTimeOffset.Now.ToUnixTimeSeconds();
            writer.Write((byte)type);
            writer.Write(time);
            writer.Write(data.Length);
            foreach (var text in data)
                writer.Write(text);

            writer.Flush();
        }

        public void Dispose()
        {
            writer.Dispose();
            fs.Dispose();
        }
    }
}