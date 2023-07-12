using FPLedit.Shared;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Bildfahrplan.Render;

internal class StationRenderProps
{
    public static int IndividualTrackOffset => 15;

    public float Left { get; }
    public float Right { get; }
    public float Center => (Left + Right) / 2;

    public Dictionary<string, float> TrackOffsets { get; }

    public float CurKilometer { get; }

    public StationRenderProps(Station sta, float kil, float left, bool includeTracks = false)
    {
        CurKilometer = kil;

        TrackOffsets = new Dictionary<string, float>();
        Left = Right = left;

        if (includeTracks)
        {
            if (sta.Tracks.Any())
                Right = left + (1 + sta.Tracks.Count) * IndividualTrackOffset;

            int i = 0;
            foreach (var t in sta.Tracks)
                TrackOffsets.Add(t.Name, left + (++i) * IndividualTrackOffset);
        }
    }
}