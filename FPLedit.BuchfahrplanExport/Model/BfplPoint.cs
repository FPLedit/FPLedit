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

        public float Kilometre
        {
            get => float.Parse(GetAttribute("km", "0.0"), CultureInfo.InvariantCulture);
            set => SetAttribute("km", value.ToString("0.0", CultureInfo.InvariantCulture));
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
    }
}
