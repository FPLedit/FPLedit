using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// Train name calculator with a specified name for each train.
    /// </summary>
    [Templating.TemplateSafe]
    public class SpecialTrainNameCalculator : ITrainLinkNameCalculator
    {
        internal const string PREFIX = "Special";
        
        private string[] names;
        
        /// <inheritdoc />
        public void Deserialize(IEnumerable<string> parts)
        {
            if (parts.Count() < 2)
                throw new ArgumentException("parts is not long enough!");
            names = parts.Skip(1).ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<string> Serialize() => new[] { PREFIX }.Concat(names);

        /// <inheritdoc />
        public string GetTrainName(int countingIndex)
        {
            return names[countingIndex];
        }
        
        /// <summary>
        /// Initialize a new empty instance.
        /// </summary>
        public SpecialTrainNameCalculator() {}

        /// <summary>
        /// Create a new instance, providing data.
        /// </summary>
        public SpecialTrainNameCalculator(string[] names)
        {
            this.names = names;
        }
    }
}