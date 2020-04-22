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
    
    [Templating.TemplateSafe]
    public interface ITrainLinkNameCalculator
    {
        void Deserialize(IEnumerable<string> parts);
        IEnumerable<string> Serialize();
        string GetTrainName(int countingIndex);
    }

    public static class TrainLinkNameCalculatorManager
    {
        private static readonly Dictionary<string, Type> registered = new Dictionary<string, Type>()
        {
            [AutoTrainNameCalculator.PREFIX] = typeof(AutoTrainNameCalculator),
            [SpecialTrainNameCalculator.PREFIX] = typeof(SpecialTrainNameCalculator),
        };

        public static ITrainLinkNameCalculator Deserialize(string trainNamingScheme)
        {
            //HACK: This is intentional and may lead to a crash when a part contains a semicolon. But jTrainGraph does is like that.
            var parts = trainNamingScheme.Split(';');
            if (parts.Length < 2)
                throw new FormatException("Train link naming scheme is to short: " + trainNamingScheme);
            
            if (registered.TryGetValue(parts[0], out var tnct) && typeof(ITrainLinkNameCalculator).IsAssignableFrom(tnct))
            {
                var tnc = (ITrainLinkNameCalculator) Activator.CreateInstance(tnct);
                tnc.Deserialize(parts);
                return tnc;
            }

            return null;
        }

        //HACK: This is intentional and may lead to a crash when a part contains a semicolon. But jTrainGraph does is like that.
        public static string Serialize(ITrainLinkNameCalculator tnc) => string.Join(";", tnc.Serialize());
    }

    public class AutoTrainNameCalculator : ITrainLinkNameCalculator
    {
        internal const string PREFIX = "Auto";
        
        private int increment;
        private TrainNameParts trainName;

        public void Deserialize(IEnumerable<string> parts)
        {
            var partsArr = parts.ToArray();
            if (partsArr.Length != 3)
                throw new ArgumentException("AutoTrainNameCalculator: Length condition was not met (parts.Length != 3)", nameof(parts));
            
            trainName = new TrainNameParts(partsArr[1]);
            increment = int.Parse(partsArr[2]);
        }

        public IEnumerable<string> Serialize() => new []{ PREFIX, trainName.FullName, increment.ToString() };

        public string GetTrainName(int countingIndex) => 
            trainName.BaseName + (trainName.Number + (countingIndex + 1) * increment).ToString(new string('0', trainName.NumberLength));
    }
    
    public class SpecialTrainNameCalculator : ITrainLinkNameCalculator
    {
        internal const string PREFIX = "Special";
        
        private List<string> names;
        
        public void Deserialize(IEnumerable<string> parts)
        {
            if (parts.Count() < 2)
                throw new ArgumentException("parts is not long enough!");
            names = parts.Skip(1).ToList();
        }

        public IEnumerable<string> Serialize() => new[] { PREFIX }.Concat(names);

        public string GetTrainName(int countingIndex)
        {
            return names[countingIndex];
        }
    }
}