using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    // Schöner deutscher Begriff: Fahrtzeiteintrag
    [Serializable]
    public class ArrDep : Entity
    {
        public ObservableCollection<ShuntMove> ShuntMoves { get; private set; }

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

        public string ArrivalTrack
        {
            get => GetAttribute<string>("at");
            set => SetAttribute("at", value);
        }

        public string DepartureTrack
        {
            get => GetAttribute<string>("dt");
            set => SetAttribute("dt", value);
        }

        // Meta-Properties
        public TimeSpan FirstSetTime
            => Arrival == default ? Departure : Arrival;

        public bool HasMinOneTimeSet
            => FirstSetTime != default;

        public ArrDep(Timetable tt) : base("t", tt)
        {
            ShuntMoves = new ObservableChildrenCollection<ShuntMove>(this, "shMove", _parent);
        }

        public ArrDep(XMLEntity en, Timetable tt) : base(en, tt)
        {
            ShuntMoves = new ObservableChildrenCollection<ShuntMove>(this, "shMove", _parent);
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

            foreach (var shunt in copy.ShuntMoves)
                ShuntMoves.Add(shunt.Clone<ShuntMove>());
         }
    }
}
