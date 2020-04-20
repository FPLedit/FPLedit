using System;

namespace FPLedit.Shared
{
    /// <summary>
    /// Object model type that represents a single stop with time data and all additional data. It is always part of a train.
    /// </summary>
    /// <remarks>You can use <see cref="Train"/>.*ArrDeps* functions to manipulate single ArrDep entries of a given train.</remarks>
    [XElmName("t")]
    [Templating.TemplateSafe]
    public sealed class ArrDep : Entity
    {
        /// <summary>
        /// Collection that allows to modify the shunt moves at this train stop.
        /// </summary>
        public IChildrenCollection<ShuntMove> ShuntMoves { get; }

        /// <summary>
        /// Unique identifier of the station of this stop. This is only available when the parent timetable is a network timetable.
        /// </summary>
        /// <exception cref="TimetableTypeNotSupportedException">The parent timetable is not a network timetable.</exception>
        [XAttrName("fpl-id", IsFpleditElement = true)]
        public int StationId
        {
            get
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "station ids");
                return GetAttribute<int>("fpl-id");
            }
            set
            {
                if (_parent.Type == TimetableType.Linear)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "station ids");
                SetAttribute("fpl-id", value.ToString());
            }
        }

        /// <summary>
        /// Arrival time.
        /// </summary>
        [XAttrName("a")]
        public TimeEntry Arrival
        {
            get => GetTimeAttributeValue("a");
            set => SetNotEmptyTimeAttribute("a", value);
        }

        /// <summary>
        /// Departure time.
        /// </summary>
        [XAttrName("d")]
        public TimeEntry Departure
        {
            get => GetTimeAttributeValue("d");
            set => SetNotEmptyTimeAttribute("d", value);
        }

        /// <summary>
        /// Optional boolean flag: Whether the train has to stop in front of the "Trapeztafel" (German railway signal Ne 1)
        /// before entering the station. Commonly on branch lines without much signalisation. May be shown in some outputs.
        /// </summary>
        [XAttrName("fpl-tr", IsFpleditElement = true)]
        public bool TrapeztafelHalt
        {
            get => Convert.ToBoolean(GetAttribute<int>("fpl-tr"));
            set => SetAttribute("fpl-tr", value ? "1" : "0");
        }

        /// <summary>
        /// Optional metdata, whether and who has to give a "Zuglaufmeldung" (German railway security construct typically
        /// used on branch lines without much signalisation). May be shown in some outputs.
        /// </summary>
        [XAttrName("fpl-zlm", IsFpleditElement = true)]
        public string Zuglaufmeldung
        {
            get => GetAttribute<string>("fpl-zlm");
            set => SetAttribute("fpl-zlm", value);
        }
        
        /// <summary>
        /// Optional value denoting a special track used for arrival at the given station. The track must be defined in
        /// the corresponding Station. If this is set to null, default values might be used or templates might fall back
        /// to <see cref="DepartureTrack"/>.
        /// </summary>
        /// <seealso cref="Station.Tracks"/>
        /// <seealso cref="Station.DefaultTrackLeft"/>
        /// <seealso cref="Station.DefaultTrackRight"/>
        /// <seealso cref="Track"/>
        [XAttrName("at")]
        public string ArrivalTrack
        {
            get => GetAttribute<string>("at");
            set => SetAttribute("at", value);
        }

        /// <summary>
        /// Optional value denoting a special track used for departure at the given station. The track must be defined in
        /// the corresponding Station. If this is set to null, default values might be used or templates might fall back
        /// to <see cref="ArrivalTrack"/>.
        /// </summary>
        /// <seealso cref="Station.Tracks"/>
        /// <seealso cref="Station.DefaultTrackLeft"/>
        /// <seealso cref="Station.DefaultTrackRight"/>
        /// <seealso cref="Track"/>
        [XAttrName("dt")]
        public string DepartureTrack
        {
            get => GetAttribute<string>("dt");
            set => SetAttribute("dt", value);
        }

        /// <summary>
        /// Returns the lowest time value set either on <see cref="Arrival"/> or <see cref="Departure"/> - or
        /// <see cref="TimeEntry.Zero"/> if no time entry is set.
        /// </summary>
        /// <seealso cref="HasMinOneTimeSet"/>
        public TimeEntry FirstSetTime
            => Arrival == default ? Departure : Arrival;

        /// <summary>
        /// Meta-property (readonly) stating, whether the train actually has any time entry (<see cref="Arrival"/> or
        /// <see cref="Departure"/>) set at this station.
        /// </summary>
        public bool HasMinOneTimeSet
            => FirstSetTime != default;

        public ArrDep(Timetable tt) : base("t", tt)
        {
            ShuntMoves = new ObservableChildrenCollection<ShuntMove>(this, "shMove", _parent);
        }

        /// <inheritdoc />
        public ArrDep(XMLEntity en, Timetable tt) : base(en, tt)
        {
            ShuntMoves = new ObservableChildrenCollection<ShuntMove>(this, "shMove", _parent);
        }

        /// <summary>
        /// Applies every data value from another instance - except StationID.
        /// </summary>
        public void ApplyCopy(ArrDep copy)
        {
            Arrival = copy.Arrival;
            Departure = copy.Departure;
            ArrivalTrack = copy.ArrivalTrack;
            DepartureTrack = copy.DepartureTrack;
            TrapeztafelHalt = copy.TrapeztafelHalt;
            Zuglaufmeldung = copy.Zuglaufmeldung;

            ShuntMoves.Clear();
            foreach (var copyShunt in copy.ShuntMoves)
            {
                var newShunt = new ShuntMove(_parent);
                newShunt.ApplyCopy(copyShunt);
                ShuntMoves.Add(newShunt);
            }
        }
    }
}
