using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("{SName} [{GetAttribute(\"km\", \"\")}]")]
    public sealed class Station : Entity, IStation
    {
        public ObservableCollection<Track> Tracks { get; private set; }

        public Station(XMLEntity en, Timetable tt) : base(en, tt)
        {
            Positions.TestForErrors();
            Tracks = new ObservableChildrenCollection<Track>(this, "track", _parent);
        }

        public Station(Timetable tt) : base("sta", tt)
        {
            Tracks = new ObservableChildrenCollection<Track>(this, "track", _parent);
        }

        public string SName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        public PositionCollection Positions
            => new PositionCollection(this, _parent);

        public RouteValueCollection<int> LineTracksRight
            => new RouteValueCollection<int>(this, _parent, "tr", "1", s => int.Parse(s), i => i.ToString());

        public RouteValueCollection<int> Wellenlinien
            => new RouteValueCollection<int>(this, _parent, "fpl-wl", "0", s => int.Parse(s), i => i.ToString());

        public RouteValueCollection<string> Vmax
            => new RouteValueCollection<string>(this, _parent, "fpl-vmax", "", s => s, s => s);

        public RouteValueCollection<string> DefaultTrackRight
            => new RouteValueCollection<string>(this, _parent, "dTi", "", s => s, s => s);

        public RouteValueCollection<string> DefaultTrackLeft
            => new RouteValueCollection<string>(this, _parent, "dTa", "", s => s, s => s);

        public int Id
        {
            get
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new NotSupportedException("Lineare Strecken haben keine Bahnhofs-Ids");
                return GetAttribute<int>("fpl-id");
            }
            set
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new NotSupportedException("Lineare Strecken haben keine Bahnhofs-Ids");
                SetAttribute("fpl-id", value.ToString());
            }
        }

        public int[] Routes
        {
            get
            {
                if (_parent.Type == TimetableType.Linear)
                    return new[] { 0 };
                return GetAttribute("fpl-rt", "")
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.Parse(s)).ToArray();
            }
            set
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new NotSupportedException("Lineare Strecken haben keine Routen-Ids");
                SetAttribute("fpl-rt", string.Join(",", value));
            }
        }
    }
}
