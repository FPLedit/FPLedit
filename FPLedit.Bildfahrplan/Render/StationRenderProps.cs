using FPLedit.Shared;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Bildfahrplan.Render;

internal class StationRenderProps
{
    public static int IndividualTrackOffset => 15;

    public int Left { get; }
    public int Right { get; }
    public int Center => (int)((Left + Right) / 2f);

    public Dictionary<string, int> TrackOffsets { get; }

    public decimal CurKilometer { get; }

    public StationRenderProps(Station sta, decimal kil, float left, bool includeTracks = false)
    {
        CurKilometer = kil;

        TrackOffsets = new Dictionary<string, int>();
        Left = Right = (int)left;

        if (includeTracks)
        {
            if (sta.Tracks.Any())
                Right = (int)(left + (1 + sta.Tracks.Count) * IndividualTrackOffset);

            var i = 0;
            foreach (var t in sta.Tracks)
                TrackOffsets.Add(t.Name, (int)(left + (++i) * IndividualTrackOffset));
        }
    }
}