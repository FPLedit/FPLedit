#nullable disable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	/// <summary>
	/// Safe types are types, which can be copied without real cloning. e.g. simple structs or strings (it is immutable)
	/// </summary>
	internal static class DeepClonerSafeTypes
	{
        private static readonly ConcurrentDictionary<Type, bool> knownTypes = new ();

		static DeepClonerSafeTypes()
		{
			foreach (
				var x in
					new[]
						{
							typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
							typeof(float), typeof(double), typeof(decimal), typeof(char), typeof(string), typeof(bool), typeof(DateTime),
							typeof(IntPtr), typeof(UIntPtr), typeof(Guid),
							// do not clone such native type
							Type.GetType("System.RuntimeType"),
							Type.GetType("System.RuntimeTypeHandle"),
							StringComparer.Ordinal.GetType(),
							StringComparer.CurrentCulture.GetType(), // CultureAwareComparer - can be same
						}) knownTypes.TryAdd(x, true);
		}

		private static bool CanReturnSameType(Type type, HashSet<Type> processingTypes)
		{
			if (knownTypes.TryGetValue(type, out bool isSafe))
				return isSafe;
			
			// enums are safe
			// pointers (e.g. int*) are unsafe, but we cannot do anything with it except blind copy
			if (type.IsEnum || type.IsPointer)
			{
				knownTypes.TryAdd(type, true);
				return true;
			}

			// do not copy db null
			if (type.FullName.StartsWith("System.DBNull"))
			{
				knownTypes.TryAdd(type, true);
				return true;
			}

			if (type.FullName.StartsWith("System.RuntimeType"))
			{
				knownTypes.TryAdd(type, true);
				return true;
			}
			
			if (type.FullName.StartsWith("System.Reflection.") && Equals(type.GetTypeInfo().Assembly, typeof(PropertyInfo).GetTypeInfo().Assembly))
			{
				knownTypes.TryAdd(type, true);
				return true;
			}

			if (type.IsSubclassOfTypeByName("CriticalFinalizerObject"))
			{
				knownTypes.TryAdd(type, true);
				return true;
			}
			
			// better not to touch ms dependency injection
			if (type.FullName.StartsWith("Microsoft.Extensions.DependencyInjection."))
			{
				knownTypes.TryAdd(type, true);
				return true;
			}

			if (type.FullName == "Microsoft.EntityFrameworkCore.Internal.ConcurrencyDetector")
			{
				knownTypes.TryAdd(type, true);
				return true;
			}

			// default comparers should not be cloned due possible comparison EqualityComparer<T>.Default == comparer
			if (type.FullName.Contains("EqualityComparer"))
			{
				if (type.FullName.StartsWith("System.Collections.Generic.GenericEqualityComparer`")
				    || type.FullName.StartsWith("System.Collections.Generic.ObjectEqualityComparer`")
				    || type.FullName.StartsWith("System.Collections.Generic.EnumEqualityComparer`")
				    || type.FullName.StartsWith("System.Collections.Generic.NullableEqualityComparer`")
				    || type.FullName == "System.Collections.Generic.ByteEqualityComparer")
				{
					knownTypes.TryAdd(type, true);
					return true;
				}
			}

			// classes are always unsafe (we should copy it fully to count references)
			if (!type.IsValueType)
			{
				knownTypes.TryAdd(type, false);
				return false;
			}

			if (processingTypes == null)
				processingTypes = new HashSet<Type>();

			// structs cannot have a loops, but check it anyway
			processingTypes.Add(type);

			List<FieldInfo> fi = new List<FieldInfo>();
			var tp = type;
			do
			{
				fi.AddRange(tp.GetDeclaredFields());
				tp = tp.GetTypeInfo().BaseType;
			}
			while (tp != null);

			foreach (var fieldInfo in fi)
			{
				// type loop
				var fieldType = fieldInfo.FieldType;
				if (processingTypes.Contains(fieldType))
					continue;

				// not safe and not not safe. we need to go deeper
				if (!CanReturnSameType(fieldType, processingTypes))
				{
					knownTypes.TryAdd(type, false);
					return false;
				}
			}

			knownTypes.TryAdd(type, true);
			return true;
		}

		public static bool CanReturnSameObject(Type type)
		{
			return CanReturnSameType(type, null);
		}
	}
}
