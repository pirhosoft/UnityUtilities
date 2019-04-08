using PiRhoSoft.UtilityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(EnumDisplayAttribute))]
	public class EnumDisplayDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "(UEDDIT) Invalid type for EnumDisplay on field {0}: EnumDisplay can only be used with to Enum fields";
		private const string _invalidEnumWarning = "(UEDDIE) Invalid type for EnumDisplay: type {0} is not an enum";

		#region Static Property Interface

		public static void Draw(GUIContent label, SerializedProperty property, Type enumType, EnumDisplayType type, bool forceFlags, float minimumButtonWidth = EnumDisplayAttribute.DefaultMinimumWidth)
		{
			var height = GetHeight(label, enumType, type, minimumButtonWidth);
			var rect = EditorGUILayout.GetControlRect(false, height);

			Draw(rect, label, property, enumType, type, forceFlags, minimumButtonWidth);
		}

		public static void Draw(Rect position, GUIContent label, SerializedProperty property, Type enumType, EnumDisplayType type, bool forceFlags, float minimumButtonWidth = EnumDisplayAttribute.DefaultMinimumWidth)
		{
			if (property.propertyType == SerializedPropertyType.Enum)
			{
				property.intValue = Draw(position, label, property.intValue, enumType, type, forceFlags, minimumButtonWidth);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		#endregion

		#region Static Generic Interface

		public static float GetHeight<EnumType>(GUIContent label, EnumDisplayType type, float minimumButtonWidth = EnumDisplayAttribute.DefaultMinimumWidth) where EnumType : Enum
		{
			return GetHeight(label, typeof(EnumType), type, minimumButtonWidth);
		}

		public static int Draw<EnumType>(GUIContent label, int value, EnumDisplayType type, bool forceFlags, float minimumButtonWidth = EnumDisplayAttribute.DefaultMinimumWidth) where EnumType : Enum
		{
			return Draw(label, value, typeof(EnumType), type, forceFlags, minimumButtonWidth);
		}

		public static int Draw<EnumType>(Rect position, GUIContent label, int value, EnumDisplayType type, bool forceFlags, float minimumButtonWidth = EnumDisplayAttribute.DefaultMinimumWidth) where EnumType : Enum
		{
			return Draw(position, label, value, typeof(EnumType), type, forceFlags, minimumButtonWidth);
		}

		#endregion

		#region Static Type Interface

		public static float GetHeight(GUIContent label, Type enumType, EnumDisplayType type, float minimumButtonWidth = EnumDisplayAttribute.DefaultMinimumWidth)
		{
			if (type == EnumDisplayType.Buttons)
			{
				var width = label.text != string.Empty ? RectHelper.CurrentFieldWidth : RectHelper.CurrentViewWidth;
				GetButtonInfo(enumType, width, minimumButtonWidth, out var info, out float buttonWidth, out int rows, out int columns);
				return EditorGUIUtility.singleLineHeight * rows;
			}
			else
			{
				return EditorGUIUtility.singleLineHeight;
			}
		}

		public static int Draw(GUIContent label, int value, Type enumType, EnumDisplayType type, bool forceFlags, float minimumButtonWidth = EnumDisplayAttribute.DefaultMinimumWidth)
		{
			var height = GetHeight(label, enumType, type, minimumButtonWidth);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return Draw(rect, label, value, enumType, type, forceFlags, minimumButtonWidth);
		}

		public static int Draw(Rect position, GUIContent label, int value, Type enumType, EnumDisplayType type, bool forceFlags, float minimumButtonWidth = EnumDisplayAttribute.DefaultMinimumWidth)
		{
			var width = label.text != string.Empty ? RectHelper.CurrentFieldWidth : RectHelper.CurrentViewWidth;
			GetButtonInfo(enumType, width, minimumButtonWidth, out var info, out var buttonWidth, out var rows, out var columns);

			var useFlags = forceFlags || info.IsFlags;

			switch (type)
			{
				case EnumDisplayType.Buttons:
				{
					position = EditorGUI.PrefixLabel(position, label);

					if (useFlags)
						value = DrawFlagButtons(position, value, info, buttonWidth, rows, columns);
					else
						value = DrawSingleButtons(position, value, info, buttonWidth, rows, columns);

					break;
				}
				case EnumDisplayType.Popup:
				{
					var enumValue = (Enum)Enum.Parse(enumType, value.ToString());

					if (useFlags)
						enumValue = EditorGUI.EnumFlagsField(position, label, enumValue);
					else
						enumValue = EditorGUI.EnumPopup(position, label, enumValue);

					value = Convert.ToInt32(enumValue);
					break;
				}
			}

			return value;
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Enum)
			{
				var enumDisplay = attribute as EnumDisplayAttribute;
				return GetHeight(label, fieldInfo.FieldType, enumDisplay.Type, enumDisplay.MinimumWidth);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				return EditorGUI.GetPropertyHeight(property, label);
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var enumDisplay = attribute as EnumDisplayAttribute;

			label.tooltip = Label.GetTooltip(fieldInfo);
			Draw(position, label, property, fieldInfo.FieldType, enumDisplay.Type, enumDisplay.ForceFlags, enumDisplay.MinimumWidth);
		}

		#endregion

		#region Implementation

		private class EnumInfo
		{
			public int Count;
			public bool IsFlags;
			public GUIContent[] Names;
			public Array Values;
		}

		private static Dictionary<Type, EnumInfo> _infos = new Dictionary<Type, EnumInfo>();

		private static void GetButtonInfo(Type enumType, float width, float minimumButtonWidth, out EnumInfo info, out float buttonWidth, out int rows, out int columns)
		{
			if (!_infos.TryGetValue(enumType, out info))
			{
				info = new EnumInfo();

				if (enumType.IsEnum)
				{
					info.Names = enumType.GetEnumNames().Select(name => new GUIContent(name)).ToArray();
					info.Values = enumType.GetEnumValues();
					info.Count = info.Values.Length;
					info.IsFlags = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
				}
				else
				{
					Debug.LogWarningFormat(_invalidEnumWarning, enumType.Name);
					info.Names = new GUIContent[0];
					info.Values = new Enum[0];
					info.Count = 0;
					info.IsFlags = false;
				}

				_infos.Add(enumType, info);
			}

			if (width < minimumButtonWidth)
			{
				buttonWidth = width;
				rows = info.Count;
				columns = 1;
			}
			else
			{
				rows = 1;
				columns = info.Count;
				buttonWidth = width / info.Count;

				while (buttonWidth < minimumButtonWidth)
				{
					rows++;
					columns = Mathf.CeilToInt(info.Count / (float)rows);
					buttonWidth = width / columns;
				}
			}
		}

		private static int DrawSingleButtons(Rect position, int value, EnumInfo info, float buttonWidth, int rows, int columns)
		{
			var index = 0;
			for (var row = 0; row < rows; row++)
			{
				var rowRect = RectHelper.TakeHeight(ref position, EditorGUIUtility.singleLineHeight);

				for (var i = 0; i < columns && index < info.Values.Length; i++, index++)
				{
					var enumValue = (int)info.Values.GetValue(index);
					var pressed = value == enumValue;
					var rect = RectHelper.TakeWidth(ref rowRect, buttonWidth);

					if (GUI.Toggle(rect, pressed, info.Names[index], GUI.skin.button))
						value = enumValue;
				}
			}

			return value;
		}

		private static int DrawFlagButtons(Rect position, int value, EnumInfo info, float buttonWidth, int rows, int columns)
		{
			var index = 0;
			for (var row = 0; row < rows; row++)
			{
				var rowRect = RectHelper.TakeHeight(ref position, EditorGUIUtility.singleLineHeight);

				for (var i = 0; i < columns && index < info.Values.Length; i++, index++)
				{
					var enumValue = (int)info.Values.GetValue(index);
					var pressed = ((value & enumValue) == enumValue && enumValue != 0) || (value == 0 && enumValue == 0);
					var rect = RectHelper.TakeWidth(ref rowRect, buttonWidth);

					if (GUI.Toggle(rect, pressed, info.Names[index], GUI.skin.button))
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
