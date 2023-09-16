#nullable disable
using System;
using System.Linq;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class ReflectionHelper
	{
        public static PropertyInfo[] GetPublicProperties(this Type t) => t.GetTypeInfo().DeclaredProperties.ToArray();

        public static FieldInfo[] GetDeclaredFields(this Type t) => t.GetTypeInfo().DeclaredFields.Where(x => !x.IsStatic).ToArray();

        public static ConstructorInfo[] GetPublicConstructors(this Type t) => t.GetTypeInfo().DeclaredConstructors.ToArray();

        public static bool IsSubclassOfTypeByName(this Type t, string typeName)
		{
			while (t != null)
			{
				if (t.Name == typeName)
					return true;
				t = t.GetTypeInfo().BaseType;
			}

			return false;
		}
    }
}
