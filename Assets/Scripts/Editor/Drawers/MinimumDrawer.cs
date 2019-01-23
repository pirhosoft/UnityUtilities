using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(MinimumAttribute))]
	public class MinimumDrawer : PropertyDrawer
	{
		public const string _invalidTypeWarning = "Invalid type for Minimum on field {0}: Minimum can only be applied to int or float fields";

		#region Static Interface

		public static float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public static int Draw(GUIContent label, int value, int minimum)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			return Draw(rect, label, value, minimum);
		}

		public static int Draw(Rect position, GUIContent label, int value, int minimum)
		{
			var selected = EditorGUI.IntField(position, label, value);
			return Mathf.Min(selected, minimum);
		}

		public static float Draw(GUIContent label, float value, float minimum)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			return Draw(rect, label, value, minimum);
		}

		public static float Draw(Rect position, GUIContent label, float value, float minimum)
		{
			var selected = EditorGUI.FloatField(position, label, value);
			return Mathf.Min(selected, minimum);
		}

		public static void Draw(SerializedProperty property, GUIContent label, int minimum)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			Draw(rect, property, label, minimum);
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label, int minimum)
		{
			Draw(position, property, label, (float)minimum);
		}

		public static void Draw(SerializedProperty property, GUIContent label, float minimum)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			Draw(rect, property, label, minimum);
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label, float minimum)
		{
			EditorGUI.PropertyField(position, property, label);

			if (property.propertyType == SerializedPropertyType.Integer)
				property.intValue = Mathf.Max(Mathf.RoundToInt(minimum), property.intValue);
			else if (property.propertyType == SerializedPropertyType.Float)
				property.floatValue = Mathf.Max(minimum, property.floatValue);
			else
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight();
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);
			Draw(position, property, label, (attribute as MinimumAttribute).MinimumValue);
		}

		#endregion	
	}
}
