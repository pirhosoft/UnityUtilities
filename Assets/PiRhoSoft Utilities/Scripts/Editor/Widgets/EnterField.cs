using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class EnterField
	{
		public static bool DrawInt(string controlName, GUIContent label, ref int value)
		{
			var height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return DrawInt(controlName, rect, label, ref value);
		}

		public static bool DrawInt(string controlName, Rect position, GUIContent label, ref int value)
		{
			GUI.SetNextControlName(controlName);
			value = EditorGUI.IntField(position, label, value);
			return WasEnterPressed(controlName);
		}

		public static bool DrawFloat(string controlName, GUIContent label, ref float value)
		{
			var height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Float, label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return DrawFloat(controlName, rect, label, ref value);
		}

		public static bool DrawFloat(string controlName, Rect position, GUIContent label, ref float value)
		{
			GUI.SetNextControlName(controlName);
			value = EditorGUI.FloatField(position, label, value);
			return WasEnterPressed(controlName);
		}

		public static bool DrawString(string controlName, GUIContent label, ref string text)
		{
			var height = EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return DrawString(controlName, rect, label, ref text);
		}

		public static bool DrawString(string controlName, Rect position, GUIContent label, ref string text)
		{
			GUI.SetNextControlName(controlName);
			text = EditorGUI.TextField(position, label, text);
			return WasEnterPressed(controlName);
		}

		private static bool WasEnterPressed(string controlName)
		{
			return Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return) && GUI.GetNameOfFocusedControl() == controlName;
		}
	}
}
