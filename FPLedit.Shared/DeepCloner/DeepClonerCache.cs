#nullable disable
using System;
using System.Collections.Concurrent;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerCache
	{
		private static readonly ConcurrentDictionary<Type, object> _typeCache = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, object> _structAsObjectCache = new ConcurrentDictionary<Type, object>();

		public static object GetOrAddClass<T>(Type type, Func<Type, T> adder)
		{
			// return _typeCache.GetOrAdd(type, x => adder(x));
			
			// this implementation is slightly faster than getoradd
			object value;
			if (_typeCache.TryGetValue(type, out value)) return value;

			// will lock by type object to ensure only one type generator is generated simultaneously
			lock (type)
			{
				value = _typeCache.GetOrAdd(type, t => adder(t));
			}

			return value;
		}

		public static object GetOrAddStructAsObject<T>(Type type, Func<Type, T> adder)
		{
			// return _typeCache.GetOrAdd(type, x => adder(x));

			// this implementation is slightly faster than getoradd
			object value;
			if (_structAsObjectCache.TryGetValue(type, out value)) return value;
			
			// will lock by type object to ensure only one type generator is generated simultaneously
			lock (type)
			{
				value = _structAsObjectCache.GetOrAdd(type, t => adder(t));
			}

			return value;
		}

		/// <summary>
		/// This method can be used when we switch between safe / unsafe variants (for testing)
		/// </summary>
		public static void ClearCache()
		{
			_typeCache.Clear();
			_structAsObjectCache.Clear();
		}
	}
}
