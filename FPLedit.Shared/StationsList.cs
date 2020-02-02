using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FPLedit.Shared
{
    [XElmName("jTrainGraph_stations")]
    public sealed class StationsList : Entity
    {
        public ReadOnlyCollection<Station> Stations { get; private set; }

        public StationsList() : base("jTrainGraph_stations", null) // Root without parent
        {
            throw new NotSupportedException("Generieren von Streckendateien nicht möglich!");
        }

        public StationsList(XMLEntity en) : base(en, null) // Root without parent
        {
            Stations = Children.Where(x => x.XName == "sta") // Filtert andere Elemente
                .Select(x => new Station(x, null))
                .ToList().AsReadOnly();
        }
    }
}
