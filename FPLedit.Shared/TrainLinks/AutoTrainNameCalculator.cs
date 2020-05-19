using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared
{
    /// <summary>
    /// Train name calculator, generating names based on a base train name and increment.
    /// </summary>
    [Templating.TemplateSafe]
    public class AutoTrainNameCalculator : ITrainLinkNameCalculator
    {
        internal const string PREFIX = "Auto";
        
        private int increment;
        private TrainNameParts trainName;

        /// <inheritdoc />
        public void Deserialize(IEnumerable<string> parts)
        {
            var partsArr = parts.ToArray();
            if (partsArr.Length != 3)
                throw new ArgumentException("AutoTrainNameCalculator: Length condition was not met (parts.Length != 3)", nameof(parts));
            
            trainName = new TrainNameParts(partsArr[1]);
            increment = int.Parse(partsArr[2]);
        }

        /// <inheritdoc />
        public IEnumerable<string> Serialize() => new []{ PREFIX, trainName.FullName, increment.ToString() };

        /// <inheritdoc />
        public string GetTrainName(int countingIndex) => 
            trainName.BaseName + (trainName.Number + (countingIndex + 1) * increment).ToString(new string('0', trainName.NumberLength));
        
        /// <summary>
        /// Initialize a new empty instance.
        /// </summary>
        public AutoTrainNameCalculator() {}

        /// <summary>
        /// Create a new instance, providing data.
        /// </summary>
        /// <param name="originalName">Name of the original train.</param>
        /// <param name="increment">Increment of the train number.</param>
        /// <remarks>Train number will be extracted with <see cref="TrainNameParts"/>.</remarks>
        public AutoTrainNameCalculator(string originalName, int increment)
        {
            trainName = new TrainNameParts(originalName);
            this.increment = increment;
        }
    }
}