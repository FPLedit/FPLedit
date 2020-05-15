using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
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