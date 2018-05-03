using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("{SName} [Linear: {LinearKilometre}]")]
    public sealed class Station : Entity, IStation
    {
        public Station(XMLEntity en, Timetable tt) : base(en, tt)
        {
            Positions.TestForErrors();
        }

        public Station(Timetable tt) : base("sta", tt)
        {
        }

        public string SName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        public float LinearKilometre
        {
            get => Positions.GetPosition(Timetable.LINEAR_ROUTE_ID).Value;
            set => Positions.SetPosition(Timetable.LINEAR_ROUTE_ID, value);
        }

        public PositionCollection Positions
            => new PositionCollection(this, _parent);


        public int Wellenlinien
        {
            get => GetAttribute("fpl-wl", 0);
            set => SetAttribute("fpl-wl", value.ToString());
        }

        public int LineTracksRight
        {
            get => GetAttribute("tr", 1);
            set => SetAttribute("tr", value.ToString());
        }

        public string Vmax
        {
            get => GetAttribute("fpl-vmax", "");
            set => SetAttribute("fpl-vmax", value);
        }

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
