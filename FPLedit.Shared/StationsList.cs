using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class StationsList : Entity
    {
        private List<Station> stations;

        public ReadOnlyCollection<Station> Stations => stations.AsReadOnly();

        public StationsList() : base("jTrainGraph_stations", null) // Root without parent
        {
            throw new NotSupportedException("Generieren von Streckendateien nicht möglich!");
        }

        public StationsList(XMLEntity en) : base(en, null) // Root without parent
        {
            stations = new List<Station>();
            foreach (var c in Children.Where(x => x.XName == "sta")) // Filtert andere Elemente
                stations.Add(new Station(c, null));
        }
    }
}
