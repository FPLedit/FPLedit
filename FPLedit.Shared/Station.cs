using System;
using System.Diagnostics;
using System.Globalization;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("{SName} [{Kilometre}]")]
    public sealed class Station : Entity
    {
        public Station(XMLEntity en, Timetable tt) : base(en, tt)
        {

        }

        public Station(Timetable tt) : base("sta", tt)
        {

        }

        public string SName
        {
            get
            {
                return GetAttribute<string>("name", "");
            }
            set
            {
                SetAttribute("name", value);
            }
        }

        public float Kilometre
        {
            get
            {
                return float.Parse(GetAttribute("km", "0.0"), CultureInfo.InvariantCulture);
            }
            set
            {
                SetAttribute("km", value.ToString("0.0", CultureInfo.InvariantCulture));
            }
        }
    }
}
