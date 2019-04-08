using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.UtilityEditor
{
	public class TypeList
	{
		public Type BaseType;
		public bool HasNone;

		public GUIContent[] Names;
		public List<Type> Types;

		public SelectionTree Tree;

		#region Lookup

		public int GetIndex(Type type)
		{
			var index = Types.IndexOf(type);

			if (HasNone)
			{
				if (index >= 0) index++; // skip 'None'
				else index = 0;
			}

			return index;
		}

		public Type GetType(int index)
		{
			if (HasNone) index--;  // skip 'None'
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

		public static List<Type> ListDerivedTypes<BaseType>(bool includeAbstract)
		{
			return ListDerivedTypes(typeof(BaseType), includeAbstract);
		}

		public static List<Type> ListDerivedTypes(Type baseType, bool includeAbstract)
		{
			return FindTypes(type => includeAbstract ? baseType.IsAssignableFrom(type) : IsCreatableAs(baseType, type)).ToList();
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

		public static TypeList GetTypeList<T>(bool includeNone, bool includeAbstract)
		{
			return GetTypeList(typeof(T), includeNone, includeAbstract);
		}

		public static TypeList GetTypeList(Type baseType, bool includeNone, bool includeAbstract)
		{
			// include the settings in the name so lists of the same type can be created with different settings
			var listName = string.Format("{0}-{1}-{2}", includeNone, includeAbstract, baseType.AssemblyQualifiedName);
			
			if (!_derivedTypeLists.TryGetValue(listName, out var list))
			{
				list = new TypeList { BaseType = baseType, HasNone = includeNone };
				_derivedTypeLists.Add(listName, list);
			}

			if (list.Types == null)
			{
				var types = ListDerivedTypes(baseType, includeAbstract);
				var ordered = types.Select(type => new ListedType(types, baseType, type)).OrderBy(type => type.Name);
				var none = includeNone ? new List<GUIContent> { new GUIContent("None") } : new List<GUIContent>();

				list.Types = ordered.Select(type => type.Type).ToList();
				list.Names = none.Concat(ordered.Select(type => new GUIContent(type.Name, AssetPreview.GetMiniTypeThumbnail(type.Type)))).ToArray();

				list.Tree = new SelectionTree();
				list.Tree.Add(baseType.Name, list.Names);
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

		#region Serialization

		// these functions are based on Unity's serialization rules defined here: https://docs.unity3d.com/Manual/script-Serialization.html

		public static List<Type> SerializableTypes = new List<Type>
		{
			typeof(bool),
			typeof(sbyte), typeof(short), typeof(int), typeof(long),
			typeof(byte), typeof(ushort), typeof(uint), typeof(ulong),
			typeof(float), typeof(double), typeof(decimal),
			typeof(char), typeof(string),
			typeof(Vector2), typeof(Vector3), typeof(Vector4),
			typeof(Quaternion), typeof(Matrix4x4),
			typeof(Color), typeof(Color32), typeof(Gradient),
			typeof(Rect), typeof(RectOffset),
			typeof(LayerMask), typeof(AnimationCurve), typeof(GUIStyle)
		};

		public static bool IsSerializable(FieldInfo field)
		{
			var included = field.IsPublic || GetAttribute<SerializeField>(field) != null;
			var excluded = GetAttribute<NonSerializedAttribute>(field) != null;
			var compatible = !field.IsStatic && !field.IsLiteral && !field.IsInitOnly && IsSerializable(field.FieldType);

			return included && !excluded && compatible;
		}

		public static bool IsSerializable(Type type)
		{
			return IsSerializable(type, false);
		}

		private static bool IsSerializable(Type type, bool inner)
		{
			if (type.IsAbstract)
				return false; // covers static as well

			if (type.IsEnum)
				return true;

			if (type.IsGenericType)
				return !inner && type.GetGenericTypeDefinition() == typeof(List<>) && IsSerializable(type.GetGenericArguments()[0], true);

			if (type.IsArray && IsSerializable(type.GetElementType(), true))
				return !inner;

			if (typeof(Object).IsAssignableFrom(type))
				return true;

			if (GetAttribute<SerializableAttribute>(type) != null)
				return true;

			return SerializableTypes.Contains(type);
		}

		#endregion
	}
}
