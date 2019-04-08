using PiRhoSoft.UtilityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(StringDisplayAttribute))]
	class StringDisplayDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "(USDDIT) Invalid type for StringDisplay on field {0}: StringDisplay can only be used with string fields";

		private static readonly GUIContent _temporaryContent = new GUIContent();
		private static NewStyle _wrappingTextBoxStyle = new NewStyle { WordWrap = true };
		private static NewStyle _nonWrappingTextAreaStyle = new NewStyle { WordWrap = false };

		private static Dictionary<int, Vector2> _scrollPositions = new Dictionary<int, Vector2>();

		#region Internal Lookup

		private delegate string ScrollableTextAreaInternalSignature(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style);
		private static ScrollableTextAreaInternalSignature ScrollableTextAreaInternal;

		static StringDisplayDrawer()
		{
			ScrollableTextAreaInternal = InternalHelper.CreateDelegate<ScrollableTextAreaInternalSignature>(typeof(EditorGUI), nameof(ScrollableTextAreaInternal));
		}

		#endregion

		#region Static Height Interface

		private static float GetTextHeight(string value, bool fullWidth, bool wordWrap, int minimumLines, int maximumLines, GUIStyle style)
		{
			var labelHeight = fullWidth ? RectHelper.LineHeight : 0.0f;

			// adopted from the implementation of TextAreaDrawer.GetPropertyHeight

			_temporaryContent.text = value;

			var textHeight = style.CalcHeight(_temporaryContent, RectHelper.CurrentContextWidth);
			var rows = Mathf.CeilToInt(textHeight / 13.0f);

			rows = Mathf.Clamp(rows, minimumLines, maximumLines);

			return labelHeight + (rows * 13.0f) + style.padding.top + style.padding.bottom;
		}

		#region Field

		public static float GetFieldHeight(bool fullWidth)
		{
			var labelHeight = fullWidth ? RectHelper.LineHeight : 0.0f;
			var textHeight = EditorGUIUtility.singleLineHeight;

			return labelHeight + textHeight;
		}

		public static float GetFoldoutFieldHeight(bool isExpanded)
		{
			return GetFieldHeight(isExpanded);
		}

		#endregion

		#region Box

		public static float GetBoxHeight(string value, bool fullWidth, bool wordWrap, int minimumLines, int maximumLines)
		{
			var style = wordWrap ? _wrappingTextBoxStyle.GetContent(EditorStyles.textField) : EditorStyles.textField;
			return GetTextHeight(value, fullWidth, wordWrap, minimumLines, maximumLines, style);
		}

		public static float GetFoldoutBoxHeight(string value, bool isExpanded, bool fullWidth, bool wordWrap, int minimumLines, int maximumLines)
		{
			return isExpanded ? GetBoxHeight(value, fullWidth, wordWrap, minimumLines, maximumLines) : GetFieldHeight(false);
		}

		#endregion

		#region Area

		public static float GetAreaHeight(string value, bool fullWidth, bool wordWrap, int minimumLines, int maximumLines)
		{
			var style = wordWrap ? EditorStyles.textArea : _nonWrappingTextAreaStyle.GetContent(EditorStyles.textArea);
			return GetTextHeight(value, fullWidth, wordWrap, minimumLines, maximumLines, style);
		}

		public static float GetFoldoutAreaHeight(string value, bool isExpanded, bool fullWidth, bool wordWrap, int minimumLines, int maximumLines)
		{
			return isExpanded ? GetAreaHeight(value, fullWidth, wordWrap, minimumLines, maximumLines) : GetFieldHeight(false);
		}

		#endregion

		#region Popup

		public static float GetPopupHeight(bool fullWidth)
		{
			var labelHeight = fullWidth ? RectHelper.LineHeight : 0.0f;
			var textHeight = EditorGUIUtility.singleLineHeight;

			return labelHeight + textHeight;
		}

		public static float GetFoldoutPopupHeight(bool isExpanded)
		{
			return GetPopupHeight(isExpanded);
		}

		#endregion

		#endregion

		#region Static Property Draw Interface

		#region Field

		public static void DrawField(Rect position, GUIContent label, SerializedProperty property, bool fullWidth)
		{
			property.stringValue = DrawField(position, label, property.stringValue, fullWidth);
		}

		public static void DrawFoldoutField(Rect position, GUIContent label, SerializedProperty property)
		{
			var isExpanded = property.isExpanded;

			property.stringValue = DrawFoldoutField(position, label, property.stringValue, ref isExpanded);
			property.isExpanded = isExpanded;
		}

		#endregion

		#region Box

		public static void DrawBox(Rect position, GUIContent label, SerializedProperty property, bool fullWidth, bool wordWrap)
		{
			property.stringValue = DrawBox(position, label, property.stringValue, fullWidth, wordWrap);
		}

		public static void DrawFoldoutBox(Rect position, GUIContent label, SerializedProperty property, bool fullWidth, bool wordWrap)
		{
			var isExpanded = property.isExpanded;

			property.stringValue = DrawFoldoutBox(position, label, property.stringValue, ref isExpanded, fullWidth, wordWrap);
			property.isExpanded = isExpanded;
		}

		#endregion

		#region Area

		public static void DrawArea(Rect position, GUIContent label, SerializedProperty property, bool fullWidth, bool wordWrap)
		{
			property.stringValue = DrawArea(position, label, property.stringValue, fullWidth, wordWrap);
		}

		public static void DrawFoldoutArea(Rect position, GUIContent label, SerializedProperty property, bool fullWidth, bool wordWrap)
		{
			var isExpanded = property.isExpanded;

			property.stringValue = DrawFoldoutArea(position, label, property.stringValue, ref isExpanded, fullWidth, wordWrap);
			property.isExpanded = isExpanded;
		}

		#endregion

		#region Popup

		public static void DrawPopup(Rect position, GUIContent label, SerializedProperty property, bool fullWidth, string[] options)
		{
			property.stringValue = DrawPopup(position, label, property.stringValue, fullWidth, options);
		}

		public static void DrawFoldoutPopup(Rect position, GUIContent label, SerializedProperty property, string[] options)
		{
			var isExpanded = property.isExpanded;

			property.stringValue = DrawFoldoutPopup(position, label, property.stringValue, ref isExpanded, options);
			property.isExpanded = isExpanded;
		}

		#endregion

		#endregion

		#region Static Object Draw Interface

		#region Field

		public static string DrawField(Rect position, GUIContent label, string value, bool fullWidth)
		{
			var id = GUIUtility.GetControlID(label, FocusType.Passive, position);

			if (fullWidth)
			{
				var labelRect = RectHelper.TakeLine(ref position);
				EditorGUI.LabelField(labelRect, label);

				using (new EditorGUI.IndentLevelScope())
					return EditorGUI.TextField(position, value);
			}
			else
			{
				return EditorGUI.TextField(position, label, value);
			}
		}

		public static string DrawFoldoutField(Rect position, GUIContent label, string value, ref bool isExpanded)
		{
			var lineRect = RectHelper.TakeLine(ref position);
			var labelRect = isExpanded ? lineRect : RectHelper.TakeLabel(ref lineRect);

			isExpanded = FoldoutSection.Draw(labelRect, label, isExpanded);

			if (isExpanded)
				return EditorGUI.TextField(position, value);
			else
				return EditorGUI.TextField(lineRect, value);
		}

		#endregion

		#region Box

		public static string DrawBox(Rect position, GUIContent label, string value, bool fullWidth, bool wordWrap)
		{
			var style = wordWrap ? _wrappingTextBoxStyle.GetContent(EditorStyles.textField) : EditorStyles.textField;

			if (fullWidth)
			{
				var lineRect = RectHelper.TakeLine(ref position);
				EditorGUI.LabelField(lineRect, label);

				using (new EditorGUI.IndentLevelScope())
					return EditorGUI.TextArea(position, value, style);
			}
			else
			{
				position = EditorGUI.PrefixLabel(position, label);
				return EditorGUI.TextArea(position, value, style);
			}
		}

		public static string DrawFoldoutBox(Rect position, GUIContent label, string value, ref bool isExpanded, bool fullWidth, bool wordWrap)
		{
			var fullRect = position;
			var lineRect = RectHelper.TakeLine(ref position);
			var labelRect = !fullWidth || !isExpanded ? RectHelper.TakeLabel(ref lineRect) : lineRect;

			isExpanded = FoldoutSection.Draw(labelRect, label, isExpanded);

			if (isExpanded)
			{
				var style = wordWrap ? _wrappingTextBoxStyle.GetContent(EditorStyles.textField) : EditorStyles.textField;

				if (fullWidth)
				{
					using (new EditorGUI.IndentLevelScope())
						return EditorGUI.TextArea(position, value, style);
				}
				else
				{
					RectHelper.TakeLabel(ref fullRect);
					return EditorGUI.TextArea(fullRect, value, style);
				}
			}
			else
			{
				return EditorGUI.TextField(lineRect, value);
			}
		}

		#endregion

		#region Area

		public static string DrawArea(Rect position, GUIContent label, string value, bool fullWidth, bool wordWrap)
		{
			if (fullWidth)
			{
				var lineRect = RectHelper.TakeLine(ref position);
				EditorGUI.LabelField(lineRect, label);

				using (new EditorGUI.IndentLevelScope())
					return DrawArea(position, value, wordWrap);
			}
			else
			{
				position = EditorGUI.PrefixLabel(position, label);
				return DrawArea(position, value, wordWrap);
			}
		}

		public static string DrawFoldoutArea(Rect position, GUIContent label, string value, ref bool isExpanded, bool fullWidth, bool wordWrap)
		{
			var fullRect = position;
			var lineRect = RectHelper.TakeLine(ref position);
			var labelRect = !fullWidth || !isExpanded ? RectHelper.TakeLabel(ref lineRect) : lineRect;

			isExpanded = FoldoutSection.Draw(labelRect, label, isExpanded);

			if (isExpanded)
			{
				if (fullWidth)
				{
					using (new EditorGUI.IndentLevelScope())
						return DrawArea(position, value, wordWrap);
				}
				else
				{
					RectHelper.TakeLabel(ref fullRect);
					return DrawArea(fullRect, value, wordWrap);
				}
			}
			else
			{
				return EditorGUI.TextField(lineRect, value);
			}
		}

		private static string DrawArea(Rect position, string value, bool wordWrap)
		{
			var id = GUIUtility.GetControlID(FocusType.Passive, position);
			var scrollPosition = _scrollPositions.TryGetValue(id, out var scroll) ? scroll : Vector2.zero;

			var style = wordWrap ? EditorStyles.textArea : _nonWrappingTextAreaStyle.GetContent(EditorStyles.textArea);
			var result = ScrollableTextAreaInternal(position, value, ref scrollPosition, style);
			_scrollPositions[id] = scrollPosition;
			return result;
		}

		#endregion

		#region Popup

		public static string DrawPopup(Rect position, GUIContent label, string value, bool fullWidth, string[] options)
		{
			if (fullWidth)
			{
				var labelRect = RectHelper.TakeLine(ref position);
				EditorGUI.LabelField(labelRect, label);

				using (new EditorGUI.IndentLevelScope())
					return DrawPopup(position, value, options);
			}
			else
			{
				position = EditorGUI.PrefixLabel(position, label);
				return DrawPopup(position, value, options);
			}
		}

		public static string DrawFoldoutPopup(Rect position, GUIContent label, string value, ref bool isExpanded, string[] options)
		{
			var lineRect = RectHelper.TakeLine(ref position);
			var labelRect = isExpanded ? lineRect : RectHelper.TakeLabel(ref lineRect);

			isExpanded = FoldoutSection.Draw(labelRect, label, isExpanded);

			if (isExpanded)
				return DrawPopup(position, value, options);
			else
				return DrawPopup(lineRect, value, options);
		}

		private static string DrawPopup(Rect position, string value, string[] options)
		{
			var index = Array.IndexOf(options, value);
			var labels = options.Select(option => new GUIContent(option)).ToArray();

			index = EditorGUI.Popup(position, index, labels);

			return index >= 0 ? options[index] : value;
		}

		#endregion

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.String)
			{
				var stringDisplay = attribute as StringDisplayAttribute;

				if (stringDisplay.Foldout)
				{
					switch (stringDisplay.Type)
					{
						case StringDisplayType.Field: return GetFoldoutFieldHeight(property.isExpanded);
						case StringDisplayType.Box: return GetFoldoutBoxHeight(property.stringValue, property.isExpanded, stringDisplay.FullWidth, stringDisplay.WordWrap, stringDisplay.MinimumLines, stringDisplay.MaximumLines);
						case StringDisplayType.Area: return GetFoldoutAreaHeight(property.stringValue, property.isExpanded, stringDisplay.FullWidth, stringDisplay.WordWrap, stringDisplay.MinimumLines, stringDisplay.MaximumLines);
						case StringDisplayType.Popup: return GetFoldoutPopupHeight(property.isExpanded);
					}
				}
				else
				{
					switch (stringDisplay.Type)
					{
						case StringDisplayType.Field: return GetFieldHeight(stringDisplay.FullWidth);
						case StringDisplayType.Box: return GetBoxHeight(property.stringValue, stringDisplay.FullWidth, stringDisplay.WordWrap, stringDisplay.MinimumLines, stringDisplay.MaximumLines);
						case StringDisplayType.Area: return GetAreaHeight(property.stringValue, stringDisplay.FullWidth, stringDisplay.WordWrap, stringDisplay.MinimumLines, stringDisplay.MaximumLines);
						case StringDisplayType.Popup: return GetPopupHeight(stringDisplay.FullWidth);
					}
				}
			}

			return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			if (property.propertyType == SerializedPropertyType.String)
			{
				var stringDisplay = attribute as StringDisplayAttribute;

				if (stringDisplay.Foldout)
				{
					switch (stringDisplay.Type)
					{
						case StringDisplayType.Field: DrawFoldoutField(position, label, property); break;
						case StringDisplayType.Box: DrawFoldoutBox(position, label, property, stringDisplay.FullWidth, stringDisplay.WordWrap); break;
						case StringDisplayType.Area: DrawFoldoutArea(position, label, property, stringDisplay.FullWidth, stringDisplay.WordWrap); break;
						case StringDisplayType.Popup: DrawFoldoutPopup(position, label, property, stringDisplay.Options); break;
					}
				}
				else
				{
					switch (stringDisplay.Type)
					{
						case StringDisplayType.Field: DrawField(position, label, property, stringDisplay.FullWidth); break;
						case StringDisplayType.Box: DrawBox(position, label, property, stringDisplay.FullWidth, stringDisplay.WordWrap); break;
						case StringDisplayType.Area: DrawArea(position, label, property, stringDisplay.FullWidth, stringDisplay.WordWrap); break;
						case StringDisplayType.Popup: DrawPopup(position, label, property, stringDisplay.FullWidth, stringDisplay.Options); break;
					}
				}
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		#endregion
	}
}
