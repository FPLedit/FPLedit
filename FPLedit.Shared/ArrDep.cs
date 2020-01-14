using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    // Schöner deutscher Begriff: Fahrtzeiteintrag
    [Serializable]
    [XElmName("t")]
    [Templating.TemplateSafe]
    public class ArrDep : Entity
    {
        public IChildrenCollection<ShuntMove> ShuntMoves { get; private set; }

        [XAttrName("fpl-id")]
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

        [XAttrName("a")]
        public TimeSpan Arrival
        {
            get => GetTimeValue("a");
            set => SetNotEmptyTime(value, "a");
        }

        [XAttrName("d")]
        public TimeSpan Departure
        {
            get => GetTimeValue("d");
            set => SetNotEmptyTime(value, "d");
        }

        [XAttrName("fpl-tr")]
        public bool TrapeztafelHalt
        {
            get => Convert.ToBoolean(GetAttribute<int>("fpl-tr"));
            set => SetAttribute("fpl-tr", value ? "1" : "0");
        }

        [XAttrName("fpl-zlm")]
        public string Zuglaufmeldung
        {
            get => GetAttribute<string>("fpl-zlm");
            set => SetAttribute("fpl-zlm", value);
        }

        [XAttrName("at")]
        public string ArrivalTrack
        {
            get => GetAttribute<string>("at");
            set => SetAttribute("at", value);
        }

        [XAttrName("dt")]
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
            ArrivalTrack = copy.ArrivalTrack;
            DepartureTrack = copy.DepartureTrack;
            TrapeztafelHalt = copy.TrapeztafelHalt;
            Zuglaufmeldung = copy.Zuglaufmeldung;

            ShuntMoves.Clear();
            foreach (var shunt in copy.ShuntMoves)
                ShuntMoves.Add(shunt.Clone<ShuntMove>());
        }
    }
}
