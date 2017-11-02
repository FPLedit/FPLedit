using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("{SName} [{Kilometre}]")]
    public sealed class Station : Entity, IStation
    {
        public Station(XMLEntity en, Timetable tt) : base(en, tt)
        {
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
            get => Positions.GetPosition(0).Value;
            set => Positions.SetPosition(0, value);
        }

        public PositionCollection Positions
        {
            get => new PositionCollection(this, _parent);
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
    }
}
