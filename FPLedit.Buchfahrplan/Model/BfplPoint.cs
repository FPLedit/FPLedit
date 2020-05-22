using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FPLedit.Buchfahrplan.Model
{
    [DebuggerDisplay("Point {SName} [{Positions}]")]
    [XElmName("p", IsFpleditElement = true, ParentElements = new[] {"bfpl_attrs"})]
    public sealed class BfplPoint : Entity, IStation
    {
        public BfplPoint(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public BfplPoint(Timetable tt) : base("p", tt)
        {
        }

        [XAttrName("name")]
        public string SName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        [XAttrName("fpl-wl")]
        public RouteValueCollection<int> Wellenlinien
            => new RouteValueCollection<int>(this, ParentTimetable, "fpl-wl", "0", s => int.Parse(s), i => i.ToString());

        [XAttrName("fpl-vmax")]
        public RouteValueCollection<string> Vmax
            => new RouteValueCollection<string>(this, ParentTimetable, "fpl-vmax", "", s => s, s => s);

        [XAttrName("fpl-rt")]
        public int[] Routes
        {
            get
            {
                if (ParentTimetable.Type == TimetableType.Linear)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "Station.Routes");
                return GetAttribute("fpl-rt", "")
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.Parse(s)).ToArray();
            }
            private set
            {
                if (ParentTimetable.Type == TimetableType.Linear)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "Station.Routes");
                SetAttribute("fpl-rt", string.Join(",", value));
            }
        }

        public PositionCollection Positions => new PositionCollection(this, ParentTimetable);

        public int Id
        {
            get => throw new InvalidOperationException("Points haben keine Id!");
            set => throw new InvalidOperationException("Points haben keine Id!");
        }
        
        internal void _InternalAddRoute(int route)
        {
            var list = Routes.ToList();
            list.Add(route);
            Routes = list.ToArray();
        }
        
        internal void _InternalRemoveRoute(int route)
        {
            var list = Routes.ToList();
            list.Remove(route);
            Routes = list.ToArray();
        }
    }
}
