using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public abstract class ObjectControl<T> : PropertyControl where T : class
	{
		public abstract void Setup(T target, SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute);
		public abstract float GetHeight(GUIContent label);
		public abstract void Draw(Rect position, GUIContent label);
		
		public sealed override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			var target = PropertyHelper.GetObject<T>(property);
			Setup(target, property, fieldInfo, attribute);
		}

		public sealed override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight(label);
		}

		public sealed override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			// switch the undo scope from the property to the object

			using (new EditObjectScope(property.serializedObject))
			{
				using (new UndoScope(property.serializedObject.targetObject, false))
					Draw(position, label);
			}
		}

		public void Draw(GUIContent label)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(true, height);

			Draw(rect, label);
		}
	}
}
