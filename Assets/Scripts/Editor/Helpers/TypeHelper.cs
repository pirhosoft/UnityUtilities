using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class TypeList
	{
		public int Count;
		public Type Type;
		public GUIContent[] Names;
		public Type[] Types;
	}

	public class ListedType
	{
		public Type Type;
		public string Name;

		public ListedType(Type type)
		{
			Type = type;
			Name = type.Name;
		}

		public ListedType(string name)
		{
			Type = null;
			Name = name;
		}

		internal ListedType(IEnumerable<Type> types, Type rootType, Type type)
		{
			Type = type;
			Name = Type.Name;

			if (type != rootType)
			{
				if (types.Any(t => t.BaseType == type))
					Name += "/" + Type.Name;

				type = type.BaseType;
			}

			while (type != rootType)
			{
				Name = type.Name + "/" + Name;
				type = type.BaseType;
			}
		}
	}

	public static class TypeHelper
	{
		private static Dictionary<Type, TypeList> _derivedTypeLists = new Dictionary<Type, TypeList>();

		public static TypeList GetTypeList<T>(params ListedType[] prependedTypes)
		{
			TypeList list;
			if (!_derivedTypeLists.TryGetValue(typeof(T), out list))
			{
				list = new TypeList { Type = typeof(T) };
				_derivedTypeLists.Add(typeof(T), list);
			}

			if (list.Types == null)
			{
				var allTypes = ListDerivedTypes<T>();
				var listedTypes = allTypes.Select(type => new ListedType(allTypes, typeof(T), type)).OrderBy(type => type.Name);

				var types = prependedTypes.Concat(listedTypes);

				list.Types = types.Select(type => type.Type).ToArray();
				list.Names = types.Select(type => new GUIContent(type.Name)).ToArray();
				list.Count = list.Types.Length;
			}

			return list;
		}

		public static AttributeType GetAttribute<AttributeType>(Type type) where AttributeType : Attribute
		{
			var attributes = type.GetCustomAttributes(typeof(AttributeType), false);
			return attributes != null && attributes.Length > 0 ? attributes[0] as AttributeType : null;
		}

		public static AttributeType GetAttribute<AttributeType>(FieldInfo field) where AttributeType : Attribute
		{
			var attributes = field.GetCustomAttributes(typeof(AttributeType), false);
			return attributes != null && attributes.Length > 0 ? attributes[0] as AttributeType : null;
		}

		public static T CreateInstance<T>(Type type) where T : class
		{
			if (type != null && !type.IsAbstract && typeof(T).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
				return Activator.CreateInstance(type) as T;

			return null;
		}

		public static bool IsCreatableAs(Type baseType, Type type)
		{
			return baseType.IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null;
		}

		public static bool IsCreatableAs<BaseType>(Type type)
		{
			return IsCreatableAs(typeof(BaseType), type);
		}

		public static IEnumerable<Type> ListTypes(Func<Type, bool> predicate)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(assembly => !assembly.IsDynamic)
				.SelectMany(t => t.GetTypes())
				.Where(predicate);
		}

		public static List<Type> ListDerivedTypes(Type baseType)
		{
			// TODO: There are a lot of assemblies so it might make sense to filter the list a bit. There isn't a
			// specific way to do that, but something like this might work: https://stackoverflow.com/questions/5160051/c-sharp-how-to-get-non-system-assemblies

			var types = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic);

			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetExportedTypes())
				{
					if (IsCreatableAs(baseType, type))
						types.Add(type);
				}
			}

			return types;
		}

		public static List<Type> ListDerivedTypes<BaseType>()
		{
			return ListDerivedTypes(typeof(BaseType));
		}

		public static List<Type> ListDerivedTypesWithAttribute<BaseType, AttributeType>() where AttributeType : Attribute
		{
			var types = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic);

			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetExportedTypes())
				{
					if (IsCreatableAs<BaseType>(type))
					{
						var attribute = GetAttribute<AttributeType>(type);
						if (attribute != null)
							types.Add(type);
					}
				}
			}

			return types;
		}

		public static List<Type> ListEnumsWithAttribute<AttributeType>() where AttributeType : Attribute
		{
			var types = new List<Type>();

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetExportedTypes())
				{
					if (type.IsEnum)
					{
						var attribute = GetAttribute<AttributeType>(type);
						if (attribute != null)
							types.Add(type);
					}
				}
			}

			return types;
		}
	}
}
