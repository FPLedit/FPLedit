using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FPLedit.DebugDump
{
    public sealed class DumpReader
    {
        public DumpEvent[] Events { get; }

        public DumpReader(string filename)
        {
            using(var fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read))
            using (var writer = new BinaryReader(fs))
            {
                if (writer.ReadString() != "FPLDUMP1")
                    throw new NotSupportedException("Wrong dump file version.");
                Events = ReadEvents(writer, fs).ToArray();
            }
        }

        private IEnumerable<DumpEvent> ReadEvents(BinaryReader writer, Stream fs)
        {
            while (fs.Position < fs.Length - 1)
            {
                var type = (DumpEventType) writer.ReadByte();
                var time = DateTimeOffset.FromUnixTimeSeconds(writer.ReadInt64());
                var dataLength = writer.ReadInt32();
                var data = new string[dataLength];

                for (int i = 0; i < dataLength; i++)
                    data[i] = writer.ReadString();
                yield return new DumpEvent(type, time, data);
            }
        }
    }
}