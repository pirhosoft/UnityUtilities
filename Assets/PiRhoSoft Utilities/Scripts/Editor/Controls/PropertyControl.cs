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
	}
}
