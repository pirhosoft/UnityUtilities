using PiRhoSoft.UtilityEngine;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(EnumButtonsAttribute))]
	public class EnumButtonsDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for EnumButtonsDrawer on field {0}: EnumButtons can only be applied to Enum fields";

		#region Static Interface

		public static float GetHeight(int count, bool useLabel, float minimumButtonWidth)
		{
			var width = useLabel ? RectHelper.CurrentFieldWidth : RectHelper.CurrentViewWidth;
			GetButtonInfo(width, minimumButtonWidth, count, out float buttonWidth, out int rows, out int columns);
			return EditorGUIUtility.singleLineHeight * rows;
		}

		public static int Draw(GUIContent label, int value, Type type, int count, float minimumButtonWidth)
		{
			var height = GetHeight(count, !string.IsNullOrEmpty(label.text), minimumButtonWidth);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return Draw(rect, label, value, type, minimumButtonWidth);
		}

		public static int Draw(GUIContent label, int value, bool flags, Array values, GUIContent[] names, float minimumButtonWidth)
		{
			var height = GetHeight(values.Length, !string.IsNullOrEmpty(label.text), minimumButtonWidth);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return Draw(rect, label, value, flags, values, names, minimumButtonWidth);
		}

		public static int Draw(Rect position, GUIContent label, int value, Type type, float minimumButtonWidth)
		{
			var flags = TypeHelper.GetAttribute<FlagsAttribute>(type) != null;
			var values = Enum.GetValues(type);
			var names = Enum.GetNames(type).Select(name => new GUIContent(ObjectNames.NicifyVariableName(name))).ToArray();

			return Draw(position, label, value, flags, values, names, minimumButtonWidth);
		}

		public static int Draw(Rect position, GUIContent label, int value, bool flags, Array values, GUIContent[] names, float minimumButtonWidth)
		{
			var rect = EditorGUI.PrefixLabel(position, label);

			GetButtonInfo(rect.width, minimumButtonWidth, values.Length, out float buttonWidth, out int rows, out int columns);

			return flags
				? DrawButtonFlags(rect, buttonWidth, rows, columns, value, values, names)
				: DrawButtons(rect, buttonWidth, rows, columns, value, values, names);
		}

		public static void Draw(SerializedProperty property, GUIContent label, Type type, int count, float minimumButtonWidth)
		{
			var height = GetHeight(count, !string.IsNullOrEmpty(label.text), minimumButtonWidth);
			var rect = EditorGUILayout.GetControlRect(false, height);

			Draw(rect, property, label, type, minimumButtonWidth);
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label, Type type, float minimumButtonWidth)
		{
			if (property.propertyType == SerializedPropertyType.Enum)
			{
				property.intValue = Draw(position, label, property.intValue, type, minimumButtonWidth);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Enum)
			{
				return GetHeight(Enum.GetValues(fieldInfo.FieldType).Length, !string.IsNullOrEmpty(label.text), (attribute as EnumButtonsAttribute).MinimumWidth);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				return EditorGUI.GetPropertyHeight(property);
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);
			Draw(position, property, label, fieldInfo.FieldType, (attribute as EnumButtonsAttribute).MinimumWidth);
		}

		#endregion

		#region Drawing

		private static void GetButtonInfo(float width, float minimumButtonWidth, int count, out float buttonWidth, out int rows, out int columns)
		{
			if (width < minimumButtonWidth)
			{
				buttonWidth = width;
				rows = count;
				columns = 1;
			}
			else
			{
				rows = 1;
				columns = count;
				buttonWidth = width / count;

				while (buttonWidth < minimumButtonWidth)
				{
					rows++;
					columns = Mathf.CeilToInt(count / (float)rows);
					buttonWidth = width / columns;
				}
			}
		}

		private static int DrawButtons(Rect position, float buttonWidth, int rows, int columns, int value, Array values, GUIContent[] names)
		{
			var index = 0;
			for (var row = 0; row < rows; row++)
			{
				var rowRect = RectHelper.TakeHeight(ref position, EditorGUIUtility.singleLineHeight);

				for (var i = 0; i < columns && index < values.Length; i++, index++)
				{
					var enumValue = (int)values.GetValue(index);
					var pressed = value == enumValue;
					var rect = RectHelper.TakeWidth(ref rowRect, buttonWidth);

					if (GUI.Toggle(rect, pressed, names[index], GUI.skin.button))
						value = enumValue;
				}
			}

			return value;
		}

		private static int DrawButtonFlags(Rect position, float buttonWidth, int rows, int columns, int value, Array values, GUIContent[] names)
		{
			var index = 0;
			for (var row = 0; row < rows; row++)
			{
				var rowRect = RectHelper.TakeHeight(ref position, EditorGUIUtility.singleLineHeight);

				for (var i = 0; i < columns && index < values.Length; i++, index++)
				{
					var enumValue = (int)values.GetValue(index);
					var pressed = ((value & enumValue) == enumValue && enumValue != 0) || (value == 0 && enumValue == 0);
					var rect = RectHelper.TakeWidth(ref rowRect, buttonWidth);

					if (GUI.Toggle(rect, pressed, names[index], GUI.skin.button))
					{
						if (enumValue == 0)
							value = 0;
						else
							value |= enumValue;
					}
					else
					{
						if (enumValue != ~0)
							value &= ~enumValue;
					}
				}
			}

			return value;
		}

		#endregion
	}
}
