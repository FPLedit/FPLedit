using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan.Render
{
    internal class StationX
    {
        public static int IndividualTrackOffset => 15;

        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Center => (Left + Right) / 2;

        public Dictionary<string, float> TrackOffsets { get; private set; }

        public Station Station { get; private set; }
        public float CurKilometer { get; private set; }

        public StationX(Station sta, float kil, float left, bool includeTracks = false)
        {
            Station = sta;
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
}
