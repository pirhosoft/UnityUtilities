using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class TextButton : StaticContent
	{
		public static readonly TextButton CreateButton = new TextButton("Create", "Create this item", IconButton.AddName);

		public string Label;
		public string Tooltip;
		public string IconName;

		public TextButton(string title, string tooltip = "", string iconName = "")
		{
			Label = title;
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
