using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public abstract class PropertyControl
	{
		public abstract void Setup(SerializedProperty property, FieldInfo fieldInfo);
		public abstract float GetHeight(SerializedProperty property, GUIContent label);
		public abstract void Draw(Rect position, SerializedProperty property, GUIContent label);

		public static T GetObject<T>(SerializedProperty property) where T : class
		{
			var obj = (object)property.serializedObject.targetObject;
			var elements = property.propertyPath.Replace("Array.data[", "[").Split('.');

			foreach (var element in elements)
			{
				if (element.StartsWith("["))
				{
					var indexString = element.Substring(1, element.Length - 2);
					var index = Convert.ToInt32(indexString);

					obj = GetIndexed(obj, index);
				}
				else
				{
					obj = obj.GetType().GetField(element, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
				}
			}

			return obj as T;
		}

		private static object GetIndexed(object obj, int index)
		{
			if (obj is Array array)
				return array.GetValue(index);

			if (obj is IList list)
				return list[index];

			return null;
		}
	}
}
