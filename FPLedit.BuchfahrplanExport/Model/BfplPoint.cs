using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport.Model
{
    [Serializable]
    [DebuggerDisplay("Point {PName} [{Kilometre}]")]
    public sealed class BfplPoint : Entity
    {
        public BfplPoint(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public BfplPoint(Timetable tt) : base("p", tt)
        {
        }

        public string PName
        {
            get => GetAttribute<string>("name", "");
            set => SetAttribute("name", value);
        }

        public float Kilometre
        {
            get => float.Parse(GetAttribute("km", "0.0"), CultureInfo.InvariantCulture);
            set => SetAttribute("km", value.ToString("0.0", CultureInfo.InvariantCulture));
        }
    }
}
