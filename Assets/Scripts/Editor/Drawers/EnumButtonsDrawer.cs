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
		private const string _invalidTypeWarning = "Invalid type for EnumButtonsDisplayDrawer on field {0}: EnumButtonsDisplay can only be applied to Enum fields";
		private const float _minWidth = 40.0f;

		#region Static Interface

		public static float GetHeight(int count, bool useLabel)
		{
			var width = useLabel ? EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - RectHelper.IndentWidth - RectHelper.HorizontalPadding : EditorGUIUtility.currentViewWidth;
			GetButtonInfo(width, count, out float buttonWidth, out int rows, out int columns);
			return EditorGUIUtility.singleLineHeight * rows;
		}

		public static int Draw(GUIContent label, int value, Type type, int count)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight(count, label != null));
			return Draw(rect, label, value, type);
		}

		public static int Draw(Rect position, GUIContent label, int value, Type type)
		{
			var rect = position;
			if (label != null)
				rect = EditorGUI.PrefixLabel(position, label);

			var flags = TypeHelper.GetAttribute<FlagsAttribute>(type) != null;
			var values = Enum.GetValues(type);
			var names = Enum.GetNames(type).Select(name => new GUIContent(name)).ToArray();

			GetButtonInfo(rect.width, values.Length, out float buttonWidth, out int rows, out int columns);
			return flags ? DrawButtonFlags(rect, buttonWidth, rows, columns, value, values, names) : DrawButtons(rect, buttonWidth, rows, columns, value, values, names);
		}

		public static void Draw(SerializedProperty property, GUIContent label, Type type, int count)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight(count, label != null));
			Draw(rect, property, label, type);
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label, Type type)
		{
			if (property.propertyType == SerializedPropertyType.Enum)
			{
				property.intValue = Draw(position, label, property.intValue, type);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		#endregion

		#region Virtual Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Enum)
			{
				return GetHeight(Enum.GetValues(fieldInfo.FieldType).Length, label != null);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				return EditorGUI.GetPropertyHeight(property);
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = GuiHelper.GetTooltip(fieldInfo);
			Draw(position, property, label, fieldInfo.FieldType);
		}

		#endregion

		#region Drawing

		private static void GetButtonInfo(float width, int count, out float buttonWidth, out int rows, out int columns)
		{
			if (width < _minWidth)
			{
				buttonWidth = width;
				rows = count;
				columns = 1;
				return;
			}

			rows = 1;
			columns = count;
			buttonWidth = width / count;

			while (buttonWidth < _minWidth)
			{
				rows++;
				columns = Mathf.CeilToInt(count / (float)rows);
				buttonWidth = width / columns;
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
