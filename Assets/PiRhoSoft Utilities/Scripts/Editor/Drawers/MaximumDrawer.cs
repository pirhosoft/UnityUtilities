using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(MaximumAttribute))]
	public class MaximumDrawer : PropertyDrawer
	{
		public const string _invalidTypeWarning = "Invalid type for Maximum on field {0}: Maximum can only be applied to int or float fields";

		#region Static Interface

		public static float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public static int Draw(GUIContent label, int value, int maximum)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			return Draw(rect, label, value, maximum);
		} 

		public static int Draw(Rect position, GUIContent label, int value, int maximum)
		{
			var selected = EditorGUI.IntField(position, label, value);
			return Mathf.Min(selected, maximum);
		}
		
		public static float Draw(GUIContent label, float value, float maximum)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			return Draw(rect, label, value, maximum);
		} 

		public static float Draw(Rect position, GUIContent label, float value, float maximum)
		{
			var selected = EditorGUI.FloatField(position, label, value);
			return Mathf.Min(selected, maximum);
		}

		public static void Draw(SerializedProperty property, GUIContent label, int maximum)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			Draw(rect, property, label, maximum);
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label, int maximum)
		{
			Draw(position, property, label, (float)maximum);
		}

		public static void Draw(SerializedProperty property, GUIContent label, float maximum)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			Draw(rect, property, label, maximum);
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label, float maximum)
		{
			EditorGUI.PropertyField(position, property, label);

			if (property.propertyType == SerializedPropertyType.Integer)
				property.intValue = Mathf.Min(Mathf.RoundToInt(maximum), property.intValue);
			else if (property.propertyType == SerializedPropertyType.Float)
				property.floatValue = Mathf.Min(maximum, property.floatValue);
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
			Draw(position, property, label, (attribute as MaximumAttribute).MaximumValue);
		}

		#endregion
	}
}
