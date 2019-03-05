using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    // Schöner deutscher Begriff: Fahrtzeiteintrag
    [Serializable]
    public class ArrDep : Entity
    {
        public int StationId
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

        public TimeSpan Arrival
        {
            get => GetTime("a");
            set => SetNotEmptyTime(value, "a");
        }

        public TimeSpan Departure
        {
            get => GetTime("d");
            set => SetNotEmptyTime(value, "d");
        }

        public bool TrapeztafelHalt
        {
            get => Convert.ToBoolean(GetAttribute<int>("fpl-tr"));
            set => SetAttribute("fpl-tr", value ? "1" : "0");
        }

        public string Zuglaufmeldung
        {
            get => GetAttribute<string>("fpl-zlm");
            set => SetAttribute("fpl-zlm", value);
        }

        // Meta-Properties
        public TimeSpan FirstSetTime
            => Arrival == default ? Departure : Arrival;

        public bool HasMinOneTimeSet
            => FirstSetTime != default;

        public ArrDep(Timetable tt) : base("t", tt)
        {
        }

        public ArrDep(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        private void SetNotEmptyTime(TimeSpan time, string key)
        {
            var t = time.ToShortTimeString();
            SetAttribute(key, t != "00:00" ? t : "");
        }

        private TimeSpan GetTime(string key)
        {
            var val = GetAttribute(key, "");
            return val != "" ? TimeSpan.Parse(val) : default;
        }

        /// <summary>
        /// Applies every data value from another instance - except StationID.
        /// </summary>
        /// <param name="copy"></param>
        public void ApplyCopy(ArrDep copy)
        {
            Arrival = copy.Arrival;
            Departure = copy.Departure;
            TrapeztafelHalt = copy.TrapeztafelHalt;
            Zuglaufmeldung = copy.Zuglaufmeldung;
        }
    }
}
