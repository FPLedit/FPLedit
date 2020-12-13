using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.TrainLinks;

namespace FPLedit.Shared
{
    /// <summary>
    /// Represents a link between a <see cref="IWritableTrain"/> an a <see cref="LinkedTrain"/>.
    /// </summary>
    [XElmName("tl", ParentElements = new[] { "ta", "ti", "tr" })]
    [Templating.TemplateSafe]
    public class TrainLink : Entity
    {
        private readonly LinkedTrain[] linkedTrains;
        
        /// <summary>
        /// The parent (writable) train.
        /// </summary>
        public Train ParentTrain { get; }

        /// <summary>
        /// All linked trains, associated with this link element. This collection cannot be edited.
        /// </summary>
        public IList<LinkedTrain> LinkedTrains => Array.AsReadOnly(linkedTrains);
        
        /// <summary>
        /// The naming scheme used for this train links children.
        /// </summary>
        [XAttrName("tln")]
        public ITrainNameGen TrainNamingScheme
        {
            get => TrainNameGenManager.Deserialize(GetAttribute("tln", ""));
            set => SetAttribute("tln", TrainNameGenManager.Serialize(value));
        }
        
        /// <summary>
        /// Indices of the child trains. Zero based, counted on trains of this parent train's direction.
        /// </summary>
        [XAttrName("tli")]
        internal int[] TrainIndices
        {
            get => GetAttribute("tli", "").Split(';').Select(s => int.Parse(s)).ToArray();
            private set => SetAttribute("tli", string.Join(";", value));
        }
        
        /// <summary>
        /// Time difference (in minutes) between the linked trains.
        /// </summary>
        [XAttrName("tld")]
        public TimeEntry TimeDifference
        {
            get
            {
                if (!ParentTimetable.TimePrecisionSeconds)
                    return new TimeEntry(0, GetAttribute<int>("tld"));

                var time = GetAttribute("tld", "00:00");
                if (time == "")
                    time = "00:00";
                return ParentTimetable.TimeFactory.Parse(time!);
            }
            set => SetAttribute("tld", ParentTimetable.TimePrecisionSeconds ? value.ToTimeString() : value.GetTotalMinutes().ToString());
        }
        
        /// <summary>
        /// Initial time offset before the first linked train.
        /// </summary>
        [XAttrName("tlo")]
        public TimeEntry TimeOffset
        {
            get
            {
                if (!ParentTimetable.TimePrecisionSeconds)
                    return new TimeEntry(0, GetAttribute<int>("tlo"));

                var time = GetAttribute("tlo", "00:00");
                if (time == "")
                    time = "00:00";
                return ParentTimetable.TimeFactory.Parse(time!);
            }
            set => SetAttribute("tlo", ParentTimetable.TimePrecisionSeconds ? value.ToTimeString() : value.GetTotalMinutes().ToString());
        }

        
        /// <summary>
        /// Count of the linked trains, associated with this link element.
        /// </summary>
        [XAttrName("tlc")]
        public int TrainCount
        {
            get => GetAttribute<int>("tlc");
            private set => SetAttribute("tlc", value.ToString());
        }

        /// <summary>
        /// Zero-based index of this link element, in the parent trains links.
        /// </summary>
        public int TrainLinkIndex => Array.IndexOf(ParentTrain.TrainLinks, this); // Index in parentTrain's TrainLink collection

        /// <summary>
        /// Specifies, whether applications should try to apply the parent train's transitions to linked trains.
        /// </summary>
        /// <remarks>This is not always possible.</remarks>
        [XAttrName("tlt")]
        public bool CopyTransitions
        {
            get => GetAttribute<bool>("tlt");
            set => SetAttribute("tlt", value.ToString());
        }
        
