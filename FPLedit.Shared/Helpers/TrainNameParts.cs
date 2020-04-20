using System;

namespace FPLedit.Shared.Helpers
{
    [Templating.TemplateSafe]
    public readonly struct TrainNameParts
    {
        public TrainNameParts(string fullTrainName) : this()
        {
            FullName = fullTrainName;

            Number = RemoveNamePrefix(FullName, out var prefix);
            BaseName = prefix;
        }

        public string BaseName { get; }
        public int Number { get; }
        public string FullName { get; }

        public bool CompareTo(TrainNameParts np2, bool excludePrefix)
        {
            if (excludePrefix)
                return BaseName == np2.BaseName && Number.CompareTo(np2.Number) > 0;
            return string.Compare(FullName, np2.FullName, StringComparison.Ordinal) > 0;
        }
            
        private static int RemoveNamePrefix(string name, out string prefix)
        {
            var nameBase = name.Trim();
            var array = nameBase.ToCharArray();

            int i = array.Length - 1;
            while (i >= 0 && char.IsDigit(array[i]))
                i--;
            
            prefix = nameBase.Substring(0, i + 1);
            var num = nameBase.Substring(i + 1, nameBase.Length - i - 1);
            int.TryParse(num, out int start); // Startnummer
            
            return start;
        }
    }
}