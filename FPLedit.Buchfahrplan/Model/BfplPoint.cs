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
    [XElmName("p", IsFpleditElement = true)]
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
            => new RouteValueCollection<int>(this, _parent, "fpl-wl", "0", s => int.Parse(s), i => i.ToString());

        [XAttrName("fpl-vmax")]
        public RouteValueCollection<string> Vmax
            => new RouteValueCollection<string>(this, _parent, "fpl-vmax", "", s => s, s => s);

        [XAttrName("fpl-rt")]
        public int[] Routes
        {
            get
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new NotSupportedException("Lineare Strecken haben keine Routen-Ids");
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

        public PositionCollection Positions => new PositionCollection(this, _parent);

        public int Id
        {
            get => throw new NotSupportedException("Points haben keine Id!");
            set => throw new NotSupportedException("Points haben keine Id!");
        }
    }
}
