#nullable disable
using System;
using System.Collections.Concurrent;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerCache
	{
		private static readonly ConcurrentDictionary<Type, object> typeCache = new ();

		private static readonly ConcurrentDictionary<Type, object> structAsObjectCache = new ();

		public static object GetOrAddClass<T>(Type type, Func<Type, T> adder)
		{
			// return _typeCache.GetOrAdd(type, x => adder(x));
			
			// this implementation is slightly faster than getoradd
            if (typeCache.TryGetValue(type, out var value)) return value;

			// will lock by type object to ensure only one type generator is generated simultaneously
			lock (type)
			{
				value = typeCache.GetOrAdd(type, t => adder(t));
			}

			return value;
		}

		public static object GetOrAddStructAsObject<T>(Type type, Func<Type, T> adder)
		{
			// return _typeCache.GetOrAdd(type, x => adder(x));

			// this implementation is slightly faster than getoradd
            if (structAsObjectCache.TryGetValue(type, out var value)) return value;
			
			// will lock by type object to ensure only one type generator is generated simultaneously
			lock (type)
			{
				value = structAsObjectCache.GetOrAdd(type, t => adder(t));
			}

			return value;
		}
	}
}
