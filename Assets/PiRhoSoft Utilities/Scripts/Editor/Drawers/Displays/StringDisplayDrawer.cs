using PiRhoSoft.UtilityEngine;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(StringDisplayAttribute))]
	class StringDisplayDrawer : PropertyDrawer
	{
		private static readonly GUIContent _temporaryContent = new GUIContent();
		private const string _invalidTypeWarning = "Invalid type for StringDisplay on field {0}: StringDisplay can only be used with string fields";

		#region Static Object Interface

		public float GetTextHeight(GUIContent label, string value, int minimumLines, int maximumLines)
		{
			// taken from the implementation of TextAreaDrawer.GetPropertyHeight

			_temporaryContent.text = value;

			var height = EditorStyles.textArea.CalcHeight(_temporaryContent, RectHelper.CurrentContextWidth);
			var rows = Mathf.CeilToInt(height / 13.0f);

			rows = Mathf.Clamp(rows, minimumLines, maximumLines) - 1;

			return 32.0f + (rows * 13.0f);
		}

		public float GetFoldoutHeight(GUIContent label, string value, bool expanded, int minimumLines, int maximumLines)
		{
			if (expanded)
				return GetTextHeight(label, value, minimumLines, maximumLines);
			else
				return EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);
		}

		public float GetPopupHeight(GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		//public string DrawTextBox(Rect position, GUIContent label, string value, bool wordWrap)
		//{
		//	position = EditorGUI.PrefixLabel(position, label);
		//
		//	return EditorGUI.TextArea(position, value, style);
		//}
		//
		//public string DrawTextArea(Rect position, GUIContent label, string value, bool wordWrap)
		//{
		//	Rect labelPosition = EditorGUI.IndentedRect(position);
		//	labelPosition.height = 16f;
		//	position.yMin += labelPosition.height;
		//	EditorGUI.HandlePrefixLabel(position, labelPosition, label);
		//
		//	return EditorGUI.ScrollableTextAreaInternal(position, value, ref _scrollPosition, EditorStyles.textArea);
		//}
		//
		//public string DrawFoldoutBox()
		//{
		//}
		//
		//public string DrawFoldoutArea()
		//{
		//}

		public string DrawPopup(Rect position, GUIContent label, string value, string[] options)
		{
			var index = Array.IndexOf(options, value);
			var labels = options.Select(option => new GUIContent(option)).ToArray();

			index = EditorGUI.Popup(position, label, index, labels);

			return index >= 0 ? options[index] : value;
		}

		#endregion

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var stringDisplay = attribute as StringDisplayAttribute;

			switch (stringDisplay.Type)
			{
				case StringDisplayType.TextBox: return GetTextHeight(label, property.stringValue, stringDisplay.MinimumLines, stringDisplay.MaximumLines);
			}

			return GetPopupHeight(label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			if (property.propertyType == SerializedPropertyType.String)
			{
				var options = (attribute as StringDisplayAttribute).Options;
				var index = EditorGUI.Popup(position, label, Array.IndexOf(options, property.stringValue), options.Select(option => new GUIContent(option)).ToArray());
				if (index >= 0 && index < options.Length)
					property.stringValue = options[index];
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}
	}
}
