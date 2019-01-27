using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(FoldoutString))]
	public class FoldoutStringDrawer : PropertyDrawer
	{
		public static readonly IconButton _collapseButton = new IconButton(IconButton.Expanded, "Collapse this text box");
		public static readonly IconButton _expandButton = new IconButton(IconButton.Collapsed, "Expand this text box");

		public const float DefaultExpandedHeight = 100.0f;

		public static float GetHeight(bool expanded, float expandedHeight)
		{
			return expanded ? expandedHeight : EditorGUIUtility.singleLineHeight;
		}

		public static string Draw(Rect position, GUIContent label, string text, ref bool expanded)
		{
			position = EditorGUI.PrefixLabel(position, label);

			var iconRect = RectHelper.AdjustHeight(RectHelper.TakeLeadingIcon(ref position), EditorGUIUtility.singleLineHeight, RectVerticalAlignment.Top);

			if (expanded)
			{
				text = EditorGUI.TextArea(position, text);
			}
			else
			{
				position = RectHelper.TakeLine(ref position);
				text = EditorGUI.TextField(position, text);
			}

			using (ColorScope.Color(new Color(0.3f, 0.3f, 0.3f)))
			{
				var button = expanded ? _collapseButton : _expandButton;

				if (GUI.Button(iconRect, button.Content, GUIStyle.none))
					expanded = !expanded;
			}

			return text;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var attribute = TypeHelper.GetAttribute<FoldoutStringAttribute>(fieldInfo);
			var expanded = PropertyHelper.GetSibling(property, nameof(FoldoutString.IsExpanded));

			return GetHeight(expanded.boolValue, attribute != null ? attribute.ExpandedHeight : DefaultExpandedHeight);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var str = property.FindPropertyRelative(nameof(FoldoutString.String));
			var expanded = property.FindPropertyRelative(nameof(FoldoutString.IsExpanded));

			var isExpanded = expanded.boolValue;

			property.stringValue = Draw(position, label, property.stringValue, ref isExpanded);

			expanded.boolValue = isExpanded;
		}
	}
}