        /// <summary>
        /// Initializes a new train link element.
        /// </summary>
        /// <param name="parentTrain">THe parent train.</param>
        /// <param name="count">The number of linke dtrains, that will be created. This cannot be mutated afterwards.</param>
        public TrainLink(Train parentTrain, int count) : base("tl", parentTrain.ParentTimetable)
        {
            ParentTrain = parentTrain;
            linkedTrains = new LinkedTrain[count];
            ParentTimetable.TrainsXmlCollectionChanged += TrainsXmlCollectionChanged;
            TrainCount = count;
        }

        /// <inheritdoc />
        public TrainLink(XMLEntity en, Train parentTrain) : base(en, parentTrain.ParentTimetable)
        {
            ParentTrain = parentTrain;
            linkedTrains = new LinkedTrain[TrainCount];
            ParentTimetable.TrainsXmlCollectionChanged += TrainsXmlCollectionChanged;
        }

        internal void _InternalInjectLinkedTrain(LinkedTrain train, int counting)
        {
            linkedTrains[counting] = train;
        }

        /// <summary>
        /// Get a train name for the specified train with the giving counting index.
        /// </summary>
        /// <param name="countingIndex">The zero-based counting index of the linked train, relative to this link.</param>
        public string GetChildTrainName(int countingIndex)
        {
            return TrainNamingScheme?.GetTrainName(countingIndex) ?? "";
        }

        /// <summary>
        /// Change a single timetable entry according to the properties of this link element.
        /// </summary>
        /// <param name="tempArrDep"></param>
        /// <param name="countingIndex">The zero-based counting index of the linked train, relative to this link.</param>
        public ArrDep ProcessArrDep(ArrDep tempArrDep, int countingIndex)
        {
            var clone = new ArrDep(ParentTimetable);
            clone.ApplyCopy(tempArrDep);

            var offset = TimeOffset + (countingIndex + 1) * TimeDifference;
            if (clone.Arrival != TimeEntry.Zero)
            {
                clone.Arrival += offset;
                clone.Arrival.Normalize();
            }

            if (clone.Departure != TimeEntry.Zero)
            {
                clone.Departure += offset;
                clone.Departure.Normalize();
            }

            foreach (var shunt in clone.ShuntMoves)
            {
                if (shunt.Time != TimeEntry.Zero)
                {
                    shunt.Time += offset;
                    shunt.Time.Normalize();
                }
            }

            return clone;
        }

        private void ApplyToChildren(Action<LinkedTrain> action)
        {
            foreach (var train in linkedTrains)
            {
                if (train != null)
                    action(train);
            }
        }

        /// <summary>
        /// Also apply all changes made to the parent train to linked trains.
        /// </summary>
        /// <param name="applyTimes">Specifies, whether timetable entries should be copied.</param>
        /// <param name="applyAttributes">Specifies, whether other train attributes should be copied. This will also apply the current <see cref="TrainNamingScheme"/>.</param>
        public void Apply(bool applyTimes, bool applyAttributes)
        {
            ApplyToChildren(lt =>
            {
                if (applyTimes)
                {
                    var path = ParentTimetable.Type == TimetableType.Linear
                        ? ParentTimetable.GetLinearStationsOrderedByDirection(TrainDirection.ti) // All arrdeps are sorted in line direction if linear...
                        : lt.GetPath();
                    var arrdeps = lt.GetArrDepsUnsorted();
                    lt.Children.Clear();
                    foreach (var sta in path)
                        lt.Children.Add(arrdeps[sta].XMLEntity);
                }

                if (applyAttributes)
                {
                    lt.Attributes.Clear();
                    foreach (var attr in ParentTrain.Attributes)
                        lt.SetAttribute(attr.Key, attr.Value);
                    lt.SetAttribute("islink", "true");
                    lt.SetAttribute("name", lt.TName);
                }
            });
        }

        private void TrainsXmlCollectionChanged(object? sender, EventArgs e)
        {
            var trains = ParentTimetable.Trains.Where(t => t.Direction == ParentTrain.Direction).ToArray();
            TrainIndices = linkedTrains.Select(lt => Array.IndexOf(trains, lt)).ToArray();
        }
    }
}