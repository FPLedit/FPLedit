using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public sealed class Route
    {
        private readonly Station[] stations;
        
        public int Index { get; }

        public IList<Station> Stations => Array.AsReadOnly(stations);

        public float MinPosition
            => Exists ? stations[0].Positions.GetPosition(Index) ?? 0f : 0f;

        public float MaxPosition
            => Exists ? stations[stations.Length - 1].Positions.GetPosition(Index) ?? 0f : 0f;

        public bool Exists => stations.Length > 0;

        internal Route(int index, IEnumerable<Station> stations)
        {
            Index = index;
            this.stations = stations.OrderBy(s => s.Positions.GetPosition(Index)).ToArray();
        }

        public string GetRouteName()
        {
            if (!Exists)
                return "<leer>";
            return stations[0].SName + " - " + stations[stations.Length - 1].SName; // this will always work, as stations.Length >= 1
        }
    }
}
