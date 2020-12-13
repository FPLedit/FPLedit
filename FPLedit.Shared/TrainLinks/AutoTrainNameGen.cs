using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared.TrainLinks
{
    /// <summary>
    /// Train name calculator, generating names based on a base train name and increment.
    /// </summary>
    [Templating.TemplateSafe]
    public class AutoTrainNameGen : ITrainNameGen
    {
        internal const string PREFIX = "Auto";
        
        /// <summary>
        /// Increment of the train number for each created linked train.
        /// </summary>
        public int Increment { get; private set; }
        
        /// <summary>
        /// Base Name that is used to calculate the names of the linked trains.
        /// </summary>
        /// <remarks>This is not neccessarily the same as the name of the parent train!</remarks>
        public TrainNameParts BaseTrainName { get; private set; }

        /// <inheritdoc />
        public void Deserialize(IEnumerable<string> parts)
        {
            var partsArr = parts.ToArray();
            if (partsArr.Length != 3)
                throw new ArgumentException("AutoTrainNameCalculator: Length condition was not met (parts.Length != 3)", nameof(parts));
            
            BaseTrainName = new TrainNameParts(partsArr[1]);
            Increment = int.Parse(partsArr[2]);
        }

        /// <inheritdoc />
        public IEnumerable<string> Serialize() => new []{ PREFIX, BaseTrainName.FullName, Increment.ToString() };

        /// <inheritdoc />
        public string GetTrainName(int countingIndex) => 
            BaseTrainName.BaseName + (BaseTrainName.Number + (countingIndex + 1) * Increment).ToString(new string('0', BaseTrainName.NumberLength));
        
        /// <summary>
        /// Initialize a new empty instance.
        /// </summary>
        public AutoTrainNameGen() {}

        /// <summary>
        /// Create a new instance, providing data.
        /// </summary>
        /// <param name="originalName">Name of the original train.</param>
        /// <param name="increment">Increment of the train number.</param>
        /// <remarks>Train number will be extracted with <see cref="TrainNameParts"/>.</remarks>
        public AutoTrainNameGen(string originalName, int increment)
        {
            BaseTrainName = new TrainNameParts(originalName);
            this.Increment = increment;
        }
    }
}