using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// Root object used to import linear line files. (*.str)
    /// </summary>
    [XElmName("jTrainGraph_stations")]
    public sealed class StationsList : Entity
    {
        /// <summary>
        /// All statuins that were defined in this file.
        /// </summary>
        public IList<Station> Stations { get; }

        public StationsList(XMLEntity en) : base(en, null) // Root without parent
        {
            Stations = Children.Where(x => x.XName == "sta") // Filters other elements
                .Select(x => new Station(x, null))
                .ToList().AsReadOnly();
        }
    }
}
