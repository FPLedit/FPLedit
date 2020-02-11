using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    [DebuggerDisplay("{SName} [{GetAttribute(\"km\", \"\")}]")]
    [XElmName("sta")]
    [Templating.TemplateSafe]
    public sealed class Station : Entity, IStation
    {
        public IChildrenCollection<Track> Tracks { get; }

        public Station(XMLEntity en, Timetable tt) : base(en, tt)
        {
            Positions.TestForErrors();
            Tracks = new ObservableChildrenCollection<Track>(this, "track", _parent);
        }

        public Station(Timetable tt) : base("sta", tt)
        {
            Tracks = new ObservableChildrenCollection<Track>(this, "track", _parent);
        }

        [XAttrName("name")]
        public string SName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        public PositionCollection Positions
            => new PositionCollection(this, _parent);

        [XAttrName("tr")]
        public RouteValueCollection<int> LineTracksRight
            => new RouteValueCollection<int>(this, _parent, "tr", "1", s => int.Parse(s), i => i.ToString());

        [XAttrName("fpl-wl", IsFpleditElement = true)]
        public RouteValueCollection<int> Wellenlinien
            => new RouteValueCollection<int>(this, _parent, "fpl-wl", "0", s => int.Parse(s), i => i.ToString());

        [XAttrName("fpl-vmax", IsFpleditElement = true)]
        public RouteValueCollection<string> Vmax
            => new RouteValueCollection<string>(this, _parent, "fpl-vmax", "", s => s, s => s);

        [XAttrName("dTi")]
        public RouteValueCollection<string> DefaultTrackRight
            => new RouteValueCollection<string>(this, _parent, "dTi", "", s => s, s => s);

        [XAttrName("dTa")]
        public RouteValueCollection<string> DefaultTrackLeft
            => new RouteValueCollection<string>(this, _parent, "dTa", "", s => s, s => s);

        [XAttrName("fpl-id", IsFpleditElement = true)]
        public int Id
        {
            get
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "station ids");
                return GetAttribute<int>("fpl-id");
            }
            internal set
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "station ids");
                if (GetAttribute<string>("fpl-id") != null)
                    throw new InvalidOperationException("Station hat bereits eine Id!");
                SetAttribute("fpl-id", value.ToString());
            }
        }

        [XAttrName("fpl-rt", IsFpleditElement = true)]
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
            private set
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "route ids");
                SetAttribute("fpl-rt", string.Join(",", value));
            }
        }
        
        internal bool _InternalAddRoute(int route)
        {
            if (Routes.Contains(route)) 
                return false;
            var list = Routes.ToList();
            list.Add(route);
            Routes = list.ToArray();
            return true;
        }
        
        internal bool _InternalRemoveRoute(int route)
        {
            if (Routes.Contains(route)) 
                return false;
            var list = Routes.ToList();
            list.Remove(route);
            Routes = list.ToArray();
            return true;
        }
    }
}
