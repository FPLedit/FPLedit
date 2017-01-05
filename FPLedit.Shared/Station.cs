using System;
using System.Diagnostics;
using System.Globalization;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Station : XMLEntity
    {
        public Station(XMLEntity en, Timetable tt) : base(en.el, tt)
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

        [DebuggerStepThrough]
        public override string ToString()
        {
            return SName + " [" + Kilometre + "]";
        }
    }
}
