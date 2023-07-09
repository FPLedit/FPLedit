using System;

namespace FPLedit.DebugDump;

internal record DumpEvent(DumpEventType Type, DateTimeOffset Time, string[] Data);
    
internal enum DumpEventType : byte
{
    TimetableChange = 1,
    Log = 2,
    TempFile = 4,
    UiInteraction = 8,
    DebugDumpInternal = 16,
}