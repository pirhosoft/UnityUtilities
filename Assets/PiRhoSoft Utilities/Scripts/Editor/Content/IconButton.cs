using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class IconButton : StaticContent
	{
		public const string Add = "Toolbar Plus";
		public const string CustomAdd = "Toolbar Plus More";
		public const string Remove = "Toolbar Minus";
		public const string Edit = "UnityEditor.InspectorWindow";
		public const string Expanded = "IN foldout focus on";
		public const string Collapsed = "IN foldout focus";
		public const string Refresh = "d_preAudioLoopOff";
		public const string Load = "SceneLoadIn";
		public const string Unload = "SceneLoadOut";

		public string IconName;
		public string Tooltip;

		public IconButton(string iconName, string tooltip = "")
		{
			IconName = iconName;
			Tooltip = tooltip;
		}

		protected override GUIContent Create()
		{
			var image = !string.IsNullOrEmpty(IconName) ? EditorGUIUtility.IconContent(IconName)?.image : null;
			return new GUIContent(image, Tooltip);
		}
	}
}
