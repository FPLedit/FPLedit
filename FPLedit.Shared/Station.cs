using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("{SName} [Linear: {Kilometre}]")]
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

        public float Kilometre
        {
            get => Positions.GetPosition().Value;
            set => Positions.SetPosition(value);
        }

        private PositionCollection Positions
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
    }
}
