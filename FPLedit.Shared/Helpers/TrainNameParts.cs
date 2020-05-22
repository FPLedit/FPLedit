using System;

namespace FPLedit.Shared.Helpers
{
    /// <summary>
    /// Helper struct to extract train base name and train number.
    /// </summary>
    /// <remarks>
    /// <para>This will (only) find and extract numeric parts at the end of the gieven full train name.</para>
    /// <para>For example: "P 01" will get split in BaseName: "P ", Number: 1, NumberLength: 2.</para>
    /// <para>Whitespace at the end or the beginning will be ignored and removed.</para>
    /// </remarks>
    [Templating.TemplateSafe]
    public readonly struct TrainNameParts
    {
        /// <summary>
        /// Initializes a new instance with the given full train name.
        /// </summary>
        public TrainNameParts(string fullTrainName) : this()
        {
            FullName = fullTrainName;

            (Number, NumberLength) = RemoveNamePrefix(FullName, out var prefix);
            BaseName = prefix;
        }

        /// <summary>
        /// The non-numeric part of the train name.
        /// </summary>
        public string BaseName { get; }
        /// <summary>
        /// Extracted numeric part of the train name.
        /// </summary>
        public int Number { get; }
        /// <summary>
        /// Length (in digits) of the numeric part of the train name.
        /// </summary>
        public int NumberLength { get; }
        /// <summary>
        /// The full original train name, as supplied to this struct.
        /// </summary>
        public string FullName { get; }

        public bool CompareTo(TrainNameParts np2, bool excludePrefix)
        {
            if (excludePrefix)
                return BaseName == np2.BaseName && Number.CompareTo(np2.Number) > 0;
            return string.Compare(FullName, np2.FullName, StringComparison.Ordinal) > 0;
        }
            
        private static (int startNumber, int numberLength) RemoveNamePrefix(string name, out string prefix)
        {
            var nameBase = name.Trim();
            var array = nameBase.ToCharArray();

            int i = array.Length - 1;
            while (i >= 0 && char.IsDigit(array[i]))
                i--;

            var numberLength = array.Length - i - 1;
            
            prefix = nameBase.Substring(0, i + 1);
            var num = nameBase.Substring(i + 1, nameBase.Length - i - 1);
            int.TryParse(num, out int start); // Startnummer
            
            return (start, numberLength);
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is TrainNameParts tnp)
                return tnp.FullName == FullName;
            return false;
        }
        public override int GetHashCode() => FullName.GetHashCode();
        public static bool operator ==(TrainNameParts left, TrainNameParts right) => left.Equals(right);
        public static bool operator !=(TrainNameParts left, TrainNameParts right) => !(left == right);
    }
}