using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class TextButton : StaticContent
	{
		public string Label;
		public string Tooltip;
		public string IconName;

		public TextButton(string label, string tooltip = "", string iconName = "")
		{
			Label = label;
			Tooltip = tooltip;
			IconName = iconName;
		}

		protected override GUIContent Create()
		{
			var image = !string.IsNullOrEmpty(IconName) ? EditorGUIUtility.IconContent(IconName)?.image : null;
			return new GUIContent(Label, image, Tooltip);
		}
	}
}
