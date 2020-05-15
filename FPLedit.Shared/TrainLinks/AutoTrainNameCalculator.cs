using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
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
}