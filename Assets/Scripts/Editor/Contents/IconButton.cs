using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class IconButton : StaticContent
	{
		public const string AddName = "Toolbar Plus";
		public const string CustomAddName = "Toolbar Plus More";
		public const string RemoveName = "Toolbar Minus";
		public const string EditName = "UnityEditor.InspectorWindow";
		public const string RefreshName = "d_preAudioLoopOff";
		public const string LoadName = "SceneLoadIn";
		public const string UnloadName = "SceneLoadOut";
		public const string ExpandedName = "IN foldout focus on";
		public const string CollapsedName = "IN foldout focus";

		public static readonly IconButton Add = new IconButton(AddName, "Add an item");
		public static readonly IconButton CustomAdd = new IconButton(CustomAddName, "Add an item");
		public static readonly IconButton Edit = new IconButton(EditName, "Edit this item");
		public static readonly IconButton Remove = new IconButton(RemoveName, "Remove this item");
		public static readonly IconButton Refresh = new IconButton(RefreshName, "Refresh these items");
		public static readonly IconButton Collapse = new IconButton(ExpandedName, "Collapse this control");
		public static readonly IconButton Expand = new IconButton(CollapsedName, "Expand this control");
		public static readonly IconButton Load = new IconButton(LoadName, "Load this item");
		public static readonly IconButton Unload = new IconButton(UnloadName, "Unload this item");

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
