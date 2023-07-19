using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit;

internal class BinaryCacheFile : ICacheFile
{
    private const byte CURRENT_VERSION = 1;
    private const string MAGIC_STRING = "fpledit-cache";
    private ConcurrentDictionary<string, string?> values;

    public BinaryCacheFile()
    {
        values = new ConcurrentDictionary<string, string?>();
    }

    public void Clear()
    {
        values.Clear();
    }

    public void Write(Stream stream, string hash)
    {
        using var bw = new BinaryWriter(stream);

        bw.Write(MAGIC_STRING);
        bw.Write(CURRENT_VERSION);
        bw.Write(hash);
                
        bw.Write(values.Count);

        foreach (var val in values)
        {
            if (val.Value == null) continue;
            bw.Write(val.Key);
            bw.Write(val.Value);
        }
    }

    public void Read(Stream stream, string checkHash)
    {
        values = new ConcurrentDictionary<string, string?>();
        using var br = new BinaryReader(stream);

        if (br.ReadString() != MAGIC_STRING)
            return;
        var ver = br.ReadByte();
        if (ver != CURRENT_VERSION)
            return;
        if (br.ReadString() != checkHash)
            return;
        var count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var key = br.ReadString();
            var value = br.ReadString();
            values[key] = value;
        }
    }

    public string? Get(string key)
    {
        values.TryGetValue(key, out var s);
        return s;
    }

    public void Set(string key, string? value)
    {
        values[key] = value;
    }

    public bool Any() => values.Any();

    public bool ShouldWriteCacheFile(Timetable tt, IReadOnlySettings settings) 
        => (tt.Type == TimetableType.Network && tt.Stations.Count > 20) || settings.Get<bool>("core.force-cache-file");
}