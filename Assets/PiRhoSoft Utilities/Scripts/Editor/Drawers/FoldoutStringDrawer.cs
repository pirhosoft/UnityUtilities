using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(FoldoutString))]
	public class FoldoutStringDrawer : PropertyDrawer
	{
		private static readonly IconButton _collapseButton = new IconButton(IconButton.Expanded, "Collapse this text box");
		private static readonly IconButton _expandButton = new IconButton(IconButton.Collapsed, "Expand this text box");

		private const float _defaultExpandedHeight = 100.0f;

		public static float GetHeight(bool expanded)
		{
			return expanded ? _defaultExpandedHeight : EditorGUIUtility.singleLineHeight;
		}

		public static string Draw(GUIContent label, string text, ref bool expanded)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight(expanded));
			return Draw(rect, label, text, ref expanded);
		}

		public static string Draw(Rect position, GUIContent label, string text, ref bool expanded)
		{
			var rect = EditorGUI.PrefixLabel(position, label);
			var iconRect = RectHelper.AdjustHeight(RectHelper.TakeLeadingIcon(ref rect), EditorGUIUtility.singleLineHeight, RectVerticalAlignment.Top);

			using (ColorScope.Color(new Color(0.3f, 0.3f, 0.3f)))
			{
				var button = expanded ? _collapseButton : _expandButton;

				if (GUI.Button(iconRect, button.Content, GUIStyle.none))
					expanded = !expanded;
			}

			if (expanded)
			{
				text = EditorGUI.TextArea(rect, text);
			}
			else
			{
				var textRect = RectHelper.TakeLine(ref rect);
				text = EditorGUI.TextField(textRect, text);
			}

			return text;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var expanded = PropertyHelper.GetSibling(property, nameof(FoldoutString.IsExpanded));
			return GetHeight(expanded.boolValue);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			var str = property.FindPropertyRelative(nameof(FoldoutString.String));
			var expanded = property.FindPropertyRelative(nameof(FoldoutString.IsExpanded));

			var isExpanded = expanded.boolValue;

			Draw(position, label, str.stringValue, ref isExpanded);

			expanded.boolValue = isExpanded;
		}
	}
}
