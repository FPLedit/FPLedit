using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public class Route
    {
        public int Index { get; }

        public List<Station> Stations { get; }

        public float MinPosition
            => Stations.Min(s => s.Positions.GetPosition(Index)) ?? 0f;

        public float MaxPosition
            => Stations.Max(s => s.Positions.GetPosition(Index)) ?? 0f;

        public Route(int index, List<Station> stations)
        {
            Index = index;
            Stations = stations;
        }

        public string GetRouteName()
        {
            var st = GetOrderedStations();
            return (st.FirstOrDefault()?.SName ?? "") + " - " + (st.LastOrDefault()?.SName ?? "");
        }

        public List<Station> GetOrderedStations()
            => Stations.OrderBy(s => s.Positions.GetPosition(Index)).ToList();
    }
}
