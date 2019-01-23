﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class TypeList
	{
		public Type BaseType;
		public bool HasNone;

		public GUIContent[] Names;
		public List<Type> Types;

		#region Lookup

		public int GetIndex(Type type)
		{
			var index = Types.IndexOf(type);

			if (HasNone)
			{
				if (index >= 0) index += 2; // skip 'None' and separator
				else index = 0;
			}

			return index;
		}

		public Type GetType(int index)
		{
			if (HasNone) index -= 2;  // skip 'None' and separator
			return index >= 0 && index < Types.Count ? Types[index] : null;
		}

		#endregion
	}

	public static class TypeHelper
	{
		private static Dictionary<string, TypeList> _derivedTypeLists = new Dictionary<string, TypeList>();

		#region Attributes

		public static bool HasAttribute<AttributeType>(Type type) where AttributeType : Attribute
		{
			return GetAttribute<AttributeType>(type) != null;
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

		public static bool HasAttribute(Type type, Type attributeType)
		{
			return GetAttribute(type, attributeType) != null;
		}

		public static Attribute GetAttribute(Type type, Type attributeType)
		{
			var attributes = type.GetCustomAttributes(attributeType, false);
			return attributes != null && attributes.Length > 0 ? attributes[0] as Attribute : null;
		}

		public static Attribute GetAttribute(FieldInfo field, Type attributeType)
		{
			var attributes = field.GetCustomAttributes(attributeType, false);
			return attributes != null && attributes.Length > 0 ? attributes[0] as Attribute : null;
		}

		#endregion

		#region Creation

		public static T CreateInstance<T>(Type type) where T : class
		{
			if (type != null && !type.IsAbstract && typeof(T).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
				return Activator.CreateInstance(type) as T;

			return null;
		}

		public static bool IsCreatableAs<BaseType>(Type type)
		{
			return IsCreatableAs(typeof(BaseType), type);
		}

		public static bool IsCreatableAs(Type baseType, Type type)
		{
			return baseType.IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null;
		}

		#endregion

		#region Listing

		public static List<Type> ListDerivedTypes<BaseType>()
		{
			return FindTypes(type => IsCreatableAs<BaseType>(type)).ToList();
		}

		public static List<Type> ListDerivedTypes(Type baseType)
		{
			return FindTypes(type => IsCreatableAs(baseType, type)).ToList();
		}

		public static List<Type> ListTypesWithAttribute<AttributeType>() where AttributeType : Attribute
		{
			return FindTypes(type => HasAttribute<AttributeType>(type)).ToList();
		}

		public static List<Type> ListTypesWithAttribute(Type attributeType)
		{
			return FindTypes(type => HasAttribute(type, attributeType)).ToList();
		}

		public static IEnumerable<Type> FindTypes(Func<Type, bool> predicate)
		{
			// There are a lot of assemblies so it might make sense to filter the list a bit. There isn't a specific
			// way to do that, but something like this would work: https://stackoverflow.com/questions/5160051/c-sharp-how-to-get-non-system-assemblies

			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(assembly => !assembly.IsDynamic) // GetExportedTypes throws an exception when called on dynamic assemblies
				.SelectMany(t => t.GetExportedTypes())
				.Where(predicate);
		}

		public static TypeList GetTypeList<T>(bool includeNone)
		{
			return GetTypeList(typeof(T), includeNone);
		}

		public static TypeList GetTypeList(Type baseType, bool includeNone)
		{
			// include the settings in the name so lists of the same type can be created with different settings
			var listName = string.Format("{0}-{1}", includeNone, baseType.AssemblyQualifiedName);

			TypeList list;
			if (!_derivedTypeLists.TryGetValue(listName, out list))
			{
				list = new TypeList { BaseType = baseType, HasNone = includeNone };
				_derivedTypeLists.Add(listName, list);
			}

			if (list.Types == null)
			{
				var types = ListDerivedTypes(baseType);
				var ordered = types.Select(type => new ListedType(types, baseType, type)).OrderBy(type => type.Name);
				var none = includeNone ? new List<GUIContent> { new GUIContent("None"), new GUIContent("") } : new List<GUIContent>();

				list.Types = ordered.Select(type => type.Type).ToList();
				list.Names = Enumerable.Concat(none, ordered.Select(type => new GUIContent(type.Name))).ToArray();
			}

			return list;
		}

		private class ListedType
		{
			public Type Type;
			public string Name;

			public ListedType(IEnumerable<Type> types, Type rootType, Type type)
			{
				Type = type;
				Name = Type.Name;

				// repeat the name for types that have derivations so they appear in their own submenu (otherwise they wouldn't be selectable)
				if (type != rootType)
				{
					if (types.Any(t => t.BaseType == type))
						Name += "/" + Type.Name;

					type = type.BaseType;
				}

				// prepend all parent type names up to but not including the root type
				while (type != rootType)
				{
					Name = type.Name + "/" + Name;
					type = type.BaseType;
				}
			}
		}

		#endregion
	}
}
