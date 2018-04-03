using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FPLedit.Buchfahrplan.Model
{
    [Serializable]
    [DebuggerDisplay("Point {SName} [{Kilometre}]")]
    public sealed class BfplPoint : Entity, IStation
    {
        public BfplPoint(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public BfplPoint(Timetable tt) : base("p", tt)
        {
        }

        public string SName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }


        public int Wellenlinien
        {
            get => GetAttribute("fpl-wl", 0);
            set => SetAttribute("fpl-wl", value.ToString());
        }

        public string Vmax
        {
            get => GetAttribute("fpl-vmax", "");
            set => SetAttribute("fpl-vmax", value);
        }

        public float LinearKilometre
        {
            get => Positions.GetPosition(Timetable.LINEAR_ROUTE_ID).Value;
            set => Positions.SetPosition(Timetable.LINEAR_ROUTE_ID, value);
        }

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
