#nullable disable
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Force.DeepCloner.Helpers
{
    internal class DeepCloneState
    {
        private class CustomEqualityComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }

        private static readonly CustomEqualityComparer comparer = new ();

        private readonly Dictionary<object, object> loops = new (comparer);

        public object GetKnownRef(object from)
        {
            if (loops.TryGetValue(from, out var value))
                return value;
            return null;
        }

        public void AddKnownRef(object from, object to)
        {
            loops[from] = to;
        }
    }
}
