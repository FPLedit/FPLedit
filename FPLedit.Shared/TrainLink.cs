using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared
{
    // countingIndex everywhere is 0 based!
    [XElmName("tl", ParentElements = new[] { "ta", "ti", "tr" })]
    [Templating.TemplateSafe]
    public class TrainLink : Entity
    {
        private readonly LinkedTrain[] linkedTrains;
        public Train ParentTrain { get; }

        public IList<LinkedTrain> LinkedTrains => Array.AsReadOnly(linkedTrains);
        
        [XAttrName("tln")]
        public ITrainLinkNameCalculator TrainNamingScheme
        {
            get => TrainLinkNameCalculatorManager.Deserialize(GetAttribute<string>("tln"));
            set => SetAttribute("tln", TrainLinkNameCalculatorManager.Serialize(value));
        }
        
        [XAttrName("tli")]
        internal int[] TrainIndices
        {
            get => GetAttribute<string>("tli").Split(';').Select(s => int.Parse(s)).ToArray();
            private set => SetAttribute("tli", string.Join(";", value));
        }
        
        [XAttrName("tld")]
        public int TimeDifference
        {
            get => GetAttribute<int>("tld");
            set => SetAttribute("tld", value.ToString());
        }
        
        [XAttrName("tlo")]
        public int TimeOffset
        {
            get => GetAttribute<int>("tlo");
            set => SetAttribute("tlo", value.ToString());
        }
        
        [XAttrName("tlc")]
        public int TrainCount
        {
            get => GetAttribute<int>("tlc");
            private set => SetAttribute("tlc", value.ToString());
        }

        public int TrainLinkIndex => Array.IndexOf(ParentTrain.TrainLinks, this); // Index in parentTrain's TrainLink collection

        [XAttrName("tlt")]
        public bool CopyTransitions
        {
            get => GetAttribute<bool>("tlt");
            set => SetAttribute("tlt", value.ToString());
        }
        
        public TrainLink(Train parentTrain, int count) : base("tl", parentTrain._parent)
        {
            ParentTrain = parentTrain;
            linkedTrains = new LinkedTrain[count];
            _parent.TrainsChanged += TrainsChanged;
            TrainCount = count;
        }

        /// <inheritdoc />
        public TrainLink(XMLEntity en, Train parentTrain) : base(en, parentTrain._parent)
        {
            ParentTrain = parentTrain;
            linkedTrains = new LinkedTrain[TrainCount];
            _parent.TrainsChanged += TrainsChanged;
        }

        internal void _InternalInjectLinkedTrain(LinkedTrain train, int counting)
        {
            linkedTrains[counting] = train;
        }

        public string GetChildTrainName(int countingIndex)
        {
            return TrainNamingScheme.GetTrainName(countingIndex);
        }

        public ArrDep ProcessArrDep(ArrDep tempArrDep, int countingIndex)
        {
            var clone = new ArrDep(_parent);
            clone.ApplyCopy(tempArrDep);

            var offset = new TimeEntry(0,TimeOffset + (countingIndex + 1) * TimeDifference);
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

        public void ApplyToChildren(Action<LinkedTrain> action)
        {
            foreach (var train in linkedTrains)
                action(train);
        }

        private void TrainsChanged(object sender, EventArgs e)
        {
            var trains = _parent.Trains.Where(t => t.Direction == ParentTrain.Direction).ToArray();
            TrainIndices = linkedTrains.Select(lt => Array.IndexOf(trains, lt)).ToArray();
        }
    }
}