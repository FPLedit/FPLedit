using System;

namespace FPLedit.DebugDump
{
    internal class DumpEvent
    {
        public DumpEvent(DumpEventType type, DateTimeOffset time, string[] data)
        {
            Type = type;
            Time = time;
            Data = data;
        }

        public DumpEventType Type { get; }
        public DateTimeOffset Time { get; }
        public string[] Data { get; }
    }
    
    internal enum DumpEventType : byte
    {
        TimetableChange = 1,
        Log = 2,
        TempFile = 4,
        UiInteraction = 8,
        DebugDumpInternal = 16,
    }
}