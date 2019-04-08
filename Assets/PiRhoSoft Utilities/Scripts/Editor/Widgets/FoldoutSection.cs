using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class FoldoutSection
	{
		private static readonly Label _collapseButton = new Label(Icon.BuiltIn(Icon.Expanded), string.Empty, "Collapse");
		private static readonly Label _expandButton = new Label(Icon.BuiltIn(Icon.Collapsed), string.Empty, "Expand");

		public static bool Draw(Rect position, GUIContent label, bool isExpanded)
		{
			var foldoutRect = RectHelper.TakeLeadingIcon(ref position);

			foldoutRect.y += 1.0f; // this looks better than centering (which would be 1.5 for a standard line)

			if (GUI.Button(foldoutRect, isExpanded ? _collapseButton.Content : _expandButton.Content, GUIStyle.none))
				isExpanded = !isExpanded;

			EditorGUI.LabelField(position, label);
			return isExpanded;
		}
	}
}
